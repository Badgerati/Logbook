/*
Logbook is a simple logging library for errors/debug/info.

Copyright (c) 2016, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using Logbook.Interfaces;

namespace Logbook.Events
{

    /// <summary>
    /// OnLog event delegate header.
    /// </summary>
    /// <param name="message">The log message.</param>
    public delegate void OnLogEventHandler(ILogMessage message);

}
