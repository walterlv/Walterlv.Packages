using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walterlv.IO.PackageManagement
{
    /// <summary>
    /// 表示复制或移动文件夹，如果目标文件夹存在时应该采取的覆盖策略。
    /// </summary>
    public enum DirectoryOverwriteStrategy
    {
        /// <summary>
        /// 禁止覆盖。如果目标路径存在文件夹，则抛出异常。
        /// </summary>
        DoNotOverwrite,

        /// <summary>
        /// 覆盖。如果目标路径存在文件夹，则会删除目标文件夹中的全部内容。
        /// </summary>
        Overwrite,

        /// <summary>
        /// 合并式覆盖。如果目标路径存在文件夹，则会按文件覆盖掉目标文件夹中的全部文件。即目标文件夹中只有相同相对路径的文件会被覆盖，其他文件依然存在。
        /// </summary>
        MergeOverwrite,

        /// <summary>
        /// 合并式跳过。如果目标路径存在文件夹，则会跳过目标文件夹中所存在的文件，只复制不存在的文件。
        /// </summary>
        MergeSkip,
    }
}
