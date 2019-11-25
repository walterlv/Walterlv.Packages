using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Walterlv.Logging.Markdown
{
    /// <summary>
    /// 为 <see cref="ILogger"/> 提供 Markdown 风格的格式化输出。
    /// </summary>
    public static class MarkdownLogFormatter
    {
        /// <summary>
        /// 记录一份带格式的数据。
        /// </summary>
        /// <param name="logger">日志。</param>
        /// <param name="text">对此带格式数据的简短描述。</param>
        /// <param name="lang">代码的语言标识。</param>
        /// <param name="code">代码正文。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        public static void TraceCode(this ILogger logger, string text, string lang, string code, [CallerMemberName] string? callerMemberName = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.Trace($@"{text}
```{lang}
{code}
```", callerMemberName);
        }

        /// <summary>
        /// 使用表格的形式记录一份数据。
        /// </summary>
        /// <param name="logger">日志。</param>
        /// <param name="text">表头，或者对此表格摘要的简短描述。</param>
        /// <param name="items">要记录的表格。集合中的每一个元素是表格中的一行。</param>
        /// <param name="columnFormatter">表格的列描述。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        public static void TraceTable<T>(this ILogger logger, string text,
            IReadOnlyList<T> items, IDictionary<string, Func<T, string>> columnFormatter,
            [CallerMemberName] string? callerMemberName = null)
            where T : notnull
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var table = MakeTable(items, columnFormatter);
            logger.Message($@"{text}
{table}", callerMemberName);
        }

        /// <summary>
        /// 记录一份关键的带格式的数据。
        /// </summary>
        /// <param name="logger">日志。</param>
        /// <param name="text">对此带格式数据的简短描述。</param>
        /// <param name="lang">代码的语言标识。</param>
        /// <param name="code">代码正文。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        public static void MessageCode(this ILogger logger, string text, string lang, string code, [CallerMemberName] string? callerMemberName = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.Message($@"{text}
```{lang}
{code}
```", callerMemberName);
        }

        /// <summary>
        /// 使用表格的形式记录一份关键的摘要。
        /// </summary>
        /// <typeparam name="T">要格式化成表格的集合中的元素类型。</typeparam>
        /// <param name="logger">日志。</param>
        /// <param name="text">表头，或者对此表格摘要的简短描述。</param>
        /// <param name="items">要记录的表格。集合中的每一个元素是表格中的一行。</param>
        /// <param name="columnFormatter">表格的列描述。</param>
        /// <param name="callerMemberName">编译器自动传入。</param>
        public static void MessageTable<T>(this ILogger logger, string text,
            IReadOnlyList<T> items, IDictionary<string, Func<T, string>> columnFormatter,
            [CallerMemberName] string? callerMemberName = null)
            where T : notnull
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var table = MakeTable(items, columnFormatter);
            logger.Message($@"{text}
{table}", callerMemberName);
        }

        /// <summary>
        /// 创建表格。
        /// </summary>
        /// <typeparam name="T">要格式化成表格的集合中的元素类型。</typeparam>
        /// <param name="items">要记录的表格。集合中的每一个元素是表格中的一行。</param>
        /// <param name="columnFormatter">表格的列描述。</param>
        /// <returns>Markdown 格式的表格。</returns>
        private static string MakeTable<T>(IReadOnlyList<T> items, IDictionary<string, Func<T, string>> columnFormatter) where T : notnull
        {
            var builder = new StringBuilder();
            // 列数。
            var columnCount = columnFormatter.Count;
            // 每一列的名称。
            var headers = columnFormatter.Keys.ToArray();
            // 用于计算列值的函数。
            var funcs = columnFormatter.Values.ToArray();
            // 表头宽度。
            var headerWidths = columnFormatter.Keys.Select(x => x.Length).ToList();
            // 列宽（仅初始化，尚未确定，即将计算）。
            var columnWidths = new int[columnCount];

            // 计算列宽。
            for (var i = 0; i < columnCount; i++)
            {
                columnWidths[i] = Math.Max(headers[i].Length, items.Max(x => funcs[i](x).Length));
            }

            // 输出表头。
            builder.Append('|');
            for (var i = 0; i < columnCount; i++)
            {
                builder.Append($" {headers[i].PadRight(columnWidths[i], ' ')} |");
            }
            builder.Append('\n');

            // 输出表分隔符和对齐方式。
            builder.Append('|');
            builder.Append($":{"".PadRight(columnWidths[0], '-')} |");
            for (var i = 1; i < columnCount; i++)
            {
                builder.Append($" {"".PadRight(columnWidths[i], '-')}:|");
            }
            builder.Append('\n');

            // 输出表格内容（行）。
            foreach (var item in items)
            {
                builder.Append('|');
                builder.Append($" {funcs[0](item).PadRight(columnWidths[0], ' ')} |");
                for (var i = 1; i < columnCount; i++)
                {
                    builder.Append($" {funcs[i](item).PadLeft(columnWidths[i], ' ')} |");
                }
                builder.Append('\n');
            }

            return builder.ToString();
        }
    }
}
