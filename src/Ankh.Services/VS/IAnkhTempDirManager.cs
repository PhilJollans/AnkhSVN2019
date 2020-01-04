// $Id$
//
// Copyright 2008 The AnkhSVN Project
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

using System;
using System.Collections.Generic;
using System.Text;

namespace Ankh.VS
{
    public interface IAnkhTempDirManager
    {
        /// <summary>
        /// Gets a temporary directory
        /// </summary>
        /// <returns></returns>
        /// <remarks>The directory is created.</remarks>
        string GetTempDir();

        /// <summary>
        /// Call this method explicitly when the pacakage is disposed in Visual Studio 2019,
        /// because the finalizer in TempDirCollection is no longer called and I can't get
        /// the OnBeginShutdown event to fire.
        /// </summary>
        void RemoveTempDirectories() ;
    }
}
