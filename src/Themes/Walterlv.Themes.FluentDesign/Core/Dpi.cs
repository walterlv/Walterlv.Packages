using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Walterlv.Windows.Core
{
    /// <summary>
    /// 表示一对 DPI 值。
    /// The values of *dpiX and *dpiY are identical. You only need to record one of the values to determine the DPI and respond appropriately.
    /// </summary>
    [DebuggerDisplay("X = {X} ({FactorX}), Y = {Y} ({FactorY})")]
    public struct Dpi
    {
        private static readonly Lazy<Dpi> SystemDpiLazy = new Lazy<Dpi>(() =>
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX",
                BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi",
                BindingFlags.NonPublic | BindingFlags.Static);
            var x = (int?)dpiXProperty?.GetValue(null, null) ?? 96;
            var y = (int?)dpiYProperty?.GetValue(null, null) ?? 96;
            if (x != 0 && y != 0)
            {
                return new Dpi(x, y);
            }
            if (y != 0)
            {
                return new Dpi(y, y);
            }
            if (x != 0)
            {
                return new Dpi(x, x);
            }
            return ScreenStandard;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// 获取显示器的标准 DPI 值。在此 DPI 下，一个渲染像素的尺寸与物理像素的尺寸是一致的。
        /// </summary>
        public static readonly Dpi ScreenStandard = new Dpi(96, 96);

        /// <summary>
        /// 获取系统设置的的 DPI 值。
        /// </summary>
        public static Dpi System => SystemDpiLazy.Value;

        /// <summary>
        /// 获取水平方向的 DPI 值。
        /// </summary>
        public int X { get; }

        /// <summary>
        /// 获取垂直方向的 DPI 值。
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// 获取水平方向的 DPI 因数。如 144 DPI 值的因数等于 150%。
        /// </summary>
        public double FactorX => X / (double)ScreenStandard.X;

        /// <summary>
        /// 获取垂直方向的 DPI 因数。如 144 DPI 值的因数等于 150%。
        /// </summary>
        public double FactorY => Y / (double)ScreenStandard.Y;

        /// <summary>
        /// 使用指定的水平和垂直 DPI 值创建 DPI 对象。
        /// </summary>
        /// <param name="x">水平方向的 DPI 值。</param>
        /// <param name="y">垂直方向的 DPI 值。</param>
        public Dpi(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        /// <param name="dpi1"></param>
        /// <param name="dpi2"></param>
        /// <returns></returns>
        public static bool operator ==(Dpi dpi1, Dpi dpi2)
        {
            return dpi1.X == dpi2.X && dpi1.Y == dpi2.Y;
        }

        /// <summary>
        /// 判断不相等
        /// </summary>
        /// <param name="dpi1"></param>
        /// <param name="dpi2"></param>
        /// <returns></returns>
        public static bool operator !=(Dpi dpi1, Dpi dpi2)
        {
            return !(dpi1 == dpi2);
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Dpi other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is not null && obj is Dpi dpi && Equals(dpi);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{X}({FactorX:P0}),{Y}({FactorY:P0})";
        }
    }
}
