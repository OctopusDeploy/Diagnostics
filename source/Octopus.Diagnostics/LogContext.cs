using System;
using System.Diagnostics;
using System.Linq;
using Octopus.Data.Model;
using Octopus.Diagnostics.Masking;

namespace Octopus.Diagnostics
{
    [DebuggerDisplay("{CorrelationId}")]
    public class LogContext
    {
        readonly object sensitiveDataMaskLock = new object();
        Lazy<AhoCorasick> trie;
        SensitiveDataMask sensitiveDataMask;

        public LogContext(string correlationId = null, string[] sensitiveValues = null)
        {
            this.CorrelationId = correlationId ?? GenerateId();
            this.SensitiveValues = sensitiveValues ?? new string[0];
            trie = new Lazy<AhoCorasick>(CreateTrie);
        }

        private LogContext(string correlationId, string[] sensitiveValues, Lazy<AhoCorasick> trie)
        {
            CorrelationId = correlationId;
            this.SensitiveValues = sensitiveValues;
            this.trie = trie;
        }

        public string CorrelationId { get; }

        [Encrypted]
        public string[] SensitiveValues { get; private set; }

        public void SafeSanitize(string raw, Action<string> action)
        {
            try
            {
                // JIT creation of sensitiveDataMask
                if (sensitiveDataMask == null && SensitiveValues.Length > 0)
                    lock (sensitiveDataMaskLock)
                    {
                        if (sensitiveDataMask == null && SensitiveValues.Length > 0)
                        {
                            sensitiveDataMask = new SensitiveDataMask();
                        }
                    }

                if (sensitiveDataMask != null)
                    sensitiveDataMask.ApplyTo(trie.Value, raw, action);
                else
                    action(raw);
            }
            catch
            {
                action(raw);
            }
        }

        public LogContext CreateChild(string[] sensitiveValues = null)
        {
            var id = CorrelationId + '/' + GenerateId();

            if (sensitiveValues == null || sensitiveValues.Length == 0)
            {
                // Reuse parent trie
                return new LogContext(id, this.SensitiveValues, trie);
            }

            return new LogContext(id, this.SensitiveValues.Union(sensitiveValues).ToArray());
        }

        /// <summary>
        /// Adds additional sensitive-variables to the LogContext. 
        /// </summary>
        /// <returns>The existing LogContext</returns>
        public LogContext WithSensitiveValues(string[] sensitiveValues)
        {
            if (sensitiveValues == null || sensitiveValues.Length == 0)
                return this;

            var initialSensitiveValuesCount = this.SensitiveValues.Length;
            var sensitiveValuesUnion = this.SensitiveValues.Union(sensitiveValues).ToArray();

            // If no new sensitive-values were added, avoid the cost of rebuilding the trie
            if (initialSensitiveValuesCount == sensitiveValuesUnion.Length)
                return this;

            // New sensitive-values were added, so reset.
            this.SensitiveValues = sensitiveValuesUnion;
            this.trie = new Lazy<AhoCorasick>(CreateTrie);
            return this;
        }

        /// <summary>
        /// Adds an additional sensitive-variable to the LogContext. 
        /// </summary>
        /// <returns>The existing LogContext</returns>
        public LogContext WithSensitiveValue(string sensitiveValue)
        {
            return WithSensitiveValues(new[] { sensitiveValue });
        }

        public void Flush()
        {
            sensitiveDataMask?.Flush(trie.Value);
        }

        static string GenerateId() => Guid.NewGuid().ToString("N");

        AhoCorasick CreateTrie()
        {
            if (SensitiveValues.Length == 0)
                return null;

            var trie = new AhoCorasick();
            foreach (var instance in SensitiveValues)
            {
                if (string.IsNullOrWhiteSpace(instance) || instance.Length < 4)
                    continue;

                var normalized = instance.Replace("\r\n", "").Replace("\n", "");

                trie.Add(normalized);
            }
            trie.Build();
            return trie;
        }
    }
}