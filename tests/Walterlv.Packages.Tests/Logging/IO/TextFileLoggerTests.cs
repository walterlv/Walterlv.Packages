using System;
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

        [ContractTestCase]
        public void DeleteFileWhenInitialize()
        {
            "使用 With 初始化时，不能要求文件存在。".Test((Func<TextFileLogger, TextFileLogger> extraBuilder) =>
            {
                const string testFile = "test21.md";
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                }

                var aLogger = extraBuilder(new TextFileLogger(new FileInfo(testFile)));
                aLogger.Message("YY");
                aLogger.Close();
            }).WithArguments(
                x => x.WithMaxFileSize(100),
                x => x.WithMaxLineCount(100),
                x => x.WithMaxLineCount(100, 50),
                x => x.WithWholeFileOverride()
            );

            "初始化时，超过大小的文件内容会清空。".Test(() =>
            {
                const string testFile = "test22.md";
                File.WriteAllText(testFile, "XXXXXXXX\n");

                var aLogger = new TextFileLogger(new FileInfo(testFile))
                    .WithMaxFileSize(4);
                aLogger.Message("YY");
                aLogger.Close();

                var lines = File.ReadAllLines(testFile);
                Assert.AreEqual(1, lines.Length);
                Assert.IsTrue(lines[0].Contains("YY"));
            });

            "初始化时，超过行数的文件前面行会删除。".Test(() =>
            {
                const string testFile = "test23.md";
                File.WriteAllText(testFile, "XXXXXXXX\n\nYYYYYYYY\nZZZZZZZZ\n");

                var aLogger = new TextFileLogger(new FileInfo(testFile))
                    .WithMaxLineCount(3, 1);
                aLogger.Message("WW");
                aLogger.Close();

                var lines = File.ReadAllLines(testFile);
                Assert.AreEqual(2, lines.Length);
                Assert.IsTrue(lines[0].Contains("ZZZZZZZZ"));
            });
        }
    }
}
