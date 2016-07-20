/*
Logbook is a simple logging library for errors/debug/info.

Copyright (c) 2016, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using Icarus.Core;
using NUnit.Framework;
using System;
using System.IO;

namespace Logbook.Tests
{
    [TestFixture]
    public class BasicTests
    {

        private readonly string _collection = "log" + DateTime.UtcNow.ToString("yyyyMMdd");


        [TestFixtureSetUp]
        public void SetupBase()
        {
            Logbook.Instance.Initialise(".");
        }

        [TearDown]
        public void Teardown()
        {
            Logbook.Instance.DaysToKeepLogs = 30;
            Logbook.Instance.DeleteLogsSynchronously = false;
            Logbook.Instance.LogLevel = LogLevel.Error;

            var path = Path.GetFullPath(".\\" + Logbook.Instance.DataStoreName + "\\" + _collection + ".json");
            if (File.Exists(path))
            {
                using (var file = File.CreateText(path))
                {
                    file.WriteLine("{}");
                }

                IcarusClient.Instance
                    .GetDataStore(Logbook.Instance.DataStoreName, locationTag: Logbook.Instance.Tag)
                    .GetCollection<LogMessage>(_collection)
                    .Refresh(false);
            }
        }


        #region Log Level

        [Test]
        [TestCase(LogLevel.Critical, LogLevel.Critical, false)]
        [TestCase(LogLevel.Error, LogLevel.Error, false)]
        [TestCase(LogLevel.Error, LogLevel.Critical, false)]
        [TestCase(LogLevel.Error, LogLevel.Warning, true)]
        [TestCase(LogLevel.Error, LogLevel.Debug, true)]
        [TestCase(LogLevel.Verbose, LogLevel.Debug, false)]
        [TestCase(LogLevel.Verbose, LogLevel.Verbose, false)]
        public void Log_ExampleException_Success(LogLevel logbookLevel, LogLevel exLevel, bool isNull)
        {
            Logbook.Instance.LogLevel = logbookLevel;

            var ex = new Exception("Example message");
            var logMessage = Logbook.Instance.Log(ex, exLevel);

            if (isNull)
            {
                Assert.IsNull(logMessage);
            }
            else
            {
                Assert.IsNotNull(logMessage);
                Assert.AreEqual(ex.Message, logMessage.Message);
                Assert.IsNotNull(logMessage.UtcDateTime);
                Assert.IsNotNullOrEmpty(logMessage.StackTrace);
                Assert.AreEqual(exLevel, logMessage.LogLevel);
            }
        }

        #endregion

        #region OnLog Event

        [Test]
        public void Log_Exception_OnLogEvent_Success()
        {
            try
            {
                var value = int.Parse("fail");
            }
            catch (Exception ex)
            {
                var logMessage = Logbook.Instance.Log(ex, LogLevel.Error);

                Assert.IsNotNull(logMessage);
                Assert.AreEqual(ex.Message, logMessage.Message);
                Assert.IsNotNull(logMessage.UtcDateTime);
                Assert.IsNotNullOrEmpty(logMessage.StackTrace);
            }
        }

        #endregion

        #region Log Exception

        [Test]
        public void Log_ExampleException_Success()
        {
            var ex = new Exception("Example message");

            var logMessage = Logbook.Instance.Log(ex, LogLevel.Error);

            Assert.IsNotNull(logMessage);
            Assert.AreEqual(ex.Message, logMessage.Message);
            Assert.IsNotNull(logMessage.UtcDateTime);
            Assert.IsNotNullOrEmpty(logMessage.StackTrace);
        }

        [Test]
        public void Log_RealException_Success()
        {
            try
            {
                var value = int.Parse("fail");
            }
            catch (Exception ex)
            {
                var logMessage = Logbook.Instance.Log(ex, LogLevel.Error);

                Assert.IsNotNull(logMessage);
                Assert.AreEqual(ex.Message, logMessage.Message);
                Assert.IsNotNull(logMessage.UtcDateTime);
                Assert.IsNotNullOrEmpty(logMessage.StackTrace);
            }
        }

        [Test]
        public void Log_RealException_ExtraMessage_Success()
        {
            try
            {
                var value = int.Parse("fail");
            }
            catch (Exception ex)
            {
                var msg = "Example Message";
                var logMessage = Logbook.Instance.Log(ex, LogLevel.Error, msg);

                Assert.IsNotNull(logMessage);
                Assert.AreEqual(ex.Message + " | " + msg, logMessage.Message);
                Assert.IsNotNull(logMessage.UtcDateTime);
                Assert.IsNotNullOrEmpty(logMessage.StackTrace);
            }
        }

        [Test]
        public void Log_InnerException_Success()
        {
            var ex = new Exception("Example message", new Exception("Example inner message"));

            var logMessage = Logbook.Instance.Log(ex, LogLevel.Error);

            Assert.IsNotNull(logMessage);

            Assert.AreEqual(ex.Message, logMessage.Message);
            Assert.IsNotNull(logMessage.UtcDateTime);
            Assert.IsNotNullOrEmpty(logMessage.StackTrace);

            Assert.IsNotNull(logMessage.InnerMessage);
            Assert.AreEqual(ex.InnerException.Message, logMessage.InnerMessage.Message);
            Assert.IsNotNull(logMessage.InnerMessage.UtcDateTime);
            Assert.IsNotNullOrEmpty(logMessage.InnerMessage.StackTrace);
        }

        [Test]
        public void Log_Two_InnerExceptions_Success()
        {
            var ex = new Exception("Example message", new Exception("Example inner message 1", new Exception("Example inner message 2")));

            var logMessage = Logbook.Instance.Log(ex, LogLevel.Error);

            Assert.IsNotNull(logMessage);

            Assert.AreEqual(ex.Message, logMessage.Message);
            Assert.IsNotNull(logMessage.UtcDateTime);
            Assert.IsNotNullOrEmpty(logMessage.StackTrace);

            Assert.IsNotNull(logMessage.InnerMessage);
            Assert.AreEqual(ex.InnerException.Message, logMessage.InnerMessage.Message);
            Assert.IsNotNull(logMessage.InnerMessage.UtcDateTime);
            Assert.IsNotNullOrEmpty(logMessage.InnerMessage.StackTrace);

            Assert.IsNotNull(logMessage.InnerMessage.InnerMessage);
            Assert.AreEqual(ex.InnerException.InnerException.Message, logMessage.InnerMessage.InnerMessage.Message);
            Assert.IsNotNull(logMessage.InnerMessage.InnerMessage.UtcDateTime);
            Assert.IsNotNullOrEmpty(logMessage.InnerMessage.InnerMessage.StackTrace);
        }

        #endregion

        #region Log Message

        [Test]
        public void Log_Message_Success()
        {
            var msg = "Example message";

            var logMessage = Logbook.Instance.Log(msg, LogLevel.Error);

            Assert.IsNotNull(logMessage);
            Assert.AreEqual(msg, logMessage.Message);
            Assert.IsNotNull(logMessage.UtcDateTime);
            Assert.IsNullOrEmpty(logMessage.Source);
            Assert.IsNotNullOrEmpty(logMessage.StackTrace);
        }

        [Test]
        public void Log_MessageWithSource_Success()
        {
            var msg = "Example message";
            var src = "Somewhere";

            var logMessage = Logbook.Instance.Log(msg, LogLevel.Error, source: src);

            Assert.IsNotNull(logMessage);
            Assert.AreEqual(msg, logMessage.Message);
            Assert.IsNotNull(logMessage.UtcDateTime);
            Assert.AreEqual(src, logMessage.Source);
            Assert.IsNotNullOrEmpty(logMessage.StackTrace);
        }

        #endregion

        #region Days to Keep

        [Test]
        public void DaysToKeep_Zero_Delete_Success()
        {
            // set to days to 30
            Logbook.Instance.DeleteLogsSynchronously = true;
            Logbook.Instance.DaysToKeepLogs = 30;

            var msg = "Example message";
            var src = "Somewhere";

            var logMessage = Logbook.Instance.Log(msg, LogLevel.Error, source: src);

            Assert.IsNotNull(logMessage);
            Assert.AreEqual(msg, logMessage.Message);
            Assert.IsNotNull(logMessage.UtcDateTime);
            Assert.AreEqual(src, logMessage.Source);
            Assert.IsNotNullOrEmpty(logMessage.StackTrace);

            // set days to 0, insert and there should only be one entry - the new one
            Logbook.Instance.DaysToKeepLogs = 0;

            var msg2 = "Example message 2";
            var src2 = "Somewhere2";

            var logMessage2 = Logbook.Instance.Log(msg2, LogLevel.Error, source: src2);

            var items = IcarusClient.Instance
                .GetDataStore(Logbook.Instance.DataStoreName, locationTag: Logbook.Instance.Tag)
                .GetCollection<LogMessage>(_collection)
                .All();

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(msg2, items[0].Message);
            Assert.AreEqual(src2, items[0].Source);

            // set days back to 30 and we should get 2 items
            Logbook.Instance.DaysToKeepLogs = 30;

            var msg3 = "Example message 3";
            var src3 = "Somewhere3";

            var logMessage3 = Logbook.Instance.Log(msg3, LogLevel.Error, source: src3);

            items = IcarusClient.Instance
                .GetDataStore(Logbook.Instance.DataStoreName, locationTag: Logbook.Instance.Tag)
                .GetCollection<LogMessage>(_collection)
                .All();

            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(msg3, items[1].Message);
            Assert.AreEqual(src3, items[1].Source);
        }

        #endregion

    }
}
