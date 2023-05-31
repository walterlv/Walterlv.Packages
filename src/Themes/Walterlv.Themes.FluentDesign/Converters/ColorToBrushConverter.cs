using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Walterlv.Windows.Converters;
/// <summary>
/// ColorBrushConverter 类是一个值转换器，它可以将 Color 对象转换为 SolidColorBrush 对象，用于 WPF 绑定。
/// </summary>
internal class ColorToBrushConverter : IValueConverter
{
    /// <summary>
    /// 将 Color 对象转换为 SolidColorBrush 对象。
    /// </summary>
    /// <param name="value">源数据，应为 Color 对象。</param>
    /// <param name="targetType">绑定目标类型。</param>
    /// <param name="parameter">使用的转换参数。</param>
    /// <param name="culture">用于转换的区域性。</param>
    /// <returns>对应的 SolidColorBrush 对象，如果转换失败则返回 null。</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }

        return null;
    }

    /// <summary>
    /// 将 SolidColorBrush 对象转换为 Color 对象。
    /// </summary>
    /// <param name="value">源数据，应为 SolidColorBrush 对象。</param>
    /// <param name="targetType">绑定目标类型。</param>
    /// <param name="parameter">使用的转换参数。</param>
    /// <param name="culture">用于转换的区域性。</param>
    /// <returns>对应的 Color 对象，如果转换失败则返回 null。</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color;
        }

        return null;
    }
}
