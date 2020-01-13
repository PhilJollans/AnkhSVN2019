// $Id$
//
// Copyright 2005-2009 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Ankh.Scc;
using Ankh.Scc.UI;
using Ankh.UI.Annotate;
using SharpSvn;
using Ankh.UI.Commands;

namespace Ankh.Commands
{
    /// <summary>
    /// Command to identify which users to blame for which lines.
    /// </summary>
    [SvnCommand(AnkhCommand.ItemAnnotate)]
    [SvnCommand(AnkhCommand.LogAnnotateRevision)]
    [SvnCommand(AnkhCommand.SvnNodeAnnotate)]
    [SvnCommand(AnkhCommand.DocumentAnnotate)]
    class ItemAnnotateCommand : CommandBase
    {
        // AnnotateService is defined as an MEF service.
        [Import]
        public IAnnotateService            AnnotateService { get; set; }

        public ItemAnnotateCommand ()
        {
            string assemblyFolder = Path.GetDirectoryName ( Assembly.GetExecutingAssembly().Location ) ;
            using ( var catalog = new DirectoryCatalog ( assemblyFolder, "*.dll" ) )
            {
                var container = new CompositionContainer(catalog);
                container.SatisfyImportsOnce(this);
            }
        }

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            switch (e.Command)
            {
                case AnkhCommand.SvnNodeAnnotate:
                    ISvnRepositoryItem ri = EnumTools.GetSingle(e.Selection.GetSelection<ISvnRepositoryItem>());
                    if (ri != null && ri.Origin != null && ri.NodeKind != SvnNodeKind.Directory)
                        return;
                    break;
                case AnkhCommand.ItemAnnotate:
                    foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
                    {
                        if (item.IsFile && item.IsVersioned && item.HasCopyableHistory)
                            return;
                    }
                    break;
                case AnkhCommand.DocumentAnnotate:
                    if (e.Selection.ActiveDocumentSvnItem != null && e.Selection.ActiveDocumentSvnItem.HasCopyableHistory)
                        return;
                    break;
                case AnkhCommand.LogAnnotateRevision:
                    ILogControl logControl = e.Selection.GetActiveControl<ILogControl>();
                    if (logControl == null || logControl.Origins == null)
                    {
                        e.Visible = e.Enabled = false;
                        return;
                    }

                    if (!EnumTools.IsEmpty(e.Selection.GetSelection<ISvnLogChangedPathItem>()))
                        return;
                    break;
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<SvnOrigin> targets = new List<SvnOrigin>();
            SvnRevision startRev = SvnRevision.Zero;
            SvnRevision endRev = null;
            switch (e.Command)
            {
                case AnkhCommand.ItemAnnotate:
                    endRev = SvnRevision.Working;
                    foreach (SvnItem i in e.Selection.GetSelectedSvnItems(false))
                    {
                        if (i.IsFile && i.IsVersioned && i.HasCopyableHistory)
                            targets.Add(new SvnOrigin(i));
                    }
                    break;
                case AnkhCommand.LogAnnotateRevision:
                    foreach (ISvnLogChangedPathItem logItem in e.Selection.GetSelection<ISvnLogChangedPathItem>())
                    {
                        targets.Add(logItem.Origin);
                        endRev = logItem.Revision;
                    }
                    break;
                case AnkhCommand.SvnNodeAnnotate:
                    foreach (ISvnRepositoryItem item in e.Selection.GetSelection<ISvnRepositoryItem>())
                    {
                        targets.Add(item.Origin);
                        endRev = item.Revision;
                    }
                    break;
                case AnkhCommand.DocumentAnnotate:
                    //TryObtainBlock(e);
                    targets.Add(new SvnOrigin(e.GetService<ISvnStatusCache>()[e.Selection.ActiveDocumentFilename]));
                    endRev = SvnRevision.Working;
                    break;
            }

            if (targets.Count == 0)
                return;

            bool ignoreEols = true;
            SvnIgnoreSpacing ignoreSpacing = SvnIgnoreSpacing.IgnoreSpace;
            bool retrieveMergeInfo = false;
            SvnOrigin target;

            if ((!e.DontPrompt && !Shift) || e.PromptUser)
                using (AnnotateDialog dlg = new AnnotateDialog())
                {
                    dlg.SetTargets(targets);
                    dlg.StartRevision = startRev;
                    dlg.EndRevision = endRev;

                    if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    target = dlg.SelectedTarget;
                    startRev = dlg.StartRevision;
                    endRev = dlg.EndRevision;
                    ignoreEols = dlg.IgnoreEols;
                    ignoreSpacing = dlg.IgnoreSpacing;
                    retrieveMergeInfo = dlg.RetrieveMergeInfo;
                }
            else
            {
                SvnItem one = EnumTools.GetFirst(e.Selection.GetSelectedSvnItems(false));

                if (one == null)
                    return;

                target = new SvnOrigin(one);
            }

            if (startRev == SvnRevision.Working || endRev == SvnRevision.Working && target.Target is SvnPathTarget)
            {
                IAnkhOpenDocumentTracker tracker = e.GetService<IAnkhOpenDocumentTracker>();
                if (tracker != null)
                    tracker.SaveDocument(((SvnPathTarget)target.Target).FullPath);
            }

            AnnotateService.DoBlame ( e, target, startRev, endRev, ignoreEols, ignoreSpacing, retrieveMergeInfo ) ;
        }

    }
}
