# Logbook
[![Build Status](https://travis-ci.org/Badgerati/Logbook.svg?branch=master)](https://travis-ci.org/Badgerati/Logbook)
[![Build status](https://ci.appveyor.com/api/projects/status/7bxpopvdgggfqkf8?svg=true)](https://ci.appveyor.com/project/Badgerati/logbook)
[![Code Climate](https://codeclimate.com/github/Badgerati/Logbook/badges/gpa.svg)](https://codeclimate.com/github/Badgerati/Logbook)
[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/Badgerati/Logbook/master/LICENSE.txt)

Logbook is a simple error logging tool, that stores all log messages in JSON format files - sharded off into days. Logbook uses [Icarus](https://github.com/Badgerati/Icarus "Icarus") behind the scenes for it's storage functionality.

# Features
* Logs are stored in JSON format.
* All data is stored into some specified location.
* Fast and clean.
* Can filter logs that get inserted by log level.
* Logs are stored in per-day stores, ie "log20160717.json"
* Can specify how many days of logs to keep.

# Usage
Logbook has a similar feel to using Icarus, but a little more simple.

## Initialise the Logbook
The main point of entry for everything is the `Logbook` class. This is a lazy initialised class for inserting your logs. To initialise use:

```C#
Logbook.Instance.Initialise("<SOME_LOCATION>" [, "<DATASTORE_NAME>", "<TAG>", <DAYS_TO_KEEP_LOGS>]);
```

* The `<SOME_LOCATION>` is where you would place the location to where you want Logbook to write everything.
* The optional `<DATASTORE_NAME>` parameter is the name of the directory at <SOME_LOCATION> to store the logs; by default this is just `logs`, and if it doesn't exist it will be created.
* The optional `<TAG>` parameter is mostly used if you're already using Icarus and have multiple locations already defined. If you aren't using Icarus already you can leave this out; the default value is `logs`.
* The optional `<DAYS_TO_KEEP_LOGS>` parameter is the total number of days to keep logs for; the default is 30 days, and any logs older than this are auto-deleted.

For example:

```C#
var logbook = Logbook.Instance;
logbook.Initialise("C:\\logs", daysToKeepLogs: 14);
```
If you wish to change the value of DAYS_TO_KEEP_LOGS later on, you can set the property in the Logbook as follows:

```C#
Logbook.Instance.DaysToKeepLogs = 30;
```

## Setting the Log Level
When you insert a log entry you also have to specify the LogLevel type. This can be one of the following in priority order:

* Critical
* Error
* Warning
* Information
* Debug
* Verbose

By default Logbook will log everything that is Error or higher, but you can change this. If you wish for Logbook to log everything of say Debug or higher for testing then you can use:

```C#
Logbook.Instance.LogLevel = LogLevel.Debug;
```

## Inserting Logs
Inserting a log entry is pretty simple, there are two ways:

* Create a log directly from an Exception, with an optional message to be appended
* Create a log with a specified message and optional source

### Log Exception
For the first way, by Exception, you would do the following for example:

```C#
try
{
    var value = int.Parse("fail");
}
catch (Exception ex)
{
    Logbook.Instance.Log(ex, LogLevel.Error, "Some additional optional information");
}
```

This will now log an entry for a NumberFormatException, including:

* Date and time in UTC of creation
* Exception error message, appended with the optional message if supplied
* Exception type
* Full stacktrace
* Exception source
* Logging level
* Any inner Exception details if they're available (including that exception's inner exceptions, etc.)

### Log Message
Logging a message is pretty much the same as the above, except you can specify a specific message with an optional source (such as class/method name):

```C#
if (thisValue != someValue)
{
    Logbook.Instance.Log(string.Format("Value specified is incorrect {0}", thisValue), LogLevel.Error, "SomeClass.SomeMethod");
}
```

This will log an entry with the specified message/source (empty if not supplied), and the entry will also contain the stacktrace and date.

## OnLog Events
If you have an external logging system, or would like to do some additional logic (like emails) when a exception get's logged, then you can use the OnLog event. This event is triggered whenever your calls to `Log(...)` actually log a message, not every time you call `Log(...)` as you may have level filtering on. An example is as follows:

```C#
public void Main()
{
    Logbook.Instance.Initialise("C:\Logs");
    Logbook.Instance.OnLog += OnLog_Event;
}

protected void OnLog_Event(ILogMessage message)
{
    // Your logic here
}
```

# Bugs and Feature Requests
For any bugs you may find or features you wish to request, please create an [issue](https://github.com/Badgerati/Logbook/issues "Issues") in GitHub.
