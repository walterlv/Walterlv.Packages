using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.IO
{
    public class TextFileLogger : AsyncOutputLogger
    {
        private readonly string _lineEnd;
        private readonly FileInfo _infoLogFile;
        private readonly FileInfo _errorLogFile;
        private readonly bool _append;
        private readonly string? _extraInfoFormatIfNotFormatted;
        private StreamWriter? _infoWriter;
        private StreamWriter? _errorWriter;

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// </summary>
        /// <param name="logFile">日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        /// <param name="extraInfoFormatIfNotFormatted">如果日志包含额外信息，并且尚未指定格式化字符串，则使用此字符串格式化此额外信息。</param>
        public TextFileLogger(FileInfo logFile, bool append = false, string lineEnd = "\n", string? extraInfoFormatIfNotFormatted = null)
        {
            if (logFile is null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            _infoLogFile = logFile;
            _errorLogFile = logFile;
            _lineEnd = VerifyLineEnd(lineEnd);
            _append = append;
            _extraInfoFormatIfNotFormatted = extraInfoFormatIfNotFormatted;
        }

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// 在记录的时候，信息/警告和错误是分开成两个文件的。其中信息和警告在同一个文件，警告高亮；错误在另一个文件。
        /// </summary>
        /// <param name="infoLogFile">信息和警告的日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="errorLogFile">错误日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        /// <param name="extraInfoFormatIfNotFormatted">如果日志包含额外信息，并且尚未指定格式化字符串，则使用此字符串格式化此额外信息。</param>
        public TextFileLogger(FileInfo infoLogFile, FileInfo errorLogFile, bool append = false, string lineEnd = "\n", string? extraInfoFormatIfNotFormatted = null)
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
            _infoLogFile = infoLogFile;
            _errorLogFile = errorLogFile;
            _append = append;
            _extraInfoFormatIfNotFormatted = extraInfoFormatIfNotFormatted;
        }

        protected override async Task OnInitializedAsync()
        {
            _infoWriter = await CreateWriterAsync(_infoLogFile).ConfigureAwait(false);
            _errorWriter = _errorLogFile == _infoLogFile
                ? _infoWriter
                : await CreateWriterAsync(_errorLogFile).ConfigureAwait(false);
        }

        protected override void OnLogReceived(in Context context)
        {
            if (context.ExtraInfo != null && context.CurrentLevel > LogLevel.Error)
            {
                _infoWriter?.WriteLine(context.BuildLogText(containsExtraInfo: false));
                _errorWriter?.WriteLine(context.BuildLogText(extraInfoFormatIfNotFormatted: "```csharp\n{0}\n```"));
            }
            else
            {
                _infoWriter?.WriteLine(context.BuildLogText());
            }
        }

        private async Task<StreamWriter?> CreateWriterAsync(FileInfo file)
        {
            var directory = file.Directory;
            if (directory != null && !Directory.Exists(directory.FullName))
            {
                directory.Create();
            }

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    return new StreamWriter(file.FullName, _append, Encoding.UTF8)
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
    }
}
