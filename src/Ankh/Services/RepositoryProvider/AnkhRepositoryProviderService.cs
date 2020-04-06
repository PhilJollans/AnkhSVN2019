using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Forms;
using Ankh.ExtensionPoints.RepositoryProvider;
using Ankh.UI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace Ankh.Services.RepositoryProvider
{
    //
    // This class formerly derived from AnkhService and had the GlobalService attribute.
    // I have removed the base class and converted it into an MEF component.
    //
    // Although I have CreationPolicy.Shared, the object is not created permanently.
    // In this case that is OK.
    //
    // This is in fact the first service which I have converted to an MEF component, so
    // it is still quite experimental. This specific classs doesn't really need to be a
    // service at all.
    //
    [Export(typeof(IAnkhRepositoryProviderService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class AnkhRepositoryProviderService : IAnkhRepositoryProviderService
    {
        readonly IAnkhServiceProvider                      _context ;
        readonly Dictionary<string, ScmRepositoryProvider> _nameProviderMap;

        [ImportingConstructor]
        public AnkhRepositoryProviderService([Import("AnkhContextParameter")]IAnkhServiceProvider context)
            /* : base(context) */
        {
            _context         = context ;
            _nameProviderMap = new Dictionary<string, ScmRepositoryProvider>();

            // Moved from OnInitialize for MEF construction
            ReadProviderRegistry();
        }

        #region IAnkhRepositoryProviderService Members

        /// <summary>
        /// Gets all registered SCM repository providers
        /// </summary>
        public ICollection<ScmRepositoryProvider> RepositoryProviders
        {
            get
            {
                if (_nameProviderMap != null)
                {
                    ScmRepositoryProvider[] result = new ScmRepositoryProvider[_nameProviderMap.Count];
                    _nameProviderMap.Values.CopyTo(result, 0);
                    return result;
                }
                return new ScmRepositoryProvider[] { };
            }
        }

        /// <summary>
        /// Gets all the registered SCM repository providers for the given SCM type(svn, git).
        /// </summary>
        /// <param name="type">SCM type</param>
        /// <remarks>This call DOES NOT trigger provider package initialization.</remarks>
        public ICollection<ScmRepositoryProvider> GetRepositoryProviders(RepositoryType type)
        {
            ICollection<ScmRepositoryProvider> allProviders = RepositoryProviders;
            List<ScmRepositoryProvider> result = new List<ScmRepositoryProvider>();
            foreach (ScmRepositoryProvider provider in allProviders)
            {
                if (type == RepositoryType.Any || type ==provider.Type)
                {
                    result.Add(provider);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Tries to find a registered provider with the given name.
        /// </summary>
        /// <param name="id">Repository provider's identifier</param>
        /// <param name="repoProvider">[out] Repository provider instance if found</param>
        /// <remarks>This call DOES NOT trigger provider package initialization.</remarks>
        /// <returns>true if the lookup is successful, false otherwise</returns>
        public bool TryGetRepositoryProvider(string id, out ScmRepositoryProvider repoProvider)
        {
            repoProvider = null;
            if (_nameProviderMap != null && _nameProviderMap.Count > 0)
            {
                return _nameProviderMap.TryGetValue(id, out repoProvider);
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Reads the SCM repository provider information from the registry
        /// </summary>
        private void ReadProviderRegistry()
        {
            IAnkhPackage ankhPackage = _context.GetService<IAnkhPackage>();
            if (ankhPackage != null)
            {
                using (RegistryKey key = ankhPackage.ApplicationRegistryRoot)
                {
                    using (RegistryKey aKey = key.OpenSubKey("ScmRepositoryProviders"))
                    {
                        if (aKey == null)
                            return;

                        string[] providerKeys = aKey.GetSubKeyNames();
                        foreach (string providerKey in providerKeys)
                        {
                            using (RegistryKey provider = aKey.OpenSubKey(providerKey))
                            {
                                string serviceName = (string)provider.GetValue("");
                                RepositoryType rt = GetRepositoryType(provider.GetValue("ScmType") as string);
                                ScmRepositoryProvider descriptor = new ScmRepositoryProviderProxy(_context, providerKey, serviceName, rt);
                                if (!_nameProviderMap.ContainsKey(providerKey))
                                {
                                    _nameProviderMap.Add(providerKey, descriptor);
                                }
                            }
                        }
                    }
                }
            }
        }

        private RepositoryType GetRepositoryType(string typeString)
        {
            if (string.Equals(typeString, "svn", StringComparison.OrdinalIgnoreCase))
                return RepositoryType.Subversion;
            if (string.Equals(typeString, "git", StringComparison.OrdinalIgnoreCase))
                return RepositoryType.Git;

            return RepositoryType.Any;
        }
    }
}
