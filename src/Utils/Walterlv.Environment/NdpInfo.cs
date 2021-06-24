using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Walterlv
{
    /// <summary>
    /// 包含 .NET Framework 开发平台（NET Developer Platform）的若干信息。
    /// 这与 CLR 运行时信息不同。通常，4.0/4.5/4.6/4.7/4.8 开发平台都对应 4.0 的 CLR 运行时。
    /// 关于各个系统自带的 .NET Framework 版本，请参阅：
    ///  - https://blog.walterlv.com/post/embeded-dotnet-version-in-all-windows.html
    /// </summary>
    public sealed class NdpInfo
    {
        /// <summary>
        /// 获取此 .NET Framework 的可与其他版本共存的主要版本。
        /// 形如：v3.5，v4。
        /// </summary>
        public string MainVersion { get; }

        /// <summary>
        /// 获取此 .NET Framework 的发行版本名称，如 Full，Client。
        /// </summary>
        public string ReleaseVersionName { get; }

        /// <summary>
        /// 获取此 .NET Framework 的版本号（通常是就地更新的最新版本）。
        /// 此版本号有可能是三部分，有可能是四部分。
        /// 如：3.5.30729.4926，4.7.02556。
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 获取此 .NET Framework 的发行号。
        /// 发行号随每次 .NET Framework 的发布而改变，其对应关系请参见：
        /// https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        /// </summary>
        public int Release { get; }

        /// <summary>
        /// 获取此 .NET Framework 的服务包版本。
        /// </summary>
        /// <returns>
        /// 如果为 0 说明是第一次发行，如果为 1、2 则说明是后续发行的服务包。
        /// 目前没有达到 3 的先例，但并不排除将来的情况。
        /// </returns>
        public int SP { get; }

        /// <summary>
        /// 获取 .NET Framework 发布时采用的版本号。如 4.6，4.7.1。
        /// </summary>
        public string DisplayVersion
        {
            get
            {
                // 获取正式发行版本的版本名称。
                if (ReleaseToNameDictionary.TryGetValue(Release, out var name))
                {
                    return name;
                }
                // 如果无法获取，说明使用的时预览版，则截取前两位。
                // 这可能导致较新的版本显示成较旧的名称。例如高于 4.7.1 的预览版会因此而显示成 4.7。
                var version = new Version(Version);
                return $"{version.Major}.{version.Minor}";
            }
        }

        /// <summary>
        /// 获取 .NET Framework 的显示发行名称。如 ""（完整版），"Client Profile"（精简子集）。
        /// </summary>
        public string DisplayReleaseVersionName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ReleaseVersionName) || ReleaseVersionName == "Full")
                {
                    // 完整版的发行版本名称不显示。
                    return "";
                }
                if (ReleaseVersionName == "Client")
                {
                    // 为客户端程序精简后的 .NET Framework 子集，显示精简子集的官方发行名称。
                    // https://docs.microsoft.com/en-us/dotnet/framework/deployment/client-profile
                    return "Client Profile";
                }
                // 截至 .NET Framework 4.7.1，不存在任何其他的发行版本名称。
                // 但不保证之后是否会存在。
                return ReleaseVersionName;
            }
        }

        /// <summary>
        /// 通过从注册表获取的原始信息创建 <see cref="NdpInfo"/> 的新实例。
        /// </summary>
        /// <param name="mainVersion">主要版本，在注册表中是 NDP 下的项名。</param>
        /// <param name="releaseVersionName">发行版本名称，在注册表中是版本项子项的项名。</param>
        /// <param name="version">当前安装的就地更新的最新版本号。</param>
        /// <param name="release">发行号。</param>
        /// <param name="sp">服务包号。</param>
        private NdpInfo(string mainVersion, string? releaseVersionName,
            string version, int release, int sp)
        {
            MainVersion = mainVersion;
            ReleaseVersionName = releaseVersionName ?? "";
            Version = version;
            Release = release;
            SP = sp;
        }

        /// <summary>
        /// 将此 <see cref="NdpInfo"/> 转换成 .NET Framework 的发行名称。
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder($".NET Framework {DisplayVersion}");
            if (SP > 0)
            {
                builder.Append($" SP{SP}");
            }
            var releaseVersionName = DisplayReleaseVersionName;
            if (!string.IsNullOrWhiteSpace(releaseVersionName))
            {
                builder.Append($" {releaseVersionName}");
            }
            return builder.ToString();
        }

        /// <summary>
        /// 获取 .NET Framework 4.5 及以上版本的发行号与版本名称的对应关系。
        /// 4.5 及以下版本没有这样的对应关系。
        /// 关于各个系统自带的 .NET Framework 版本，请参阅：
        ///  - https://blog.walterlv.com/post/embeded-dotnet-version-in-all-windows.html
        /// </summary>
        private static readonly Dictionary<int, string> ReleaseToNameDictionary = new Dictionary<int, string>
        {
            // .NET Framework 4.5
            { 378389, "4.5" },
            // .NET Framework 4.5.1（Windows 8.1 或 Windows Server 2012 R2 自带）
            { 378675, "4.5.1" },
            // .NET Framework 4.5.1（其他系统安装）
            { 378758, "4.5.1" },
            // .NET Framework 4.5.2
            { 379893, "4.5.2" },
            // .NET Framework 4.6（Windows 10 第一个版本 1507 自带）
            { 393295, "4.6" },
            // .NET Framework 4.6（其他系统安装）
            { 393297, "4.6" },
            // .NET Framework 4.6.1（Windows 10 十一月更新 1511 自带）
            { 394254, "4.6.1" },
            // .NET Framework 4.6.1（其他系统安装）
            { 394271, "4.6.1" },
            // .NET Framework 4.6.2（Windows 10 一周年更新 1607 和 Windows Server 2016 自带）
            { 394802, "4.6.2" },
            // .NET Framework 4.6.2（其他系统安装）
            { 394806, "4.6.2" },
            // .NET Framework 4.7（Windows 10 创造者更新 1703 自带）
            { 460798, "4.7" },
            // .NET Framework 4.7（其他系统安装）
            { 460805, "4.7" },
            // .NET Framework 4.7.1（Windows 10 秋季创造者更新 1709 和 Windows Server 1709 自带）
            { 461308, "4.7.1" },
            // .NET Framework 4.7.1（其他系统安装）
            { 461310, "4.7.1" },
            // .NET Framework 4.7.2（Windows 10 2018年四月更新 1803 和 Windows Server 1803 自带）
            { 461808, "4.7.2" },
            // .NET Framework 4.7.2（其他系统安装）
            { 461814, "4.7.2" },
            // .NET Framework 4.8（Windows 10 2019年五月更新 1903 自带）
            { 528040, "4.8" },
            // .NET Framework 4.8（其他系统安装）
            { 528049, "4.8" },
        };

        /// <summary>
        /// 获取 .NET Framework 4.x 就地更新的最新版本号。
        /// 例如：4.6，4.7.1。
        /// 如果没有安装任何版本的 .NET Framework，则返回 null。
        /// </summary>
        public static string? GetCurrentVersionName()
        {
            var dictionary = ReadFromRegistry();
            if (!dictionary.Any())
            {
                return null;
            }
            if (dictionary.TryGetValue("v4 Full", out var value))
            {
                return value.DisplayVersion;
            }
            if (dictionary.TryGetValue("v4", out value))
            {
                return value.DisplayVersion;
            }
            return dictionary.Last().Value.DisplayVersion;
        }

        /// <summary>
        /// 从注册表异步读取 .NET Framework 所有可共存的安装版本。
        /// 源码来自：https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        /// </summary>
        /// <returns>
        /// 一个字典，key 为 .NET Framework 的可共存版本，value 为此共存版本下就地更新（不可共存）的最新版本。
        /// <para>形如：</para>
        /// <para>v2.0.50727, 2.0.50727.4927 SP2 </para>
        /// <para>v3.0      , 3.0.30729.4926 SP2 </para>
        /// <para>v3.5      , 3.5.30729.4926 SP1 </para>
        /// <para>v4 Client , 4.7.03006          </para>
        /// <para>v4 Full   , 4.7.03006          </para>
        /// </returns>
        public static Task<Dictionary<string, NdpInfo>> ReadFromRegistryAsync()
        {
            return Task.Run(() => ReadFromRegistry());
        }

        /// <summary>
        /// 从注册表读取 .NET Framework 所有可共存的安装版本。
        /// 源码来自：https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        /// </summary>
        /// <returns>
        /// 一个字典，key 为 .NET Framework 的可共存版本，value 为此共存版本下就地更新（不可共存）的最新版本。
        /// <para>形如：</para>
        /// <para>v2.0.50727, 2.0.50727.4927 SP2 </para>
        /// <para>v3.0      , 3.0.30729.4926 SP2 </para>
        /// <para>v3.5      , 3.5.30729.4926 SP1 </para>
        /// <para>v4 Client , 4.7.03006          </para>
        /// <para>v4 Full   , 4.7.03006          </para>
        /// </returns>
        public static Dictionary<string, NdpInfo> ReadFromRegistry()
        {
            using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using var ndpKey = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\");
                return ReadCore(ndpKey);
            }

            static Dictionary<string, NdpInfo> ReadCore(RegistryKey? ndpKey)
            {
                // 保存读取到的所有 .NET Framework 可共存分支及其对应的就地更新版本号。
                var dictionary = new Dictionary<string, NdpInfo>();

                // 如果操作系统上根本没有此注册表值，只能说明程序本身有缺陷。
                if (ndpKey == null)
                {
                    return dictionary;
                }

                foreach (var versionKeyName in ndpKey.GetSubKeyNames())
                {
                    // v4.0 是微软创造的一个错误的项，需要排除。
                    // 参考：
                    // - [Engine finds .NET 4.0 Client Profile twice · Issue #1185](https://github.com/nunit/nunit/issues/1185)
                    // - [Remove duplicate available framework. Fixes #1185](https://github.com/nunit/nunit/commit/0c73732398cbdb69322629381ff3253387fa8166)
                    if (versionKeyName.StartsWith("v", StringComparison.InvariantCulture) && versionKeyName != "v4.0")
                    {
                        var versionKey = ndpKey.OpenSubKey(versionKeyName);
                        if (versionKey == null)
                        {
                            continue;
                        }

                        var name = (string?)versionKey.GetValue("Version", "") ?? "";
                        var sp = (int?)versionKey.GetValue("SP", 0) ?? 0;
                        var install = versionKey.GetValue("Install", "")?.ToString();
                        var release = (int?)versionKey.GetValue("Release", 0) ?? 0;

                        // 尝试获取只有一种发行类型的 .NET Framework。
                        if (string.IsNullOrEmpty(install))
                        {
                            // 现在拿不到，但是后面的代码一定可以拿得到。
                            // 出现这种情况是因为此 .NET Framework 版本有多种发行版本，如 Client，Full。
                        }
                        else
                        {
                            // 对于只有一种发行版本的 .NET Framework，此项下的键即为所需信息。
                            if (install == "1")
                            {
                                dictionary[versionKeyName] =
                                    new NdpInfo(versionKeyName, null, name, release, sp);
                            }
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        // 对于有多种发行版本（Client/Full）的 .NET Framework，此项子项下的键才是所需信息。
                        foreach (var subKeyName in versionKey.GetSubKeyNames())
                        {
                            var subKey = versionKey.OpenSubKey(subKeyName);
                            if (subKey == null)
                            {
                                continue;
                            }

                            name = (string?)subKey.GetValue("Version", "") ?? "";
                            if (!string.IsNullOrEmpty(name))
                            {
                                sp = (int?)subKey.GetValue("SP", 0) ?? 0;
                            }

                            install = subKey.GetValue("Install", "")?.ToString();
                            release = (int?)subKey.GetValue("Release", 0) ?? 0;
                            if (string.IsNullOrEmpty(install))
                            {
                                // 现在如果拿不到，也不用再拿了，因为目前还没有出现过这种情况，鬼知道将来微软会怎么玩。
                            }
                            else
                            {
                                if (install == "1")
                                {
                                    dictionary[versionKeyName + " " + subKeyName] =
                                        new NdpInfo(versionKeyName, subKeyName, name, release, sp);
                                }
                            }
                        }
                    }
                }

                return dictionary;
            }
        }
    }
}
