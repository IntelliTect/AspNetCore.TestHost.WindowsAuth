// Copyright 2018 IntelliTect
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth
{
    internal static partial class WindowsIdentityFactory
    {
        // Store credentials keyed by a GUID so that we don't have to serialize actual credentials into HTTP headers.
        private static readonly ConcurrentDictionary<Guid, NetworkCredential> CredentialStore
            = new ConcurrentDictionary<Guid, NetworkCredential>();

        /// <summary>
        ///     Return a GUID that can be lated user to log in as the user represented by the credentials.
        ///     This is passed as an HTTP header to allow a receiving in-memory server to authenticate as this user.
        /// </summary>
        public static Guid GetTokenForCredentials(NetworkCredential creds)
        {
            creds.Domain = creds.Domain ?? Environment.UserDomainName;
            Guid guid = Guid.NewGuid();
            CredentialStore[guid] = creds;
            return guid;
        }

        /// <summary>
        ///     Returns an authenticated windows identity for the credential key.
        ///     Obtain a credential key using <see cref="GetTokenForCredentials(NetworkCredential)" />
        ///     The resulting WindowsIdentity should be disposed of when it is no longer needed.
        /// </summary>
        /// <remarks>
        ///     If the credentials represented by the credentialKey match the domain and userName of the current user,
        ///     no attempt is made to log in since the user is already logged in - WindowsIdentity.GetCurrent() is used instead.
        /// </remarks>
        public static WindowsIdentity LogInAs(Guid credentialKey)
        {
            // Inspired by https://blogs.msdn.microsoft.com/jimmytr/2007/04/14/writing-test-code-with-impersonation/

            NetworkCredential creds = CredentialStore[credentialKey];

            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            string[] nameParts = currentIdentity.Name.Split('\\');

            if (string.Equals(nameParts[0], creds.Domain, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(nameParts[1], creds.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return currentIdentity;
            }

            // Current identity isn't the droid that we were looking for. Get rid of it.
            currentIdentity.Dispose();

            if (NativeMethods.LogonUser(
                creds.UserName, creds.Domain, creds.Password,
                2 /*LOGON32_LOGON_INTERACTIVE*/, // Required to get a "primary token".
                0 /*LOGON32_PROVIDER_DEFAULT*/,
                out IntPtr hToken))
            {
                var identity = new WindowsIdentity(hToken);

                // No longer needed - WindowsIdentity duplicates this handle in its ctor.
                NativeMethods.CloseHandle(hToken);

                // This identity needs to be disposed of when the request is done.
                return identity;
            }

            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to log in as user {creds.UserName}");
        }
    }
}