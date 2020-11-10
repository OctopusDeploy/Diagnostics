using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assent;
using NUnit.Framework;
using Octopus.Diagnostics;

namespace Tests
{
    public class PrettyPrintFixture
    {
        static readonly Configuration Config = new Configuration().UsingSanitiser(Sanitise);

        // The stacktraces and some messages look different between Full Framework and .NET Core
        // Keep this while this class is still consumed by non-netcoreapp3.1 projects
#if NETCOREAPP
        static readonly Configuration PlatformSpecificConfig = Config.UsingExtension("core.txt");
#else
        private static readonly Configuration PlatformSpecificConfig = Config.UsingExtension("netfx.txt");
#endif

        [Test]
        public void InnerExceptionsWithStackTrace()
        {
            var exception = CaptureException(CallThrowInnerException, true);
            this.Assent(exception, PlatformSpecificConfig);
        }

        [Test]
        public void InnerExceptionsNoStackTrace()
        {
            var exception = CaptureException(() => ThrowInnerException());
            this.Assent(exception, Config);
        }

        [Test]
        public void AggregateException()
        {
            var exception = CaptureException(() =>
            {
                Task.WaitAll(
                    ThrowDivideByZeroExceptionAsync(),
                    ThrowDivideByZeroExceptionAsync()
                );
            });

            this.Assent(exception, Config);
        }

        [Test]
        public void AggregateExceptionWithStackTrace()
        {
            var exception = CaptureException(() =>
                {
                    Task.WaitAll(
                        ThrowDivideByZeroExceptionAsync(),
                        ThrowDivideByZeroExceptionAsync()
                    );
                },
                true);

            this.Assent(exception, PlatformSpecificConfig);
        }

        [Test]
        public void OperationCanceledExceptionWithStackTraceTrue()
        {
            var exception = CaptureException(() => throw new OperationCanceledException(), true);
            this.Assent(exception, Config);
        }

        [Test]
        public void ControlledFailureExceptionWithStackTraceTrue()
        {
            var exception = CaptureException(() => throw new ControlledFailureException(), true);
            this.Assent(exception, Config);
        }

        [Test]
        public void SqlException()
        {
            var c = typeof(SqlError).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            var sqlError = Activator.CreateInstance(
                typeof(SqlError),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object?[] { 0, (byte)0, (byte)0, "", "The Error Message", "", 42, null },
                null
            );

            var sqlErrors = Activator.CreateInstance(
                typeof(SqlErrorCollection),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object?[0],
                null
            )!;

            sqlErrors.GetType().GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(sqlErrors, new[] { sqlError });

            var exception = (Exception)Activator.CreateInstance(
                typeof(SqlException),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[]
                {
                    "A fake SQLException",
                    sqlErrors,
                    null,
                    Guid.Empty
                },
                null
            )!;

            this.Assent(exception.PrettyPrint(false), Config);
        }

        string CaptureException(Action action, bool stackTrace = false)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                return ex.PrettyPrint(stackTrace);
            }

            Assert.Fail("Exception expected");
            return "";
        }

        async Task ThrowDivideByZeroExceptionAsync(int n = 3)
        {
            if (n == 0)
            {
                var o = 1 / n;
            }

            await ThrowDivideByZeroExceptionAsync(n - 1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void CallThrowInnerException()
            => ThrowInnerException();

        void ThrowInnerException(int n = 3)
        {
            if (n == 0)
                throw new Exception("Innermost");

            try
            {
                ThrowInnerException(n - 1);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception Level " + n, ex);
            }
        }

        static string Sanitise(string approval)
        {
            approval = Regex.Replace(approval, "line [0-9]+", "line <line_number>");
            approval = Regex.Replace(approval, @">b__[0-9_]+\(\)", "><compiler_generated>()");
            return approval;
        }
    }

    class ControlledFailureException : Exception
    {
        public ControlledFailureException() : base("The deployment failed")
        {
        }
    }
}