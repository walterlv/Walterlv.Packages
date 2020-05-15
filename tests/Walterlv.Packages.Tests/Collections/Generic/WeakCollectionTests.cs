using System;
using System.Collections.Generic;
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
                AddNewObject(collection);
                var a = AddNewObjectAndReturn(collection);
                GC.Collect();

                // Assert
                Assert.AreEqual(1, collection.TryGetItems(x => true).Length);

                // 必须在验证之后再使用一下变量，否则可能被提前回收。
                Console.WriteLine(a);
            });

            "移除元素，被强引用的元素也被移除，于是 GC 后没有元素了。".Test(() =>
            {
                // Arrange
                var collection = new WeakCollection<object>();

                // Action
                AddNewObject(collection);
                var a = AddNewObjectAndReturn(collection);
                collection.Remove(a);
                GC.Collect();

                // Assert
                Assert.AreEqual(0, collection.TryGetItems(x => true).Length);
            });
        }

        /// <summary>
        /// 创建一个 <typeparamref name="T"/> 的实例，然后加入到集合中。
        /// 必须调用这个方法创建，避免创建的局部变量被视为不能释放，详见：https://github.com/dotnet/runtime/issues/36265
        /// </summary>
        /// <typeparam name="T">要创建的实例的类型。</typeparam>
        /// <param name="collection">创建的实例加入到这个集合中。</param>
        private static void AddNewObject<T>(WeakCollection<T> collection) where T : class, new() => collection.Add(new T());

        /// <summary>
        /// 创建一个 <typeparamref name="T"/> 的实例，加入到集合中，然后将此实例返回。
        /// 必须调用这个方法创建并返回，否则你无法说明没释放是因为局部变量引用还是因为被测代码有问题，详见：https://github.com/dotnet/runtime/issues/36265
        /// </summary>
        /// <typeparam name="T">要创建的实例的类型。</typeparam>
        /// <param name="collection">创建的实例加入到这个集合中。</param>
        /// <returns>创建的新实例。</returns>
        private static object AddNewObjectAndReturn<T>(WeakCollection<T> collection) where T : class, new()
        {
            var t = new T();
            collection.Add(t);
            return t;
        }
    }
}
