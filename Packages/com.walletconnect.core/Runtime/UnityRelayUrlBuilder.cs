using System;
using UnityEngine;
using WalletConnectSharp.Network;

namespace WalletConnectUnity.Core
{
    public class UnityRelayUrlBuilder : RelayUrlBuilder
    {
        // TODO: Update this from CI
        public const string PackageVersion = "3.1.6";

        public override (string name, string version) GetOsInfo()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var splitOS = SystemInfo.operatingSystem.Split(' ');
            return ("android", splitOS[2]);
#elif UNITY_IOS && !UNITY_EDITOR
            var splitOS = SystemInfo.operatingSystem.Split(' ');
            // var platform = splitOS[0].ToLower();
            var version = splitOS[1];
            return ("ios", version);
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            var osString = SystemInfo.operatingSystem;
            var startIndex = osString.IndexOf("OS X", StringComparison.Ordinal) + 5;
            var version = osString.Substring(startIndex);
            return ("macos", version);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var splitOS = SystemInfo.operatingSystem.Split(' ');
            var version = splitOS[1]; // e.g. Vista or 11
            var architecture = splitOS[^1]; // e.g. 64bit
            return ("windows", $"{version}-{architecture}");
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            var splitOS = SystemInfo.operatingSystem.Split(' ');
            if (splitOS.Length < 5)
                return ("linux", "unknown");

            var kernelVersion = splitOS[1];
            var distribution = splitOS[2];
            var architecture = splitOS[splitOS.Length - 1];
            
            return ("linux", $"{distribution}-{kernelVersion}-{architecture}");
#else
            return base.GetOsInfo();
#endif
        }

        public override (string name, string version) GetSdkInfo()
        {
            return ("unity", PackageVersion);
        }

        protected override bool TryGetOrigin(out string origin)
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            origin = Application.identifier;
            return true;
#endif
            return base.TryGetOrigin(out origin);
        }
    }
}