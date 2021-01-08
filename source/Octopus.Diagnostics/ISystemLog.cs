using System;

namespace Octopus.Diagnostics
{
    public interface ISystemLog : ILog
    {
        ISystemLog ChildContext(string[] sensitiveValues);
    }
}