#if NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.IO;

/// <summary>
/// 包含文件移动冲突发生时，如果解决这个冲突的相关信息。
/// </summary>
public class FileMergeResolvingInfo
{
    private string? _resolvedSourceFilePath;
    private string? _resolvedTargetFilePath;

    /// <summary>
    /// 以默认冲突解决行为（覆盖目标文件）创建 <see cref="FileMergeResolvingInfo"/> 的新实例。
    /// </summary>
    /// <param name="targetDirectory">目标文件所在的文件夹。</param>
    /// <param name="targetFile">目标文件。</param>
    public FileMergeResolvingInfo(DirectoryInfo targetDirectory, FileInfo targetFile)
    {
        Strategy = FileMergeStrategy.KeepSource;
        TargetDirectory = targetDirectory;
        var desiredPath = targetFile.FullName;
        DesiredSourceFilePath = desiredPath;
        ResolvedSourceFilePath = desiredPath;
        DesiredTargetFilePath = desiredPath;
        ResolvedTargetFilePath = desiredPath;
    }

    /// <summary>
    /// 当移动文件发生冲突时的冲突解决策略。
    /// </summary>
    public FileMergeStrategy Strategy { get; set; }

    /// <summary>
    /// 针对同一个文件解决冲突时，此序号记录正尝试解决冲突的次数。
    /// 当首次需要解决冲突时，此值为 1；如果尝试解决冲突后依然发生了冲突，则下次解决冲突时此值会自增 1。
    /// </summary>
    public int TryingIndex { get; private set; }

    /// <summary>
    /// 尝试将源文件移动到的目标文件路径。
    /// </summary>
    public string DesiredSourceFilePath { get; }

    /// <summary>
    /// 在解决冲突时，设置此值以指定新的源文件即将移动到的新路径。
    /// </summary>
#if NETCOREAPP3_0_OR_GREATER
    [AllowNull]
#endif
    public string ResolvedSourceFilePath
    {
        get => _resolvedSourceFilePath ?? DesiredSourceFilePath;
        set => _resolvedSourceFilePath = value;
    }

    /// <summary>
    /// 目标文件所在的文件夹。
    /// </summary>
    public DirectoryInfo TargetDirectory { get; }

    /// <summary>
    /// 尝试将源文件移动到目标时，发生冲突的目标文件的文件路径。
    /// </summary>
    public string DesiredTargetFilePath { get; }

    /// <summary>
    /// 在解决冲突时，设置此值以指定新的目标文件的新路径。
    /// 指定此值可以在不改变源文件的目标路径时解决冲突以完成复制。
    /// </summary>
#if NETCOREAPP3_0_OR_GREATER
    [AllowNull]
#endif
    public string ResolvedTargetFilePath
    {
        get => _resolvedTargetFilePath ?? DesiredTargetFilePath;
        set => _resolvedTargetFilePath = value;
    }

    /// <summary>
    /// 在源文件的文件名末尾，扩展名之前添加数字编号来尝试避免冲突。
    /// 此方法将遍历目标文件夹下的文件，寻找不冲突的第一个带有编号的文件路径。
    /// 如果遍历次数足够多也未能找到不冲突的文件路径，则返回 null。
    /// </summary>
    /// <returns></returns>
    public string? GetNotExistTargetFilePath()
    {
        for (var i = 1; i < ushort.MaxValue; i++)
        {
            var fileName = Path.GetFileNameWithoutExtension(DesiredTargetFilePath);
            var ext = Path.GetExtension(DesiredTargetFilePath);
            var newPath = Path.Combine(TargetDirectory.FullName, fileName + $".{i}" + ext);
            if (!File.Exists(newPath))
            {
                return newPath;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据 <see cref="TryingIndex"/> 的值来猜测一个目标文件路径，不保证此值一定能解决冲突。
    /// </summary>
    /// <returns></returns>
    public string GetNextTargetFilePath()
    {
        var fileName = Path.GetFileNameWithoutExtension(DesiredTargetFilePath);
        var ext = Path.GetExtension(DesiredTargetFilePath);
        var newPath = Path.Combine(TargetDirectory.FullName, fileName + $".{TryingIndex}" + ext);
        return newPath;
    }

    /// <summary>
    /// 增加序号 <see cref="TryingIndex"/>。
    /// </summary>
    internal void IncreaseRetryCount()
    {
        TryingIndex++;
    }
}
