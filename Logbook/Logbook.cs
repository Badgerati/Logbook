/*
Logbook is a simple logging library for errors/debug/info.

Copyright (c) 2016, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using Icarus.Core;
using Logbook.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Logbook
{
    public class Logbook : ILogbook
    {

        #region Constants

        public const string DefaultTag = "logs";

        #endregion

        #region Lazy Initialiser

        private static Lazy<ILogbook> _lazy = new Lazy<ILogbook>(() => new Logbook());
        public static ILogbook Instance
        {
            get { return _lazy.Value; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the data store.
        /// </summary>
        /// <value>
        /// The name of the data store.
        /// </value>
        public string DataStoreName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location
        {
            get
            {
                return string.IsNullOrEmpty(Tag) || !IcarusClient.Instance.Locations.ContainsKey(Tag)
                    ? string.Empty
                    : IcarusClient.Instance.Locations[Tag];
            }
        }

        /// <summary>
        /// Gets or sets the days to keep logs.
        /// </summary>
        /// <value>
        /// The days to keep logs.
        /// </value>
        public int DaysToKeepLogs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether logs should be deleted synchronously,
        /// otherwise older logs will always been removed asynchronously.
        /// </summary>
        /// <value>
        /// <c>true</c> if [delete logs synchronously]; otherwise, <c>false</c>.
        /// </value>
        public bool DeleteLogsSynchronously
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log level that the logbook should log at, critical being the highest
        /// and verbose being the lowest.
        /// The default log level is Error.
        /// </summary>
        /// <value>
        /// The current log level of the logbook.
        /// </value>
        public LogLevel LogLevel
        {
            get;
            set;
        }

        #endregion

        #region Fields

        private Queue<string> _queue = default(Queue<string>);

        private string _collectionName
        {
            get { return "log" + DateTime.UtcNow.ToString("yyyyMMdd"); }
        }

        private IIcarusCollection<LogMessage> _collection
        {
            get
            {
                var isNew = false;
                var dataStore = IcarusClient.Instance.GetDataStore(DataStoreName, locationTag: Tag, isAccessEveryone: true);

                var collection = dataStore.GetCollection<LogMessage>(_collectionName, out isNew);
                collection.CachingEnabled = false;

                if (isNew)
                {
                    dataStore.Clear();
                }

                if (!_queue.Contains(collection.CollectionLocation))
                {
                    _queue.Enqueue(collection.CollectionLocation);
                }

                var oldLocations = GetOldLogs();
                if (oldLocations != default(IList<string>))
                {
                    var task = Task.Run(() => RemoveOldLogs(oldLocations));
                    if (DeleteLogsSynchronously)
                    {
                        Task.WaitAll(task);

                        if (DaysToKeepLogs == 0)
                        {
                            dataStore.Clear();
                            collection = dataStore.GetCollection<LogMessage>(_collectionName, out isNew);
                        }
                    }
                }

                return collection;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="Logbook"/> class from being created.
        /// </summary>
        private Logbook()
        {
            DeleteLogsSynchronously = false;
            LogLevel = LogLevel.Error;
        }

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
        public ILogbook Initialise(string logbookLocation, string dataStoreName = "", string logbookTag = "", int daysToKeepLogs = 30)
        {
            if (!string.IsNullOrEmpty(Tag))
            {
                throw new ApplicationException(string.Format("The logbook has already been initialised with a tag of '{0}', at location '{1}'.", Tag, Location));
            }

            DaysToKeepLogs = daysToKeepLogs;
            _queue = new Queue<string>(DaysToKeepLogs);

            logbookTag = string.IsNullOrEmpty(logbookTag)
                ? DefaultTag
                : logbookTag;

            DataStoreName = string.IsNullOrWhiteSpace(dataStoreName)
                ? DefaultTag
                : dataStoreName;

            IcarusClient.Instance.Initialise(logbookLocation, logbookTag);

            var dataStore = IcarusClient.Instance.GetDataStore(DataStoreName, locationTag: Tag);
            var files = Directory.GetFiles(dataStore.DataStoreLocation, @"log*.json", SearchOption.TopDirectoryOnly).ToList();

            if (files.Any())
            {
                files = files.OrderBy(x => x).ToList();
                files.ForEach(x => _queue.Enqueue(x));
            }

            return this;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">Additional message to be appended to the exception's message.</param>
        /// <returns>
        /// The log message that was created.
        /// </returns>
        public ILogMessage Log(Exception exception, LogLevel logLevel, string message = "")
        {
            if (logLevel > LogLevel)
            {
                return default(ILogMessage);
            }

            return _collection.Insert(LogMessageHelper.Create(exception, logLevel, message));
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The log message that was created.
        /// </returns>
        public ILogMessage Log(string message, LogLevel logLevel, string source = "")
        {
            if (logLevel > LogLevel)
            {
                return default(ILogMessage);
            }

            return _collection.Insert(LogMessageHelper.Create(message, logLevel, source));
        }

        #endregion

        #region Private Methods

        private IList<string> GetOldLogs()
        {
            if (_queue.Count <= DaysToKeepLogs)
            {
                return default(IList<string>);
            }

            var count = _queue.Count - DaysToKeepLogs;
            var locations = new List<string>(count);

            for (var i = 0; i < count; i++)
            {
                locations.Add(_queue.Dequeue());
            }

            return locations;
        }

        private void RemoveOldLogs(IList<string> logLocations)
        {
            if (logLocations == default(IList<string>))
            {
                return;
            }

            foreach (var location in logLocations)
            {
                if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
                {
                    File.Delete(location);
                }
            }
        }

        #endregion

    }
}
