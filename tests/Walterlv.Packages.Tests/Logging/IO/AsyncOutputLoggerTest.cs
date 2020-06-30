using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Extensions.Contracts;
using Walterlv.Logging.Core;

namespace Walterlv.Tests.Logging.IO
{
    [TestClass]
    public class AsyncOutputLoggerTest : AsyncOutputLogger
    {
        private readonly List<string> _logs = new List<string>();

        [ContractTestCase]
        public void WaitThreadSafe()
        {
            "关闭是线程安全的：中途加入了等待，那么会全部一起等待。".Test(() =>
            {
                var logger = new AsyncOutputLoggerTest();
                logger.Message("aaaa");
                logger.Message("bbbb");

                // 必须设置最小线程池数量，否则此单元测试将不能测试到并发。
                // 原理：
                //  - 假设测试机只有双核，那么最小线程数是 2
                //  - 那么一开始，下文的 task1 和 task2 开始执行
                //  - 但 task3 尝试执行时，将进入等待，直到超时才会开始执行，而超时时间是 1 秒
                //  - 这 1 秒，足以让单元测试的结果不一样
                //  - 单元测试不应该引入不确定量，因此我在测三线程的并发，就必须能并发出三个线程
                ThreadPool.GetMinThreads(out var w, out var c);
                ThreadPool.SetMinThreads(8, 8);

                var task1 = Task.Run(() =>
                {
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(6, logger._logs.Count);
                });
                var task2 = Task.Run(() =>
                {
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(6, logger._logs.Count);
                });
                var task3 = Task.Run(() =>
                {
                    Thread.Sleep(100);
                    logger.Message("cccc");
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(6, logger._logs.Count);
                });

                Task.WaitAll(task1, task2, task3);

                ThreadPool.SetMinThreads(w, c);
            });

            "关闭是线程安全的：等待完后新加入的等待，等待独立。".Test(() =>
            {
                var logger = new AsyncOutputLoggerTest();
                logger.Message("aaaa");
                logger.Message("bbbb");

                var task1 = Task.Run(() =>
                {
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(4, logger._logs.Count);
                });
                var task2 = Task.Run(() =>
                {
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(4, logger._logs.Count);
                });

                Task.WaitAll(task1, task2);

                var task3 = Task.Run(() =>
                {
                    logger.Message("cccc");
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(6, logger._logs.Count);
                });

                Task.WaitAll(task3);
            });
        }

        protected override Task OnInitializedAsync() => Task.CompletedTask;

        protected override void OnLogReceived(in LogContext context)
        {
            _logs.Add(context.Text);
            Thread.Sleep(1000);
            _logs.Add(context.Text);
        }
    }
}
