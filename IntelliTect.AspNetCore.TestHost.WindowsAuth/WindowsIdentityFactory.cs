using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth
{
    internal static class WindowsIdentityFactory 
    {
        // Store credentials keyed by a GUID so that we don't have to serialize actual credentials into HTTP headers.
        private static readonly ConcurrentDictionary<Guid, NetworkCredential> credentialStore 
            = new ConcurrentDictionary<Guid, NetworkCredential>();

        /// <summary>
        /// Return a GUID that can be lated user to log in as the user represented by the credentials.
        /// This is passed as an HTTP header to allow a receiving in-memory server to authenticate as this user.
        /// </summary>
        public static Guid GetTokenForCredentials(NetworkCredential creds)
        {
            creds.Domain = creds.Domain ?? System.Environment.UserDomainName;
            var guid = Guid.NewGuid();
            credentialStore[guid] = creds;
            return guid;
        }

        /// <summary>
        /// Returns an authenticated windows identity for the credential key.
        /// Obtain a credential key using <see cref="GetTokenForCredentials(NetworkCredential)"/>
        /// The resulting WindowsIdentity should be disposed of when it is no longer needed.
        /// </summary>
        /// <remarks>
        /// If the credentials represented by the credentialKey match the domain and userName of the current user,
        /// no attempt is made to log in since the user is already logged in - WindowsIdentity.GetCurrent() is used instead.
        /// </remarks>
        public static WindowsIdentity LogInAs(Guid credentialKey)
        {
            // Inspired by https://blogs.msdn.microsoft.com/jimmytr/2007/04/14/writing-test-code-with-impersonation/

            NetworkCredential creds = credentialStore[credentialKey];

            var currentIdentity = WindowsIdentity.GetCurrent();
            var nameParts = currentIdentity.Name.Split('\\');

            if (string.Equals(nameParts[0], creds.Domain, StringComparison.InvariantCultureIgnoreCase)
             && string.Equals(nameParts[1], creds.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return currentIdentity;
            }
            else 
            {
                // Current identity isn't the droid that we were looking for. Get rid of it.
                currentIdentity.Dispose();
            }

            if (Win32.LogonUser(
                creds.UserName, creds.Domain, creds.Password,
                2 /*LOGON32_LOGON_INTERACTIVE*/, // Required to get a "primary token".
                0 /*LOGON32_PROVIDER_DEFAULT*/,
                out IntPtr hToken))
            {
                var identity = new WindowsIdentity(hToken);

                // No longer needed - WindowsIdentity duplicates this handle in its ctor.
                Win32.CloseHandle(hToken);

                // This identity needs to be disposed of when the request is done.
                return identity;
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to log in as user {creds.UserName}");
            }
        }

        private static class Win32
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool LogonUser(
                string lpszUsername,
                string lpszDomain,
                string lpszPassword,
                int dwLogonType,
                int dwLogonProvider,
                out IntPtr phToken
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hHandle);
        }
    }
}