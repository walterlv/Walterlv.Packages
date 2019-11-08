using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Walterlv.IO.PackageManagement.Core;

namespace Walterlv.IO.PackageManagement
{
    public static class PackageDirectory
    {
        public static void Create(string directory) => Create(
            VerifyDirectoryArgument(directory, nameof(directory)));

        public static IOResult Move(string sourceDirectory, string targetDirectory, bool overwrite = true) => Move(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite);

        public static IOResult Copy(string sourceDirectory, string targetDirectory, bool overwrite = true) => Copy(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite);

        public static IOResult Delete(string directory) => Delete(
            VerifyDirectoryArgument(directory, nameof(directory)));

        public static IOResult Link(string linkDirectory, string targetDirectory, bool overwrite = true) => Link(
            VerifyDirectoryArgument(linkDirectory, nameof(linkDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite);

        public static IOResult Create(DirectoryInfo directory)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            var logger = new IOResult();
            if (Directory.Exists(directory.FullName))
            {
                logger.Log("文件夹已经存在。");
            }
            else
            {
                logger.Log("文件夹不存在，创建。");
                Directory.CreateDirectory(directory.FullName);
            }
            return logger;
        }

        public static IOResult Move(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwrite = true)
        {
            if (sourceDirectory is null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (targetDirectory is null)
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            var logger = new IOResult();
            logger.Log($"移动目录，从“{sourceDirectory.FullName}”到“{targetDirectory.FullName}”。");

            if (!Directory.Exists(sourceDirectory.FullName))
            {
                logger.Log($"要移动的源目录“{sourceDirectory.FullName}”不存在。");
                return logger;
            }

            if (Path.GetPathRoot(sourceDirectory.FullName) == Path.GetPathRoot(targetDirectory.FullName))
            {
                logger.Log("源目录与目标目录在相同驱动器，直接移动。");

                var deleteOverwriteResult = DeleteIfOverwrite(targetDirectory, overwrite);
                logger.Append(deleteOverwriteResult);

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetDirectory.FullName));
                    Directory.Move(sourceDirectory.FullName, targetDirectory.FullName);
                }
                catch (Exception ex)
                {
                    logger.Fail(ex);
                    return logger;
                }
            }
            else
            {
                logger.Log("源目录与目标目录在相同驱动器，先进行复制，再删除源目录。");

                var copyResult = Copy(sourceDirectory, targetDirectory);
                logger.Append(copyResult);

                var deleteResult = Delete(sourceDirectory);
                logger.Append(deleteResult);
            }
            return logger;
        }

        public static IOResult Copy(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwrite = true)
        {
            if (sourceDirectory is null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (targetDirectory is null)
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            var logger = new IOResult();
            logger.Log($"复制目录，从“{sourceDirectory.FullName}”到“{targetDirectory.FullName}”。");

            if (!Directory.Exists(sourceDirectory.FullName))
            {
                logger.Log($"要复制的源目录“{sourceDirectory.FullName}”不存在。");
                return logger;
            }

            var deleteOverwriteResult = DeleteIfOverwrite(targetDirectory, overwrite);
            logger.Append(deleteOverwriteResult);

            try
            {
                if (!Directory.Exists(targetDirectory.FullName))
                {
                    Directory.CreateDirectory(targetDirectory.FullName);
                }

                foreach (var file in sourceDirectory.EnumerateFiles())
                {
                    var targetFilePath = Path.Combine(targetDirectory.FullName, file.Name);
                    file.CopyTo(targetFilePath, true);
                }

                foreach (DirectoryInfo directory in sourceDirectory.EnumerateDirectories())
                {
                    var targetDirectoryPath = Path.Combine(targetDirectory.FullName, directory.Name);
                    var copyResult = Copy(directory, new DirectoryInfo(targetDirectoryPath));
                    logger.Append(copyResult);
                }
            }
            catch (Exception ex)
            {
                logger.Fail(ex);
                return logger;
            }

            return logger;
        }

        public static IOResult Delete(DirectoryInfo directory)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            var logger = new IOResult();
            logger.Log($"删除目录“{directory.FullName}”。");

            if (!Directory.Exists(directory.FullName))
            {
                logger.Log($"要删除的目录“{directory.FullName}”不存在。");
                return logger;
            }

            Delete(directory, 0, logger);

            static void Delete(DirectoryInfo directory, int depth, IOResult logger)
            {
                if (!Directory.Exists(directory.FullName))
                {
                    return;
                }

                try
                {
                    foreach (var fileInfo in directory.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                    {
                        File.Delete(fileInfo.FullName);
                    }

                    foreach (var directoryInfo in directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        var back = string.Join("\\", Enumerable.Repeat("..", depth));
                        Delete(directoryInfo, depth + 1, logger);
                    }

                    Directory.Delete(directory.FullName);
                }
                catch (Exception ex)
                {
                    logger.Fail(ex);
                }
            }

            return logger;
        }

        public static IOResult Link(DirectoryInfo linkDirectory, DirectoryInfo targetDirectory, bool overwrite = true)
        {
            var logger = new IOResult();
            logger.Log($"创建目录联接，将“{linkDirectory.FullName}”联接到“{targetDirectory.FullName}”。");

            try
            {
                JunctionPoint.Create(linkDirectory.FullName, targetDirectory.FullName, overwrite);
            }
            catch (Exception ex)
            {
                logger.Fail(ex);
            }

            return logger;
        }

        public static IOResult LinkOrMirror(DirectoryInfo linkDirectory, DirectoryInfo targetDirectory, bool overwrite = true)
        {
            var logger = new IOResult();
            logger.Log($"创建目录联接，将“{linkDirectory.FullName}”联接到“{targetDirectory.FullName}”。");

            try
            {
                JunctionPoint.Create(linkDirectory.FullName, targetDirectory.FullName, overwrite);
            }
            catch
            {
                logger.Log($"不支持目录联接，将改用镜像备份。");
                var copyResult = Copy(targetDirectory, linkDirectory);
                logger.Append(copyResult);
            }

            return logger;
        }

        private static IOResult DeleteIfOverwrite(DirectoryInfo targetDirectory, bool overwrite)
        {
            var logger = new IOResult();
            if (Directory.Exists(targetDirectory.FullName))
            {
                if (overwrite)
                {
                    logger.Log("目标目录已经存在，删除。");
                    var deleteResult = Delete(targetDirectory.FullName);
                    logger.Append(deleteResult);
                }
                else
                {
                    logger.Log("目标目录已经存在，但是要求不被覆盖，抛出异常。");
                    throw new IOException($"目标目录“{targetDirectory.FullName}”已经存在，如要覆盖，请设置 {nameof(overwrite)} 为 true。");
                }
            }
            return logger;
        }

        [ContractArgumentValidator]
        private static DirectoryInfo VerifyDirectoryArgument(string directory, string argumentName)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("不允许使用空字符串作为目录。", argumentName);
            }

            return new DirectoryInfo(directory);
        }
    }
}
