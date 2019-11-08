using System;
using System.Collections.Generic;

namespace Walterlv.IO.PackageManagement
{
    public class IOResult : ILogger
    {
        private readonly List<string> _logs = new List<string>();
        private bool _isSuccess = true;

        public void Log(string message)
        {
            _logs.Add(message);
        }

        public void Fail(Exception ex)
        {
            _isSuccess = false;
            _logs.Add(ex.ToString());
        }

        public void Append(IOResult otherResult)
        {
            if (!otherResult._isSuccess)
            {
                _isSuccess = false;
            }
            _logs.AddRange(otherResult._logs);
        }
    }

    public interface ILogger
    {
        void Fail(Exception ex);
        void Log(string message);
    }
}
