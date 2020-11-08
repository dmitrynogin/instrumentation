using System;
using static System.Threading.Timeout;

namespace Instrumentation
{
    public class Retry
    {
        public static readonly Retry Immediately = Constant(TimeSpan.Zero);
        public static readonly Retry Never = Constant(InfiniteTimeSpan);
        public static Retry Constant(TimeSpan interval) =>
            new Retry(i => interval);
        public static Retry Incremental(TimeSpan interval) =>
            new Retry(i => i * interval);
        public static Retry Exponential(TimeSpan interval) =>
            new Retry(i => Math.Pow(i, i) * interval);

        public Retry(Func<int, TimeSpan> strategy) => Strategy = strategy;
        Func<int, TimeSpan> Strategy { get; }        

        public TimeSpan this[int i] => Strategy(i);

        public Retry Limit(int max) =>
            new Retry(i => i < max ? Strategy(i) : InfiniteTimeSpan);
    }
}
