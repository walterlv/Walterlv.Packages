using System;
using System.Collections.Generic;

namespace Walterlv.Logging.Markdown
{
    /// <summary>
    /// 表示一个 Markdown 转日志表格的数据模板。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkdownDataTemplate<T>
    {
        /// <summary>
        /// 指示如何将数据转为 Markdown 表格中的一列。
        /// </summary>
        /// <returns>表格中的列头和此列的转换委托。</returns>
        IDictionary<string, Func<T, string>> ToDictionary();
    }
}
