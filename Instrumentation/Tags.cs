using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Instrumentation
{
    public class Tags : IDisposable
    {
        static AsyncLocal<Tags> Context { get; } = new AsyncLocal<Tags>();

        public Tags(params string[] tags) : this() => List = tags;
        public Tags(params (string Name, object Value)[] tags) : this() => Dictionary = tags;
        Tags()
        {
            Parent = Context.Value;
            Context.Value = this;
        }

        public virtual void Dispose()
        {
            Context.Value = Parent;
        }

        Tags Parent { get; }
        IEnumerable<string> List { get; } = new string[0];
        IEnumerable<(string Name, object Value)> Dictionary { get; } = new (string, object)[0];

        public static Dictionary<string, object> ToDictionary()
        {
            return
                Context.Value == null ? new Dictionary<string, object>() :
                Merge(Context.Value)
                    .ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);

            IEnumerable<(string, object)> Merge(Tags tags) =>
                tags.Parent == null ? tags.Dictionary :
                Merge(tags.Parent)
                    .Concat(tags.Dictionary)
                    .Distinct();
        }

        public static string[] ToArray()
        {
            return
                Context.Value == null ? new string[0] :
                Merge(Context.Value);

            string[] Merge(Tags tags) =>
                tags.Parent == null ? tags.List.ToArray() :
                Merge(tags.Parent).Concat(tags.List).Distinct()
                    .ToArray();
        }
    }
}
