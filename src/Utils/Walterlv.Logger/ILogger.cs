using System;
using System.Runtime.CompilerServices;

namespace Walterlv.Logging
{
    /// <summary>
    /// 提供记录日志的方法。
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录每一个方法的执行分支，确保仅通过日志文件就能还原代码的执行过程。
        /// </summary>
        /// <param name="text">描述当前步骤正准备做什么。如果某个步骤耗时较长或容易出现异常，建议在结束后也记录一次。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        void Trace(string text, [CallerMemberName] string? callerMemberName = null);

        /// <summary>
        /// 记录一个关键步骤执行完成之后的摘要，部分耗时的关键步骤也需要在开始之前记录一些摘要。
        /// </summary>
        /// <param name="text">
        /// 描述当前步骤完成之后做了什么关键性的更改，关键的状态变化是什么。
        /// 描述当前步骤开始之前程序是一个什么样的状态，关键的状态是什么。
        /// </param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        void Message(string text, [CallerMemberName] string? callerMemberName = null);

        /// <summary>
        /// 如果方法进入了非预期的分支，请调用此方法以便在日志文件中高亮显示。
        /// </summary>
        /// <param name="message">描述当前进入的代码分支。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        void Warning(string message, [CallerMemberName] string? callerMemberName = null);

        /// <summary>
        /// 在你无法通过异常来描述错误的时候，使用字符串来记录一条错误。
        /// 建议使用 <see cref="Warning(string, string)"/> 来表达此含义，因为 <see cref="LogLevel.Warning"/> 表达的就是方法在进入非预期分支的情况。
        /// 如果此时你能捕获一个异常，建议使用异常重载 <see cref="Error(Exception, string, string)"/>，而不是使用直接使用字符串。
        /// </summary>
        /// <param name="message">错误信息。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        void Error(string message, [CallerMemberName] string? callerMemberName = null);

        /// <summary>
        /// 在单独的日志文件中记录异常，并同时在普通的日志文件中插入一段高亮显示的日志。
        /// 请注意，并不是所有的异常都需要调用此方法记录，此方法仅仅记录非预期的异常。
        /// </summary>
        /// <param name="exception">异常实例。</param>
        /// <param name="message">对当前异常的文字描述。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        void Error(Exception exception, string? message = null, [CallerMemberName] string? callerMemberName = null);

        /// <summary>
        /// 在单独的日志文件中记录一条导致致命性错误的异常，并同时在普通的日志文件中插入一段高亮显示的致命错误标记。
        /// 请注意，仅在全局区域记录此异常，全局区域如果还能收到异常说明方法内部有未处理的异常。
        /// </summary>
        /// <param name="exception">异常实例。</param>
        /// <param name="message">对当前异常的文字描述。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        void Fatal(Exception exception, string message, [CallerMemberName] string? callerMemberName = null);
    }
}
