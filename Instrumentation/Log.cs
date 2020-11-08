using System;
using System.Collections.Generic;
using System.Threading;

namespace Instrumentation
{
    public class Log : IDisposable
    {
        static AsyncLocal<Log> Context { get; } = new AsyncLocal<Log>();

        public Log(
            Action<string, Dictionary<string, object>> onDebug,
            Action<string, Dictionary<string, object>> onInfo,
            Action<string, Dictionary<string, object>> onWarning,
            Action<string, Exception, Dictionary<string, object>> onError)
        {
            Parent = Context.Value;
            Context.Value = this;

            OnDebug = onDebug;
            OnInfo = onInfo;
            OnWarning = onWarning;
            OnError = onError;
        }

        public virtual void Dispose()
        {
            Context.Value = Parent;
        }

        Log Parent { get; }
        Action<string, Dictionary<string, object>> OnDebug { get; }
        Action<string, Dictionary<string, object>> OnInfo { get; }
        Action<string, Dictionary<string, object>> OnWarning { get; }
        Action<string, Exception, Dictionary<string, object>> OnError { get; }

        public static void Debug(string text, params (string Name, object Value)[] tags)
        {
            using (new Tags(tags))
                Context.Value?.OnDebug(text, Tags.ToDictionary());
        }

        public static void Info(string text, params (string Name, object Value)[] tags)
        {
            using (new Tags(tags))
                Context.Value?.OnInfo(text, Tags.ToDictionary());
        }

        public static void Warning(string text, params (string Name, object Value)[] tags)
        {
            using (new Tags(tags))
                Context.Value?.OnWarning(text, Tags.ToDictionary());
        }

        public static void Error(string text, Exception ex, params (string Name, object Value)[] tags)
        {
            using (new Tags(tags))
                Context.Value?.OnError(text, ex, Tags.ToDictionary());
        }
    }
}
