/*
Logbook is a simple logging library for errors/debug/info.

Copyright (c) 2016, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;

namespace Logbook.Interfaces
{
    public interface ILogbook
    {

        /// <summary>
        /// Gets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        string Tag { get; }

        /// <summary>
        /// Gets the name of the data store.
        /// </summary>
        /// <value>
        /// The name of the data store.
        /// </value>
        string DataStoreName { get; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        string Location { get; }

        /// <summary>
        /// Gets or sets the days to keep logs.
        /// </summary>
        /// <value>
        /// The days to keep logs.
        /// </value>
        int DaysToKeepLogs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logs should be deleted synchronously,
        /// otherwise older logs will always been removed asynchronously.
        /// </summary>
        /// <value>
        /// <c>true</c> if [delete logs synchronously]; otherwise, <c>false</c>.
        /// </value>
        bool DeleteLogsSynchronously { get; set; }

        /// <summary>
        /// Gets or sets the log level that the logbook should log at, critical being the highest
        /// and verbose being the lowest.
        /// The default log level is Error.
        /// </summary>
        /// <value>
        /// The current log level of the logbook.
        /// </value>
        LogLevel LogLevel { get; set; }


        /// <summary>
        /// Initialises the specified logbook location, Icarus data store name, and the amount of days to store logs.
        /// </summary>
        /// <param name="logbookLocation">The logbook location.</param>
        /// <param name="dataStoreName">Name of the data store, this is the data store name that is used by Icarus.</param>
        /// <param name="logbookTag">The logbook tag to help identify the logging location in Icarus.</param>
        /// <param name="daysToKeepLogs">The days to keep logs.</param>
        /// <returns>
        /// This logbook instance.
        /// </returns>
        ILogbook Initialise(string logbookLocation, string dataStoreName = "", string logbookTag = "", int daysToKeepLogs = 30);

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">Additional message to be appended to the exception's message.</param>
        /// <returns>
        /// The log message that was created.
        /// </returns>
        ILogMessage Log(Exception exception, LogLevel logLevel, string message = "");

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The log message that was created.
        /// </returns>
        ILogMessage Log(string message, LogLevel logLevel, string source = "");

    }
}
