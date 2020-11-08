using Moq;
using NUnit.Framework;
using System;

namespace Instrumentation
{
    public class AsResilient_Should
    {
        [Test]
        public void Repeat()
        {
            var foo = new Mock<IFoo>();
            foo
                .Setup(f => f.Bar())
                .Throws<Exception>()
                .Verifiable();

            var obj = foo.Object.AsResilient();

            Assert.Throws<Exception>(() => obj.Bar());

            foo
                .Verify(f => f.Bar(), Times.Exactly(3));
        }
   }
}