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
        public void InitializeLogger()
        {
            _logger = new Logger();
        }

        [Test]
        public void Log_WithValidMessage_LogsCorrectly()
        {
            const string testMessage = "Test message";
            LogAssert.Expect(LogType.Log, testMessage);
            _logger.Log(testMessage);
        }

        [Test]
        public void LogError_WithValidMessage_LogsErrorCorrectly()
        {
            const string testMessage = "Test error message";
            LogAssert.Expect(LogType.Error, testMessage);
            _logger.LogError(testMessage);
        }

        [Test]
        public void LogError_WithException_LogsExceptionCorrectly()
        {
            var testException = new Exception("Test exception");
            LogAssert.Expect(LogType.Exception, $"Exception: {testException.Message}");
            _logger.LogError(testException);
        }
    }
}