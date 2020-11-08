using System;
using System.Diagnostics;
using System.Threading;

namespace Instrumentation
{
    public class Stat : IDisposable
    {
        static AsyncLocal<Stat> Context { get; } = new AsyncLocal<Stat>();

        public Stat(
            Action<string, string[]> onIncrement,
            Action<string, double, string[]> onGauge)
        {
            Parent = Context.Value;
            Context.Value = this;

            OnIncrement = onIncrement;
            OnGauge = onGauge;
        }

        public virtual void Dispose()
        {
            Context.Value = Parent;
        }

        Stat Parent { get; }
        Action<string, string[]> OnIncrement { get; }
        Action<string, double, string[]> OnGauge { get; }

        public static void Increment(string text, params string[] tags)
        {
            using (new Tags(tags))
                Context.Value?.OnIncrement(text, Tags.ToArray());
        }

        public static void Gauge(string text, Stopwatch value, params string[] tags) =>
            Gauge(text, value.ElapsedMilliseconds, tags);
        public static void Gauge(string text, double value, params string[] tags)
        {
            using (new Tags(tags))
                Context.Value?.OnGauge(text, value, Tags.ToArray());
        }
    }
}
