// $Id$
//
// Copyright 2003-2009 The AnkhSVN Project
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
using System.Diagnostics;
using System.Web;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Specialized;
using System.Collections;
using System.Text;

namespace Ankh.Services
{
    /// <summary>
    /// Performs error handling and reporting.
    /// </summary>
    public static class AnkhErrorMessage
    {
        /// <summary>
        /// Concatenates the error messages and exception types from (potentially)
        /// nested exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetMessage(Exception ex)
        {
#if true
            // According to https://stackoverflow.com/questions/2176707/exception-message-vs-exception-tostring
            // the Exception.ToString provides all this and more. In particular, it includes the parameter text
            // provided to the exception. For example for ArgumentNullException it includes the name of the paramter.
            return ex.ToString() ;
#else
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.GetType().FullName + ": ");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);

                ex = ex.InnerException;
            }

            return sb.ToString();
#endif
        }

        /// <summary>
        /// Sends an error message by opening the user's mail client.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="subject"></param>
        /// <param name="ex"></param>
        /// <param name="assembly">The assembly where the error originated. This will
        /// be used to extract version information.</param>
        public static void SendByMail(string recipient, string subject, Exception ex,
            Assembly assembly, StringDictionary additionalInfo)
        {
            string attributes = GetAttributes(additionalInfo);

            StringBuilder msg = new StringBuilder();

            msg.AppendLine(GetMessage(ex));
            msg.AppendLine();
            msg.AppendLine(GetAttributes(additionalInfo));
            msg.AppendLine();

            // Send mail using MAPI class instead of the mailto command, because mailto truncates
            // the message text, probably removing all of the additional information :(
            var MapiMail = new MAPI() ;
            MapiMail.AddRecipientTo ( recipient ) ;
            MapiMail.SendMailPopup  ( subject, msg.ToString() ) ;
        }

        private static string GetAttributes(StringDictionary additionalInfo)
        {
            if (additionalInfo == null)
                return "";

            StringBuilder builder = new StringBuilder();
            foreach (DictionaryEntry de in additionalInfo)
            {
                builder.AppendFormat("{0}={1}", (string)de.Key, Uri.EscapeDataString((string)de.Value));
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
