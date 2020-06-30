using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Extensions.Contracts;
using Walterlv.Logging.IO;

namespace Walterlv.Tests.Logging.IO
{
    [TestClass]
    public class TextFileLoggerTests
    {
        [ContractTestCase]
        public void ReadWriteShare()
        {
            "关闭日志写入后，文件确保全部写入完成。".Test(() =>
            {
                const string testFile = "test1.md";
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                }

                var aLogger = new TextFileLogger(new FileInfo(testFile));

                aLogger.Message("aaaa");
                aLogger.Message("bbbb");

                aLogger.Close();

                var newLines = File.ReadAllLines(testFile);
                Assert.AreEqual(2, newLines.Length);
            });

            "测试多个日志类读写同一个文件，所有内容都没丢。".Test(() =>
            {
                const string testFile = "test2.md";
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                }

                var aLogger = new TextFileLogger(new FileInfo(testFile));
                var bLogger = new TextFileLogger(new FileInfo(testFile));

                aLogger.Message("aaaa");
                bLogger.Message("bbbb");
                aLogger.Message("cccc");
                bLogger.Message("dddd");

                aLogger.Close();
                bLogger.Close();

                var newLines = File.ReadAllLines(testFile);
                Assert.AreEqual(4, newLines.Length);
            });
        }
    }
}
