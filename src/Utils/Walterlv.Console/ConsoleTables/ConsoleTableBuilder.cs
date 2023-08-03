using System;

namespace Walterlv.ForegroundWindowMonitor;

/// <summary>
/// 表示一个控制台表格的列定义。
/// </summary>
/// <typeparam name="T">表格中每一行的数据类型。</typeparam>
public readonly record struct ConsoleTableColumnDefinition<T> where T : notnull
{
    public ConsoleTableColumnDefinition(string text, Func<T, string> columnValueFormatter)
    {
        Width = text.Length;
        WidthPercent = 0;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ColumnValueFormatter = columnValueFormatter;
    }

    public ConsoleTableColumnDefinition(int width, string text, Func<T, string> columnValueFormatter)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
        }

        Width = width;
        WidthPercent = 0;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ColumnValueFormatter = columnValueFormatter;
    }

    public ConsoleTableColumnDefinition(double widthPercent, string text, Func<T, string> columnValueFormatter)
    {
        if (widthPercent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthPercent), widthPercent, "Width percent must be greater than 0.");
        }

        Width = 0;
        WidthPercent = widthPercent;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ColumnValueFormatter = columnValueFormatter;
    }

    /// <summary>
    /// 获取列的字符显示宽度。
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 获取列的字符显示宽度百分比。
    /// </summary>
    /// <remarks>
    /// 指定了 <see cref="Width"/> 的列不参与计算百分比，其他列按百分比分剩余宽度。
    /// <para/>
    /// 所有列宽度百分比的总和允许大于 100%。当大于时，会压缩每一列按百分比计算的宽度。
    /// </remarks>
    public double WidthPercent { get; }

    /// <summary>
    /// 获取列的标题。
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// 获取列的值格式化器。
    /// </summary>
    public Func<T, string> ColumnValueFormatter { get; }

    public static implicit operator ConsoleTableColumnDefinition<T>(string headerText)
    {
        return new ConsoleTableColumnDefinition<T>(headerText, v => v.ToString()!);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((int Width, string Text) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.Width, header.Text, v => v.ToString()!);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((int Width, string Text, Func<T, string> ColumnValueFormatter) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.Width, header.Text, header.ColumnValueFormatter);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((double WidthPercent, string Text) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.WidthPercent, header.Text, v => v.ToString()!);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((double WidthPercent, string Text, Func<T, string> ColumnValueFormatter) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.WidthPercent, header.Text, header.ColumnValueFormatter);
    }
}
