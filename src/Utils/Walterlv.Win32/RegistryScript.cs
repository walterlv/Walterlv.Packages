﻿using System;
using System.Runtime.CompilerServices;
using System.Security;
using Microsoft.Win32;

namespace Walterlv.Win32
{
    /// <summary>
    /// 包含类似脚本形式访问注册表键值的相关方法。
    /// </summary>
    public static class RegistryScript
    {
        /// <summary>
        /// 读取 32 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径会包含 Wow6432Node。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        public static string? Read32(this RegistryHive root, string path, string? key = null)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry32);
            return subKey.ReadCore(path, key);
        }
        /// <summary>
        /// 读取 32 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径会包含 Wow6432Node。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        public static T? Read32<T>(this RegistryHive root, string path, string? key = null) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry32);
            return subKey.ReadCore<T>(path, key);
        }

        /// <summary>
        /// 写入 32 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write32(this RegistryHive root, string path, string value)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry32);
            return subKey.WriteCore(path, null, value);
        }

        /// <summary>
        /// 写入 32 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write32<T>(this RegistryHive root, string path, T value) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry32);
            return subKey.WriteCore<T>(path, null, value);
        }

        /// <summary>
        /// 写入 32 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write32(this RegistryHive root, string path, string? key, string value)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry32);
            return subKey.WriteCore(path, key, value);
        }

        /// <summary>
        /// 写入 32 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write32<T>(this RegistryHive root, string path, string? key, T value) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry32);
            return subKey.WriteCore<T>(path, key, value);
        }

        /// <summary>
        /// 读取 64 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径也会是传入的路径（即会采用 32 位视图）。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        public static string? Read64(this RegistryHive root, string path, string? key = null)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry64);
            return subKey.ReadCore(path, key);
        }

        /// <summary>
        /// 读取 64 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径也会是传入的路径（即会采用 32 位视图）。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        public static T? Read64<T>(this RegistryHive root, string path, string? key = null) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry64);
            return subKey.ReadCore<T>(path, key);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径也会是传入的路径（即会采用 32 位视图）。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write64(this RegistryHive root, string path, string value)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry64);
            return subKey.WriteCore(path, key: null, value);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径也会是传入的路径（即会采用 32 位视图）。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write64<T>(this RegistryHive root, string path, T value) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry64);
            return subKey.WriteCore<T>(path, key: null, value);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径也会是传入的路径（即会采用 32 位视图）。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write64(this RegistryHive root, string path, string? key, string value)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry64);
            return subKey.WriteCore(path, key, value);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位系统上，路径会是传入的路径；
        /// 64 位系统上，路径也会是传入的路径（即会采用 32 位视图）。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write64<T>(this RegistryHive root, string path, string? key, T value) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Registry64);
            return subKey.WriteCore<T>(path, key, value);
        }

        /// <summary>
        /// 读取默认视图下的指定路径注册表项的值。
        /// 32 位进程在 32 位系统上，64 位进程在 64 位系统上，路径会是传入的路径；
        /// 32 位进程在 64 位系统上，路径会包含 Wow6432Node。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        public static string? Read(this RegistryHive root, string path, string? key = null)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            return subKey.ReadCore(path, key);
        }

        /// <summary>
        /// 读取默认视图下的指定路径注册表项的值。
        /// 32 位进程在 32 位系统上，64 位进程在 64 位系统上，路径会是传入的路径；
        /// 32 位进程在 64 位系统上，路径会包含 Wow6432Node。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        public static T? Read<T>(this RegistryHive root, string path, string? key = null) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            return subKey.ReadCore<T>(path, key);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位进程在 32 位系统上，64 位进程在 64 位系统上，路径会是传入的路径；
        /// 32 位进程在 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write(this RegistryHive root, string path, string value)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            return subKey.WriteCore(path, key: null, value);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位进程在 32 位系统上，64 位进程在 64 位系统上，路径会是传入的路径；
        /// 32 位进程在 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="value">注册表默认 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write<T>(this RegistryHive root, string path, T value) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            return subKey.WriteCore<T>(path, key: null, value);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位进程在 32 位系统上，64 位进程在 64 位系统上，路径会是传入的路径；
        /// 32 位进程在 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表对应 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write(this RegistryHive root, string path, string? key, string value)
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            return subKey.WriteCore(path, key, value);
        }

        /// <summary>
        /// 写入 64 位视图下的指定路径注册表项的值。
        /// 32 位进程在 32 位系统上，64 位进程在 64 位系统上，路径会是传入的路径；
        /// 32 位进程在 64 位系统上，路径会包含 Wow6432Node。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="root">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表对应 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        public static bool Write<T>(this RegistryHive root, string path, string? key, T value) where T : struct
        {
            using var subKey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            return subKey.WriteCore<T>(path, key, value);
        }

        /// <summary>
        /// 读取指定路径注册表项的值。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <param name="rootKey">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? ReadCore(this RegistryKey rootKey, string path, string? key = null)
        {
            using var subKey = rootKey.OpenSubKey(path, writable: false);
            return (string?)subKey?.GetValue(key, defaultValue: null);
        }

        /// <summary>
        /// 读取指定路径注册表项的值。
        /// 如果读取失败，则返回 null。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="rootKey">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <returns>注册表对应 Key 下读取到的值。如果不存在此注册表项，则返回 null。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T? ReadCore<T>(this RegistryKey rootKey, string path, string? key = null) where T : struct
        {
            var type = typeof(T);
            if (type != typeof(int) && type != typeof(long))
            {
                throw new NotSupportedException(@$"泛型参数 {nameof(T)} 仅支持 Int32(DWORD) 类型和 Int64(QWORD)。");
            }

            using var subKey = rootKey.OpenSubKey(path, writable: false);
            var value = subKey?.GetValue(key, defaultValue: null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            return value switch
            {
                int when type == typeof(int) => (T)value,
                int when type == typeof(long) => (T)value,
                long when type == typeof(int) => (T)value,
                long when type == typeof(long) => (T)value,
                _ => null,
            };
        }

        /// <summary>
        /// 写入指定路径注册表项的值。
        /// </summary>
        /// <param name="rootKey">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表对应 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool WriteCore(this RegistryKey rootKey, string path, string? key, string value)
        {
            if (rootKey is null)
            {
                throw new ArgumentNullException(nameof(rootKey));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            RegistryKey? subKey;
            try
            {
                subKey = rootKey.OpenSubKey(path, writable: true)
                    ?? rootKey.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            catch (SecurityException)
            {
                return false;
            }

            if (subKey is null)
            {
                return false;
            }

            try
            {
                subKey.SetValue(key, value);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 写入指定路径注册表项的值。
        /// </summary>
        /// <typeparam name="T">仅支持 <see cref="int"/> 和 <see cref="long"/>。</typeparam>
        /// <param name="rootKey">注册表根项。</param>
        /// <param name="path">根项之后的注册表路径。</param>
        /// <param name="key">注册表值的 Key。如果传入 null，则表示项下的默认 Key。</param>
        /// <param name="value">注册表对应 Key 的值。</param>
        /// <returns>如果成功写入值，则返回 true；如果因为 UAC 权限设置或者其他权限原因导致无法写入，则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool WriteCore<T>(this RegistryKey rootKey, string path, string? key, T value) where T : struct
        {
            if (rootKey is null)
            {
                throw new ArgumentNullException(nameof(rootKey));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            RegistryKey? subKey;
            try
            {
                subKey = rootKey.OpenSubKey(path, writable: true)
                    ?? rootKey.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            catch (SecurityException)
            {
                return false;
            }

            if (subKey is null)
            {
                return false;
            }

            try
            {
                subKey.SetValue(key, value);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
        }
    }
}
