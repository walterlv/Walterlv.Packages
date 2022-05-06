using System;

/// <summary>
/// 表示移动文件发生冲突时的冲突解决策略类型。
/// </summary>
[Flags]
public enum FileMergeStrategy
{
    /// <summary>
    /// 默认值。如果枚举合并后的值为此值，那么源文件和目标文件都不保留，将全部尝试删除，除非无法删除。
    /// </summary>
    KeepNone = 0x00000000,

    /// <summary>
    /// 指定应保留源文件。
    /// </summary>
    KeepSource = 0x00000001,

    /// <summary>
    /// 指定应保留目标文件。
    /// </summary>
    KeepTarget = 0x00000002,

    /// <summary>
    /// 指定源文件和目标文件都应保留
    /// </summary>
    KeepBoth = KeepSource | KeepTarget,

    /// <summary>
    /// 一个标识位。
    /// 默认情况下，如果目标文件被占用，则会一直尝试解决冲突直到不再占用为止。
    /// 但如果启用此标记位，那么占用时将直接忽略而不复制源文件。
    /// </summary>
    IgnoreIfInUse = 0x00010000,
}
