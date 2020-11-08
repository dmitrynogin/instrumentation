using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Timeout;

namespace Instrumentation.Interceptors
{
    class Repeater : AsyncInterceptorBase
    {
        public Repeater(Retry retry, Func<MethodInfo, Exception, bool> repeat)
        {
            Retry = retry ?? throw new ArgumentNullException(nameof(retry));
            Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
        }

        Retry Retry { get; }
        Func<MethodInfo, Exception, bool> Repeat { get; }

        protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            for (int i = 1; ; i++)
                try
                {
                    Cancellation.ThrowIfRequested();
                    await proceed(invocation);
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when
                    (Repeat(invocation.Method, ex) && Retry[i] != InfiniteTimeSpan)
                {
                    Thread.Sleep(Retry[i]);
                }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            for (int i = 1; ; i++)
                try
                {
                    Cancellation.ThrowIfRequested();
                    return await proceed(invocation);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when
                    (Repeat(invocation.Method, ex) && Retry[i] != InfiniteTimeSpan)
                {
                    Thread.Sleep(Retry[i]);
                }
        }
    }
}
