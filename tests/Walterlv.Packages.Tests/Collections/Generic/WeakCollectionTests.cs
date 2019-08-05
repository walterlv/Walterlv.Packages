using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Extensions.Contracts;
using Walterlv.Collections.Generic;

namespace Walterlv.Tests.Collections.Generic
{
    [TestClass]
    public class WeakCollectionTests
    {
        [ContractTestCase]
        public void WeakCollection()
        {
            "添加元素，但只有 1 个被强引用，于是 GC 后只剩 1 个元素。".Test(() =>
            {
                // Arrange
                var collection = new WeakCollection<object>();

                // Action
                collection.Add(new object());
                var a = new object();
                collection.Add(a);
                GC.Collect();

                // Assert
                Assert.AreEqual(1, collection.TryGetItems(x => true).Length);
            });

            "移除元素，被强引用的元素也被移除，于是 GC 后没有元素了。".Test(() =>
            {
                // Arrange
                var collection = new WeakCollection<object>();

                // Action
                collection.Add(new object());
                var a = new object();
                collection.Add(a);
                collection.Remove(a);
                GC.Collect();

                // Assert
                Assert.AreEqual(0, collection.TryGetItems(x => true).Length);
            });
        }
    }
}
