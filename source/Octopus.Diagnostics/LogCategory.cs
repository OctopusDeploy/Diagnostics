﻿namespace Octopus.Diagnostics
{
    public enum LogCategory
    {
        Trace = 1,
        Verbose = 100,
        Info = 200,
        Planned = 201,
        Note = 210,
        Abandoned = 220,
        Wait = 225,
        Progress = 230,
        Finished = 240,
        Warning = 300,
        Error = 400,
        Fatal = 500
    }
}