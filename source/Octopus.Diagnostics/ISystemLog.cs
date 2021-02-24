using System;

namespace Octopus.Diagnostics
{
    public interface ISystemLog : ILog, IDisposable
    {
        ISystemLog ChildContext(string[] sensitiveValues);
    }
}