using System;
using NUnit.Framework;

namespace WalletConnectUnity.Core.Tests
{
    public class LinkerTests
    {
        [Test]
        public void BuildConnectionDeepLink_WithValidNativeUrl_ReturnsCorrectUrl()
        {
            var appLink = "myapp://";
            var wcUri = "wc:1234567890abcdef";

            var result = Linker.BuildConnectionDeepLink(appLink, wcUri);

            Assert.AreEqual("myapp://wc?uri=wc%3A1234567890abcdef", result);
        }

        [Test]
        public void BuildConnectionDeepLink_WithEmptyAppLink_ThrowsArgumentException()
        {
            var appLink = "";
            var wcUri = "wc:1234567890abcdef";

            Assert.Throws<ArgumentException>(() => Linker.BuildConnectionDeepLink(appLink, wcUri));
        }

        [Test]
        public void BuildConnectionDeepLink_WithEmptyWcUri_ThrowsArgumentException()
        {
            var appLink = "myapp://";
            var wcUri = "";

            Assert.Throws<ArgumentException>(() => Linker.BuildConnectionDeepLink(appLink, wcUri));
        }

        [Test]
        public void BuildConnectionDeepLink_WithNativeUrlWithoutTrailingSlash_ReturnsCorrectUrl()
        {
            var appLink = "myapp://main";
            var wcUri = "wc:1234567890abcdef";

            var result = Linker.BuildConnectionDeepLink(appLink, wcUri);

            Assert.AreEqual("myapp://main/wc?uri=wc%3A1234567890abcdef", result);
        }

        [Test]
        public void BuildConnectionDeepLink_WithNativeUrlWithTrailingSlash_ReturnsCorrectUrl()
        {
            var appLink = "myapp://main/";
            var wcUri = "wc:1234567890abcdef";

            var result = Linker.BuildConnectionDeepLink(appLink, wcUri);

            Assert.AreEqual("myapp://main/wc?uri=wc%3A1234567890abcdef", result);
        }
    }
}