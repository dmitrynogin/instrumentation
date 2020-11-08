using System;
using System.Threading;

namespace Instrumentation
{
    public class Cancellation : IDisposable
    {
        static AsyncLocal<CancellationTokenSource> Context { get; } =
            new AsyncLocal<CancellationTokenSource>();

        public Cancellation()
            : this(CancellationToken.None)
        {
        }

        public Cancellation(CancellationToken cancellationToken)
            : this(cancellationToken, Timeout.InfiniteTimeSpan)
        {
        }

        public Cancellation(int timeout)
            : this(TimeSpan.FromMilliseconds(timeout))
        {
        }

        public Cancellation(TimeSpan timeout)
            : this(CancellationToken.None, timeout)
        {
        }

        public Cancellation(CancellationToken cancellationToken, int timeout)
            : this(cancellationToken, TimeSpan.FromMilliseconds(timeout))
        {
        }

        public Cancellation(CancellationToken cancellationToken, TimeSpan timeout)
        {
            Parent = Context.Value;
            Context.Value = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Context.Value.CancelAfter(timeout);
        }

        public void Dispose()
        {
            var cts = Context.Value;
            Context.Value = Parent;
            cts.Dispose();
        }

        CancellationTokenSource Parent { get; }
        public static CancellationToken Token => Context.Value?.Token ?? CancellationToken.None;
        public static void Request() => Context.Value?.Cancel();
        public static bool Requested => Token.IsCancellationRequested;
        public static void ThrowIfRequested() => Token.ThrowIfCancellationRequested();
    }
}
