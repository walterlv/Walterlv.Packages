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
        public void WriteInfoErrorLog()
        {
            "写入 Info 级别的日志，发现日志已被写入。".Test(() =>
            {
                var (logger, infoFile, errorFile) = PrepareFileNotExistLogger("info01.md", "error01.md");

                logger.Message("aaaa");
                logger.Message("bbbb");
                logger.Close();

                Assert.AreEqual(2, File.ReadAllLines(infoFile.FullName).Length);
                Assert.IsFalse(File.Exists(errorFile.FullName));
            });

            "写入 Error 级别的日志，发现日志已被写入。".Test(() =>
            {
                var (logger, infoFile, errorFile) = PrepareFileNotExistLogger("info02.md", "error02.md");

                logger.Error("aaaa");
                logger.Error("bbbb");
                logger.Close();

                Assert.AreEqual(2, File.ReadAllLines(infoFile.FullName).Length);
                Assert.AreEqual(2, File.ReadAllLines(errorFile.FullName).Length);
            });

            "写入 Info 和 Error 级别的日志，发现日志已被写入。".Test(() =>
            {
                var (logger, infoFile, errorFile) = PrepareFileNotExistLogger("info03.md", "error03.md");

                logger.Message("aaaa");
                logger.Error("bbbb");
                logger.Close();

                Assert.AreEqual(2, File.ReadAllLines(infoFile.FullName).Length);
                Assert.AreEqual(1, File.ReadAllLines(errorFile.FullName).Length);
            });
        }

        [ContractTestCase]
        public void ReadWriteShare()
        {
            "关闭日志写入后，文件确保全部写入完成。".Test(() =>
            {
                var (logger, testFile) = PrepareFileNotExistLogger("test11.md");

                logger.Message("aaaa");
                logger.Message("bbbb");
                logger.Close();

                Assert.AreEqual(2, File.ReadAllLines(testFile.FullName).Length);
            });

            "测试多个日志类读写同一个文件，所有内容都没丢。".Test(() =>
            {
                var (aLogger, testFile) = PrepareFileNotExistLogger("test12.md");
                var (bLogger, _) = PrepareFileNotExistLogger("test12.md");

                aLogger.Message("aaaa");
                bLogger.Message("bbbb");
                aLogger.Message("cccc");
                bLogger.Message("dddd");

                aLogger.Close();
                bLogger.Close();

                Assert.AreEqual(4, File.ReadAllLines(testFile.FullName).Length);
            });
        }

        [ContractTestCase]
        public void DeleteFileWhenInitialize()
        {
            "使用 With 初始化时，不能要求文件存在。".Test((Func<TextFileLogger, TextFileLogger> extraBuilder) =>
            {
                var testFile = PrepareNotExistFile("test21.md");

                var aLogger = extraBuilder(new TextFileLogger(new FileInfo(testFile.FullName)));
                aLogger.Message("YY");
                aLogger.Close();

                // 无异常。
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

        private static FileInfo PrepareNotExistFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            return new FileInfo(fileName);
        }

        private static (TextFileLogger logger, FileInfo loggerFile) PrepareFileNotExistLogger(string fileName)
        {
            var file = PrepareNotExistFile(fileName);
            return (new TextFileLogger(file), file);
        }

        private static (TextFileLogger logger, FileInfo infoFile, FileInfo errorFile) PrepareFileNotExistLogger(string infoFileName, string errorFileName)
        {
            var infoFile = PrepareNotExistFile(infoFileName);
            var errorFile = PrepareNotExistFile(errorFileName);
            return (new TextFileLogger(infoFile, errorFile), infoFile, errorFile);
        }
    }
}
