using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Environment;

namespace Instrumentation
{
    public class AsLoggable_Should
    {
        [Test]
        public void Log_Success()
        {
            var sb = new StringBuilder();
            using var log = new Log(sb.Debug, sb.Info, sb.Warning, sb.Error);

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.Bar())
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            obj.Bar();

            foo
                .Verify(f => f.Bar(), Times.Once);

            Assert.IsTrue(Regex.IsMatch(
                sb.ToString(),
                @"Debug: Instrumentation.IFoo.Bar started. Method=Instrumentation.IFoo.Bar" + NewLine +
                @"Debug: Instrumentation.IFoo.Bar succeeded. Method=Instrumentation.IFoo.Bar, Taken=\d+"));
        }

        [Test]
        public void Log_Error()
        {
            var sb = new StringBuilder();
            using var log = new Log(sb.Debug, sb.Info, sb.Warning, sb.Error);

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.Bar())
                .Throws<Exception>()
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            Assert.Throws<Exception>(() => obj.Bar());

            foo
                .Verify(f => f.Bar(), Times.Once);

            Assert.IsTrue(Regex.IsMatch(
                sb.ToString(),
                @"Debug: Instrumentation.IFoo.Bar started. Method=Instrumentation.IFoo.Bar" + NewLine +
                @"Error: Instrumentation.IFoo.Bar failed. Exception of type 'System.Exception' was thrown. Method=Instrumentation.IFoo.Bar, Taken=\d+"));
        }

        [Test]
        public void Log_Success_Async()
        {
            var sb = new StringBuilder();
            using var log = new Log(sb.Debug, sb.Info, sb.Warning, sb.Error);

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.BarAsync())
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            obj.BarAsync();

            foo
                .Verify(f => f.BarAsync(), Times.Once);

            Assert.IsTrue(Regex.IsMatch(
                sb.ToString(),
                @"Debug: Instrumentation.IFoo.BarAsync started. Method=Instrumentation.IFoo.BarAsync" + NewLine +
                @"Debug: Instrumentation.IFoo.BarAsync succeeded. Method=Instrumentation.IFoo.BarAsync, Taken=\d+"));
        }

        [Test]
        public void Log_Error_Async()
        {
            var sb = new StringBuilder();
            using var log = new Log(sb.Debug, sb.Info, sb.Warning, sb.Error);

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.BarAsync())
                .Throws<Exception>()
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            Assert.ThrowsAsync<Exception>(async () => await obj.BarAsync());

            foo
                .Verify(f => f.BarAsync(), Times.Once);

            Assert.IsTrue(Regex.IsMatch(
                sb.ToString(),
                @"Debug: Instrumentation.IFoo.BarAsync started. Method=Instrumentation.IFoo.BarAsync" + NewLine +
                @"Error: Instrumentation.IFoo.BarAsync failed. Exception of type 'System.Exception' was thrown. Method=Instrumentation.IFoo.BarAsync, Taken=\d+"));
        }

        [Test]
        public void Count_Success()
        {
            var counter = 0;
            using var stat = new Stat(
                (e, p) => { Assert.AreEqual("Instrumentation.IFoo.Bar succeeded", e); counter++; },
                (e, d, p) => { Assert.AreEqual("Instrumentation.IFoo.Bar succeeded", e); counter++; });

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.Bar())
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            obj.Bar();

            foo
                .Verify(f => f.Bar(), Times.Once);

            Assert.AreEqual(2, counter);
        }

        [Test]
        public void Count_Error()
        {
            var counter = 0;
            using var stat = new Stat(
                (e, p) => { Assert.AreEqual("Instrumentation.IFoo.Bar failed", e); counter++; }, 
                (e, p, d) => { Assert.AreEqual("Instrumentation.IFoo.Bar failed", e); counter++; });

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.Bar())
                .Throws<Exception>()
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            Assert.Throws<Exception>(() => obj.Bar());

            foo
                .Verify(f => f.Bar(), Times.Once);

            Assert.AreEqual(2, counter);
        }

        [Test]
        public async Task Log_Task_Of_T_Return()
        {
            var sb = new StringBuilder();
            using var log = new Log(sb.Debug, sb.Info, sb.Warning, sb.Error);

            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.IntAsync())
                .ReturnsAsync(33)
                .Verifiable();

            var obj = foo.Object.AsLoggable();

            var i = await obj.IntAsync();
            Assert.AreEqual(33, i);

            foo
                .Verify(f => f.IntAsync(), Times.Once);

            Assert.IsTrue(Regex.IsMatch(
                sb.ToString(),
                @"Debug: Instrumentation.IFoo.IntAsync started. Method=Instrumentation.IFoo.IntAsync" + NewLine +
                @"Debug: Instrumentation.IFoo.IntAsync succeeded. Method=Instrumentation.IFoo.IntAsync, Taken=\d+"));
        }
    }

    static class Logging
    {
        public static void Debug(this StringBuilder sb, string e, Dictionary<string, object> p) =>
            sb.AppendLine("Debug: " + e + ". " + string.Join(", ", from kvp in p select $"{kvp.Key}={kvp.Value}"));
        public static void Info(this StringBuilder sb, string e, Dictionary<string, object> p) =>
            sb.AppendLine("Info: " + e + ". " + string.Join(", ", from kvp in p select $"{kvp.Key}={kvp.Value}"));
        public static void Warning(this StringBuilder sb, string e, Dictionary<string, object> p) =>
            sb.AppendLine("Warning: " + e + ". " + string.Join(", ", from kvp in p select $"{kvp.Key}={kvp.Value}"));
        public static void Error(this StringBuilder sb, string e, Exception ex, Dictionary<string, object> p) =>
            sb.AppendLine("Error: " + e + ". " + ex.Message + " " + string.Join(", ", from kvp in p select $"{kvp.Key}={kvp.Value}"));
    }
}