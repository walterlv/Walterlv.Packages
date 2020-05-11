using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Walterlv.IO.PackageManagement
{
    /// <summary>
    /// 包含此 IO 操作中的日志信息。也可以
    /// </summary>
    [DebuggerDisplay(nameof(DebuggerDisplay))]
    public class IOResult
    {
        private readonly List<string> _logs = new List<string>();
        private bool _isSuccess = true;

        internal void Log(string message)
        {
            _logs.Add(message);
        }

        internal void Fail(Exception ex)
        {
            _isSuccess = false;
            _logs.Add(ex.ToString());
        }

        internal void Append(IOResult otherResult)
        {
            if (!otherResult._isSuccess)
            {
                _isSuccess = false;
            }
            _logs.AddRange(otherResult._logs);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => string.Join(Environment.NewLine, _logs);

        /// <summary>
        /// 表示成功与否的隐式转换。
        /// </summary>
        /// <param name="result">要转换为表示成功与否的布尔值。</param>
        public static implicit operator bool(IOResult result)
        {
            return result._isSuccess;
        }
    }
}
