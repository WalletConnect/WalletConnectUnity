using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WalletConnectUnity.Core.Tests
{
    public class LoggerTests
    {
        private Logger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
        }

        [Test]
        public void LogsMessageCorrectly()
        {
            const string testMessage = "Test message";
            LogAssert.Expect(LogType.Log, testMessage);
            _logger.Log(testMessage);
        }

        [Test]
        public void LogsErrorCorrectly()
        {
            const string testMessage = "Test error message";
            LogAssert.Expect(LogType.Error, testMessage);
            _logger.LogError(testMessage);
        }

        [Test]
        public void LogsExceptionCorrectly()
        {
            var testException = new Exception("Test exception");
            LogAssert.Expect(LogType.Exception, $"Exception: {testException.Message}");
            _logger.LogError(testException);
        }
    }
}