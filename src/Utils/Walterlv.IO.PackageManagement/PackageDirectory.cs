using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Walterlv.IO.PackageManagement.Core;

namespace Walterlv.IO.PackageManagement
{
    /// <summary>
    /// 包含对文件夹操作的封装，避免在业务中过多关注异常、跨驱动器、递归等本不希望关心的问题。
    /// </summary>
    public static class PackageDirectory
    {
        /// <summary>
        /// 为指定的路径创建文件夹。
        /// </summary>
        /// <param name="directory">文件夹的路径。</param>
        public static void Create(string directory) => Create(
            VerifyDirectoryArgument(directory, nameof(directory)));

        /// <summary>
        /// 将源路径文件夹移动成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">是否覆盖。如果覆盖，那么目标文件夹中的原有文件将全部删除。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Move(string sourceDirectory, string targetDirectory, bool overwrite) => Move(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite ? DirectoryOverwriteStrategy.Overwrite : DirectoryOverwriteStrategy.DoNotOverwrite);

        /// <summary>
        /// 将源路径文件夹移动成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">指定当目标路径存在现成现成文件夹时，应该如何覆盖目标文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Move(string sourceDirectory, string targetDirectory, DirectoryOverwriteStrategy overwrite = DirectoryOverwriteStrategy.Overwrite) => Move(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite);

        /// <summary>
        /// 将源路径文件夹复制成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">是否覆盖。如果覆盖，那么目标文件夹中的原有文件将全部删除。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Copy(string sourceDirectory, string targetDirectory, bool overwrite) => Copy(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite ? DirectoryOverwriteStrategy.Overwrite : DirectoryOverwriteStrategy.DoNotOverwrite);

        /// <summary>
        /// 将源路径文件夹复制成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">指定当目标路径存在现成现成文件夹时，应该如何覆盖目标文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Copy(string sourceDirectory, string targetDirectory, DirectoryOverwriteStrategy overwrite = DirectoryOverwriteStrategy.Overwrite) => Copy(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite);

        /// <summary>
        /// 删除指定路径的文件夹，此操作会递归删除文件夹内的所有文件，最后删除此文件夹自身。
        /// 如果目标文件夹是个连接点（Junction Point, Symbolic Link），则只会删除连接点而已，不会删除连接点所指目标文件夹中的文件。
        /// </summary>
        /// <param name="directory">要删除的文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Delete(string directory) => Delete(
            VerifyDirectoryArgument(directory, nameof(directory)));

        /// <summary>
        /// 创建一个目录联接（Junction Point），并连接到 <paramref name="targetDirectory"/> 指向的路径。
        /// </summary>
        /// <param name="linkDirectory">连接点的路径。</param>
        /// <param name="targetDirectory">要连接的目标文件夹。</param>
        /// <param name="overwrite">如果要创建的连接点路径已经存在连接点或者文件夹，则指定是否删除这个现有的连接点或文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Link(string linkDirectory, string targetDirectory, bool overwrite = true) => Link(
            VerifyDirectoryArgument(linkDirectory, nameof(linkDirectory)),
            VerifyDirectoryArgument(targetDirectory, nameof(targetDirectory)),
            overwrite);

        /// <summary>
        /// 为指定的路径创建文件夹。
        /// </summary>
        /// <param name="directory">文件夹的路径。</param>
        public static IOResult Create(DirectoryInfo directory)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            var logger = new IOResult();
            logger.Log("无论是否存在，都创建文件夹。");
            Directory.CreateDirectory(directory.FullName);
            return logger;
        }

        /// <summary>
        /// 将源路径文件夹移动成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">是否覆盖。如果覆盖，那么目标文件夹中的原有文件将全部删除。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Move(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwrite) => Move(
            sourceDirectory, targetDirectory,
            overwrite ? DirectoryOverwriteStrategy.Overwrite : DirectoryOverwriteStrategy.DoNotOverwrite);

        /// <summary>
        /// 将源路径文件夹移动成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">指定当目标路径存在现成现成文件夹时，应该如何覆盖目标文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Move(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, DirectoryOverwriteStrategy overwrite = DirectoryOverwriteStrategy.Overwrite)
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
                    logger.Log("无论是否存在，都创建文件夹。");
                    Directory.CreateDirectory(targetDirectory.FullName);

                    foreach (var file in sourceDirectory.EnumerateFiles())
                    {
                        var targetFilePath = Path.Combine(targetDirectory.FullName, file.Name);
                        file.MoveTo(targetFilePath);
                    }

                    foreach (DirectoryInfo directory in sourceDirectory.EnumerateDirectories())
                    {
                        var targetDirectoryPath = Path.Combine(targetDirectory.FullName, directory.Name);
                        var moveResult = Move(directory, new DirectoryInfo(targetDirectoryPath));
                        logger.Append(moveResult);
                    }
                }
                catch (Exception ex)
                {
                    logger.Fail(ex);
                    return logger;
                }
            }
            else
            {
                logger.Log("源目录与目标目录不在相同驱动器，先进行复制，再删除源目录。");

                var copyResult = Copy(sourceDirectory, targetDirectory);
                logger.Append(copyResult);

                var deleteResult = Delete(sourceDirectory);
                logger.Append(deleteResult);
            }
            return logger;
        }

        /// <summary>
        /// 将源路径文件夹复制成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">是否覆盖。如果覆盖，那么目标文件夹中的原有文件将全部删除。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Copy(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwrite) => Copy(
            sourceDirectory, targetDirectory,
            overwrite ? DirectoryOverwriteStrategy.Overwrite : DirectoryOverwriteStrategy.DoNotOverwrite);

        /// <summary>
        /// 将源路径文件夹复制成为目标路径文件夹。
        /// 由 <paramref name="overwrite"/> 参数指定在目标文件夹存在时应该覆盖还是将源文件夹全部删除。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹。</param>
        /// <param name="targetDirectory">目标文件夹。</param>
        /// <param name="overwrite">指定当目标路径存在现成现成文件夹时，应该如何覆盖目标文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Copy(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, DirectoryOverwriteStrategy overwrite = DirectoryOverwriteStrategy.Overwrite)
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
                logger.Log("无论是否存在，都创建文件夹。");
                Directory.CreateDirectory(targetDirectory.FullName);

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

        /// <summary>
        /// 删除指定路径的文件夹，此操作会递归删除文件夹内的所有文件，最后删除此文件夹自身。
        /// 如果目标文件夹是个连接点（Junction Point, Symbolic Link），则只会删除连接点而已，不会删除连接点所指目标文件夹中的文件。
        /// </summary>
        /// <param name="directory">要删除的文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
        public static IOResult Delete(DirectoryInfo directory)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            var logger = new IOResult();
            logger.Log($"删除目录“{directory.FullName}”。");

            if (JunctionPoint.Exists(directory.FullName))
            {
                JunctionPoint.Delete(directory.FullName);
            }
            else if (!Directory.Exists(directory.FullName))
            {
                logger.Log($"要删除的目录“{directory.FullName}”不存在。");
                return logger;
            }

            Delete(directory, 0, logger);

            static void Delete(DirectoryInfo directory, int depth, IOResult logger)
            {
                if (JunctionPoint.Exists(directory.FullName))
                {
                    JunctionPoint.Delete(directory.FullName);
                }
                else if (!Directory.Exists(directory.FullName))
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

        /// <summary>
        /// 创建一个目录联接（Junction Point），并连接到 <paramref name="targetDirectory"/> 指向的路径。
        /// </summary>
        /// <param name="linkDirectory">连接点的路径。</param>
        /// <param name="targetDirectory">要连接的目标文件夹。</param>
        /// <param name="overwrite">如果要创建的连接点路径已经存在连接点或者文件夹，则指定是否删除这个现有的连接点或文件夹。</param>
        /// <returns>包含执行成功和失败的信息，以及中间执行中方法自动决定的一些细节。</returns>
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

        private static IOResult DeleteIfOverwrite(DirectoryInfo targetDirectory, DirectoryOverwriteStrategy overwrite)
        {
            var logger = new IOResult();
            if (Directory.Exists(targetDirectory.FullName))
            {
                switch (overwrite)
                {
                    case DirectoryOverwriteStrategy.DoNotOverwrite:
                        {
                            logger.Log("目标目录已经存在，但是要求不被覆盖，抛出异常。");
                            throw new IOException($"目标目录“{targetDirectory.FullName}”已经存在，如要覆盖，请设置 {nameof(overwrite)} 为 true。");
                        }
                    case DirectoryOverwriteStrategy.Overwrite:
                        {
                            logger.Log("目标目录已经存在，删除。");
                            var deleteResult = Delete(targetDirectory.FullName);
                            logger.Append(deleteResult);
                        }
                        break;
                    case DirectoryOverwriteStrategy.MergeOverwrite:
                        {
                            // 如果是合并式覆盖，那么不需要删除，也不需要抛异常，直接覆盖即可。
                        }
                        break;
                    default:
                        break;
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
