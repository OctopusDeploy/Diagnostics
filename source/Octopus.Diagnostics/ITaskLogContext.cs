using System;

namespace Octopus.Diagnostics
{
    public interface ITaskLogContext
    {
        string CorrelationId { get; }

        ITaskLogContext CreateChild(string[]? sensitiveValues = null);

        /// <summary>
        /// Adds additional sensitive-variables to the LogContext.
        /// </summary>
        /// <returns>The existing LogContext</returns>
        ITaskLogContext WithSensitiveValues(string[] sensitiveValues);

        /// <summary>
        /// Adds an additional sensitive-variable to the LogContext.
        /// </summary>
        /// <returns>The existing LogContext</returns>
        ITaskLogContext WithSensitiveValue(string sensitiveValue);

        void Flush();
    }
}