using System;
using System.Text;

namespace Octopus.Diagnostics
{
    /// <summary>
    /// Do not implement this interface directly, use the generics based one.
    /// </summary>
    public interface ICustomPrettyPrintHandler
    {
        /// <summary>
        /// Custom handler for PrettyPrinting a type of exception
        /// </summary>
        /// <param name="sb">StringBuilder for the "pretty" output</param>
        /// <param name="ex">The exception instance</param>
        /// <returns>True if the processing should continue on to processing stack trace or inner exceptions</returns>
        bool Handle(StringBuilder sb, Exception ex);
    }

    public interface ICustomPrettyPrintHandler<in TException> : ICustomPrettyPrintHandler
    {
    }
}