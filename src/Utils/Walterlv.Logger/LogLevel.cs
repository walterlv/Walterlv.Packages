namespace Walterlv.Logging
{
    /// <summary>
    /// 表示日志记录等级。
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 表示不记录日志。
        /// </summary>
        None = 0x00,

        /// <summary>
        /// 表示仅记录崩溃日志。
        /// </summary>
        Fatal = 0x01,

        /// <summary>
        /// 表示仅记录错误、异常和崩溃。
        /// </summary>
        Error = 0x02,

        /// <summary>
        /// 表示仅记录警告、错误、异常和崩溃。
        /// </summary>
        Warning = 0x03,

        /// <summary>
        /// 表示记录信息、警告、错误、异常和崩溃。
        /// </summary>
        Message = 0x04,

        /// <summary>
        /// 表示记录所有日志，包括追踪方法中每一个分支变化的 <see cref="ILogger.Trace"/>。
        /// </summary>
        Detail = 0xf0,
    }
}
