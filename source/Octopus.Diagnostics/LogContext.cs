using System;

namespace Octopus.Diagnostics
{
    public interface ILogContext
    {
        string CorrelationId { get; }

        string[] SensitiveValues { get; }

        void SafeSanitize(string raw, Action<string> action);

        ILogContext CreateChild(string[] sensitiveValues = null);

        /// <summary>
        /// Adds additional sensitive-variables to the LogContext. 
        /// </summary>
        /// <returns>The existing LogContext</returns>
        ILogContext WithSensitiveValues(string[] sensitiveValues);

        /// <summary>
        /// Adds an additional sensitive-variable to the LogContext. 
        /// </summary>
        /// <returns>The existing LogContext</returns>
        ILogContext WithSensitiveValue(string sensitiveValue);

        void Flush();
    }
}