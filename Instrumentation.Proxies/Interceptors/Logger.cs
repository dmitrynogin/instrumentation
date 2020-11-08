using Castle.DynamicProxy;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Instrumentation.Interceptors
{
    class Logger : AsyncInterceptorBase
    {
        protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            var sw = Stopwatch.StartNew();
            var method = $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}";
            using (new Tags(("Method", method)))
                try
                {
                    Log.Debug($"{method} started");
                    try
                    {
                        await proceed(invocation).ConfigureAwait(false);
                    }
                    finally
                    {
                        sw.Stop();
                    }
                    using (new Tags(("Taken", sw.ElapsedMilliseconds)))
                    {
                        var e = $"{method} succeeded";
                        Log.Debug(e);
                        Stat.Increment(e);
                        Stat.Gauge(e, sw);
                    }
                }
                catch (Exception ex)
                {
                    using (new Tags(("Taken", sw.ElapsedMilliseconds)))
                    {
                        var e = $"{method} failed";
                        Log.Error(e, ex);
                        Stat.Increment(e);
                        Stat.Gauge(e, sw);
                        throw;
                    }
                }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            TResult result = default;
            var sw = Stopwatch.StartNew();
            var method = $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}";
            using (new Tags(("Method", method)))
                try
                {
                    Log.Debug($"{method} started");
                    try
                    {
                        result = await proceed(invocation).ConfigureAwait(false);
                    }
                    finally
                    {
                        sw.Stop();
                    }
                    using (new Tags(("Taken", sw.ElapsedMilliseconds)))
                    {
                        var e = $"{method} succeeded";
                        Log.Debug(e);
                        Stat.Increment(e);
                        Stat.Gauge(e, sw);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    using (new Tags(("Taken", sw.ElapsedMilliseconds)))
                    {
                        var e = $"{method} failed";
                        Log.Error(e, ex);
                        Stat.Increment(e);
                        Stat.Gauge(e, sw);
                        throw;
                    }
                }
        }
    }
}
