/*
Logbook is a simple logging library for errors/debug/info.

Copyright (c) 2016, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;

namespace Logbook.Interfaces
{
    public interface ILogMessage
    {

        string Type { get; }
        string Message { get; }
        DateTime UtcDateTime { get; }
        string Source { get; }
        string StackTrace { get; }
        LogMessage InnerMessage { get; }
        LogLevel LogLevel { get; }

    }
}
