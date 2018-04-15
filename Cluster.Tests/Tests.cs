using System;
using NUnit.Framework;
using Cluster;
using Cluster.Mathematics;

namespace Cluster.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void test1()
        {
            Vec2 bla = new Vec2(1, 2);
            Vec2 bla2 = new Vec2(3, 4);
            Assert.NotZero(bla * bla2);
        }
    }
}