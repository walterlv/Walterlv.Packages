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
                    Thread.Sleep(300);
                    logger.Message("cccc");
                    logger.WaitFlushingAsync().Wait();
                    Assert.AreEqual(6, logger._logs.Count);
                });

                Task.WaitAll(task1, task2, task3);
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
