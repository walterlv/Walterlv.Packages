using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walterlv.Logging.Markdown
{
    /// <summary>
    /// 为 Markdown 表格提供 Builder 模式的格式化。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MarkdownDataTemplate<T> : IMarkdownDataTemplate<T>
    {
        /// <summary>
        /// 列记录。
        /// </summary>
        private readonly Dictionary<string, Func<T, string>> _columns = new Dictionary<string, Func<T, string>>();

        /// <summary>
        /// 为表格添加一列。
        /// </summary>
        /// <param name="columnHeader">
        /// 表头名称。不可为 null，如果不需要表头，请传入空字符串。
        /// </param>
        /// <param name="columnDataFormatter">
        /// 格式化表格的某项数据，一般取出其中的一项数据。如果传入 null，则直接将整个数据项作为数据。
        /// </param>
        /// <returns></returns>
        public MarkdownDataTemplate<T> AddColumn(string columnHeader, Func<T, string>? columnDataFormatter = null)
        {
            if (columnHeader is null)
            {
                throw new ArgumentNullException(nameof(columnHeader), "表格列头不可为 null，要显示空表头，请使用空字符串。");
            }

            _columns.Add(columnHeader, columnDataFormatter is null
                ? DefaultFormatter
                : columnDataFormatter);

            return this;
        }

        IDictionary<string, Func<T, string>> IMarkdownDataTemplate<T>.ToDictionary() => _columns;

        private static string DefaultFormatter(T data) => data?.ToString() ?? "";
    }
}
