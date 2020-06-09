using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.IO
{
    /// <summary>
    /// 提供记录到文本文件的日志。
    /// </summary>
    public class TextFileLogger : AsyncOutputLogger, IDisposable
    {
        private readonly string _lineEnd;
        private readonly FileInfo _infoLogFile;
        private readonly FileInfo _errorLogFile;
        private readonly bool _shouldAppendInfo;
        private readonly bool _shouldAppendError;
        private StreamWriter? _infoWriter;
        private StreamWriter? _errorWriter;

        /// <summary>
        /// 创建文本文件记录日志的 <see cref="TextFileLogger"/> 的新实例。
        /// </summary>
        /// <param name="logFile">日志文件。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public TextFileLogger(FileInfo logFile, bool append = false, string lineEnd = "\n")
        {
            if (logFile is null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            _infoLogFile = logFile;
            _errorLogFile = logFile;
            _lineEnd = VerifyLineEnd(lineEnd);
            _shouldAppendInfo = append;
            _shouldAppendError = append;
        }

        /// <summary>
        /// 创建文本文件记录日志的 <see cref="TextFileLogger"/> 的新实例。
        /// 在记录的时候，信息/警告和错误是分开成两个文件的。信息文件包含用于诊断的所有信息，而错误文件包含代码中无法预知的错误记录。
        /// </summary>
        /// <param name="infoLogFile">信息和警告的日志文件。</param>
        /// <param name="errorLogFile">错误日志文件。</param>
        /// <param name="appendInfo">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="appendError">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public TextFileLogger(FileInfo infoLogFile, FileInfo errorLogFile,
            bool appendInfo = false, bool appendError = true, string lineEnd = "\n")
        {
            if (infoLogFile is null)
            {
                throw new ArgumentNullException(nameof(infoLogFile));
            }

            if (errorLogFile is null)
            {
                throw new ArgumentNullException(nameof(errorLogFile));
            }

            _lineEnd = VerifyLineEnd(lineEnd);
            var areSameFile = string.Equals(infoLogFile.FullName, errorLogFile.FullName, StringComparison.OrdinalIgnoreCase);
            _infoLogFile = infoLogFile;
            _errorLogFile = areSameFile ? infoLogFile : errorLogFile;
            _shouldAppendInfo = appendInfo;
            _shouldAppendError = appendError;
        }

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            if (_isDisposed)
            {
                return;
            }

            _infoWriter = await CreateWriterAsync(_infoLogFile, _shouldAppendInfo).ConfigureAwait(false);
            _errorWriter = _errorLogFile == _infoLogFile
                ? _infoWriter
                : await CreateWriterAsync(_errorLogFile, _shouldAppendError).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected sealed override void OnLogReceived(in Context context)
        {
            if (_isDisposed)
            {
                return;
            }

            var areSameFile = _infoWriter == _errorWriter;
            if (!areSameFile && context.CurrentLevel > LogLevel.Error)
            {
                // 写入日志的主要部分。
                _infoWriter?.WriteLine(BuildLogText(in context, containsExtraInfo: false, _lineEnd));

                // 写入日志的扩展部分。
                _errorWriter?.WriteLine(BuildLogText(in context, context.ExtraInfo != null, _lineEnd));
            }
            else
            {
                _infoWriter?.WriteLine(BuildLogText(in context, context.ExtraInfo != null, _lineEnd));
            }
        }

        /// <summary>
        /// 派生类重写此方法时，将单条日志格式化成一端可被记录到文件的字符串。
        /// </summary>
        /// <param name="context">单条日志信息。</param>
        /// <param name="containsExtraInfo">此次格式化时，是否应该包含额外的日志信息。</param>
        /// <param name="lineEnd">记录到文件时应该使用的行尾符号。</param>
        /// <returns>格式化后的日志文本。</returns>
        protected virtual string BuildLogText(in Context context, bool containsExtraInfo, string lineEnd)
        {
            var time = context.Time.ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var member = context.CallerMemberName;
            var text = context.Text;
            string? extraInfo = null;
            if (containsExtraInfo && context.ExtraInfo != null)
            {
                extraInfo = context.ExtraInfo;
            }
            return extraInfo is null
                ? $@"[{time}][{member}] {text}"
                : $@"[{time}][{member}] {text}{lineEnd}{extraInfo}";
        }

        /// <summary>
        /// 创建写入到日志的流。
        /// </summary>
        /// <param name="file">日志文件。</param>
        /// <param name="append">是追加到文件还是直接覆盖文件。</param>
        /// <returns>可等待的实例。</returns>
        private async Task<StreamWriter?> CreateWriterAsync(FileInfo file, bool append)
        {
            var directory = file.Directory;
            if (directory != null && !Directory.Exists(directory.FullName))
            {
                directory.Create();
            }

            for (var i = 0; i < 10; i++)
            {
                if (_isDisposed)
                {
                    return null;
                }

                try
                {
                    var fileStream = File.Open(
                        file.FullName,
                        append ? FileMode.Append : FileMode.Create,
                        FileAccess.Write,
                        FileShare.Read);
                    return new StreamWriter(fileStream, Encoding.UTF8)
                    {
                        AutoFlush = true,
                        NewLine = _lineEnd,
                    };
                }
                catch (IOException)
                {
                    // 当出现了 IO 错误，通常还有恢复的可能，所以重试。
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }
                catch (Exception)
                {
                    // 当出现了其他错误，恢复的可能性比较低，所以重试更少次数，更长时间。
                    await Task.Delay(5000).ConfigureAwait(false);
                    i++;
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查字符串是否是行尾符号，如果不是则抛出异常。
        /// </summary>
        /// <param name="lineEnd">行尾符号。</param>
        /// <returns>行尾符号。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string VerifyLineEnd(string lineEnd) => lineEnd switch
        {
            null => throw new ArgumentNullException(nameof(lineEnd)),
            "\n" => "\n",
            "\r" => "\r",
            "\r\n" => "\r\n",
            _ => throw new ArgumentException("虽然你可以指定行尾符号，但也只能是 \\n、\\r 或者 \\r\\n。", nameof(lineEnd))
        };

        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    (_infoWriter?.BaseStream as FileStream)?.Dispose();
                    (_errorWriter?.BaseStream as FileStream)?.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose() => Dispose(true);

        public void Close() => Dispose(true);
    }
}
