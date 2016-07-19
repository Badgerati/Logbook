/*
Logbook is a simple logging library for errors/debug/info.

Copyright (c) 2016, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using Icarus.Core;
using Logbook.Interfaces;
using System;

namespace Logbook
{
    public class LogMessage : IcarusObject, ILogMessage
    {

        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime UtcDateTime { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public LogMessage InnerMessage { get; set; }
        public LogLevel LogLevel { get; set; }

    }


    public static class LogMessageHelper
    {

        /// <summary>
        /// Creates a log message from the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        /// A log message populated with info from the passed message.
        /// </returns>
        public static LogMessage Create(string message, LogLevel logLevel, string source)
        {
            message = string.IsNullOrWhiteSpace(message)
                ? "An issue has occurred, but not message was specified."
                : message;

            return new LogMessage()
            {
                Type = "Message",
                Message = message,
                UtcDateTime = DateTime.UtcNow,
                Source = source,
                StackTrace = Environment.StackTrace,
                InnerMessage = null,
                LogLevel = logLevel
            };
        }

        /// <summary>
        /// Creates a log message from the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">Additional message to be appended to the exception's message.</param>
        /// <returns>
        /// A log message populated with info from the passed exception.
        /// </returns>
        public static LogMessage Create(Exception exception, LogLevel logLevel, string message)
        {
            if (exception == default(Exception))
            {
                return default(LogMessage);
            }

            var logMessage = new LogMessage()
            {
                Type = exception.GetType().Name,
                Message = exception.Message + (string.IsNullOrWhiteSpace(message) ? string.Empty : " | " + message),
                UtcDateTime = DateTime.UtcNow,
                Source = exception.Source,
                StackTrace = (string.IsNullOrWhiteSpace(exception.StackTrace) ? Environment.StackTrace : exception.StackTrace),
                LogLevel = logLevel
            };

            if (exception.InnerException != default(Exception))
            {
                logMessage.InnerMessage = Create(exception.InnerException, logLevel, string.Empty);
            }

            return logMessage;
        }

    }
}
