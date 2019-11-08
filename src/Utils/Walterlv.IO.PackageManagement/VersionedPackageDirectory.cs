using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Walterlv.IO.PackageManagement
{
    public class VersionedPackageDirectory
    {
        private const string CurrentDirectoryName = "current";
        private readonly DirectoryInfo _rootDirectory;

        public VersionedPackageDirectory(string rootDirectory)
            : this(VerifyDirectoryArgument(rootDirectory, nameof(rootDirectory))) { }

        public VersionedPackageDirectory(DirectoryInfo rootDirectory)
        {
            _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        public string RootDirectory => _rootDirectory.FullName;

        /// <summary>
        /// 将一个文件夹移动到特定版本的包文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要移动的文件夹。</param>
        /// <param name="version">移动文件夹需要移动到此版本号对应的包文件夹中。</param>
        public void Move(string sourceDirectory, string version) => Move(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)), version);

        /// <summary>
        /// 将一个文件夹移动到特定版本的包文件夹中，然后让当前版本联接到此文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要移动的文件夹。</param>
        /// <param name="version">移动文件夹需要移动到此版本号对应的包文件夹中。</param>
        public void MoveAsCurrent(string sourceDirectory, string version) => MoveAsCurrent(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)), version);

        /// <summary>
        /// 将一个文件夹复制到特定版本的包文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要复制的文件夹。</param>
        /// <param name="version">复制文件夹需要复制到此版本号对应的包文件夹中。</param>
        public void Copy(string sourceDirectory, string version) => Copy(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)), version);

        /// <summary>
        /// 将一个文件夹复制到特定版本的包文件夹中，然后让当前版本联接到此文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要复制的文件夹。</param>
        /// <param name="version">复制文件夹需要复制到此版本号对应的包文件夹中。</param>
        public void CopyAsCurrent(string sourceDirectory, string version) => CopyAsCurrent(
            VerifyDirectoryArgument(sourceDirectory, nameof(sourceDirectory)), version);

        /// <summary>
        /// 将一个文件夹移动到特定版本的包文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要移动的文件夹。</param>
        /// <param name="version">移动文件夹需要移动到此版本号对应的包文件夹中。</param>
        public void Move(DirectoryInfo sourceDirectory, string version)
        {
            var targetDirectory = GetVersionDirectory(version, false);
            PackageDirectory.Move(sourceDirectory, targetDirectory);
        }

        /// <summary>
        /// 将一个文件夹移动到特定版本的包文件夹中，然后让当前版本联接到此文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要移动的文件夹。</param>
        /// <param name="version">移动文件夹需要移动到此版本号对应的包文件夹中。</param>
        public void MoveAsCurrent(DirectoryInfo sourceDirectory, string version)
        {
            var targetDirectory = GetVersionDirectory(version, false);
            var currentDirectory = GetVersionDirectory(CurrentDirectoryName, false);
            PackageDirectory.Move(sourceDirectory, targetDirectory);
            PackageDirectory.LinkOrMirror(currentDirectory, targetDirectory);
        }

        /// <summary>
        /// 将一个文件夹复制到特定版本的包文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要复制的文件夹。</param>
        /// <param name="version">复制文件夹需要复制到此版本号对应的包文件夹中。</param>
        public void Copy(DirectoryInfo sourceDirectory, string version)
        {
            var targetDirectory = GetVersionDirectory(version, false);
            PackageDirectory.Copy(sourceDirectory, targetDirectory);
        }

        /// <summary>
        /// 将一个文件夹复制到特定版本的包文件夹中，然后让当前版本联接到此文件夹中。
        /// </summary>
        /// <param name="sourceDirectory">要复制的文件夹。</param>
        /// <param name="version">复制文件夹需要复制到此版本号对应的包文件夹中。</param>
        public void CopyAsCurrent(DirectoryInfo sourceDirectory, string version)
        {
            var targetDirectory = GetVersionDirectory(version, false);
            var currentDirectory = GetVersionDirectory(CurrentDirectoryName, false);
            PackageDirectory.Copy(sourceDirectory, targetDirectory);
            PackageDirectory.LinkOrMirror(currentDirectory, targetDirectory);
        }

        /// <summary>
        /// 删除特定版本的包文件夹。
        /// 如果当前版本就是正准备删除的版本（已建立目录联接），则也会同时删除当前版本（断开目录联接）。
        /// 如果当前版本只是某个版本的副本（通常是因为管理分区在 NTFS 分区），则不会收到影响。
        /// </summary>
        /// <param name="version">要删除的版本号。</param>
        public void Delete(string version)
        {
            var directory = GetVersionDirectory(version, false);
            PackageDirectory.Delete(directory);
        }

        /// <summary>
        /// 尝试删除其他版本的文件夹，如果文件夹被占用，则忽略。
        /// 此操作会导致当前版本联接到此版本的文件夹中。
        /// </summary>
        /// <param name="version">要保留的版本号。</param>
        public void DeleteOthers(string version)
        {
            foreach (var directory in _rootDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                if (directory.Name != version && CurrentDirectoryName.Equals(directory.Name, StringComparison.OrdinalIgnoreCase))
                {
                    PackageDirectory.Delete(directory);
                }
            }
        }

        /// <summary>
        /// 删除所有版本的包文件夹，然后删除此包文件夹自身。
        /// </summary>
        public void DeleteAll() => PackageDirectory.Delete(_rootDirectory);

        /// <summary>
        /// 获取特定版本的目录信息。
        /// </summary>
        /// <param name="version">版本号。</param>
        /// <param name="ensureExists">如果此方法要求目录必须存在，则传入 true；否则传入 false。</param>
        /// <returns>一个表示此目录的目录信息对象。</returns>
        private DirectoryInfo GetVersionDirectory(string version, bool ensureExists)
        {
            var directory = Path.Combine(_rootDirectory.FullName, version);
            if (ensureExists)
            {
                PackageDirectory.Create(directory);
            }
            return new DirectoryInfo(directory);
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
