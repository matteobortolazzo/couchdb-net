using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CouchDB.Driver.ChangesFeed
{
    internal static class StreamReaderExtensions
    {
        public static async IAsyncEnumerable<string> ReadLinesAsync(this Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent();
            var decoder = new LinesDecoder(owner);

            while (!cancellationToken.IsCancellationRequested)
            {
                var readCharCount = await stream
                    .ReadAsync(owner.Memory, cancellationToken)
                    .ConfigureAwait(false);

                if (readCharCount == 0)
                {
                    yield break;
                }

                foreach (var line in decoder.ReadLines(readCharCount))
                {
                    yield return line;
                }
            }
        }

        private class LinesDecoder
        {
            private readonly Decoder _uniDecoder;
            private readonly IMemoryOwner<byte> _owner;
            private string _remainder = string.Empty;

            public LinesDecoder(IMemoryOwner<byte> owner)
            {
                _uniDecoder = Encoding.UTF8.GetDecoder();
                _owner = owner;
            }

            public IEnumerable<string> ReadLines(int readCharCount)
            {
                Span<byte> readableSpan = _owner.Memory.Span[..readCharCount];
                var charCount = _uniDecoder.GetCharCount(readableSpan, false);
                var chars = new Span<char>(new char[charCount]);
                _ = _uniDecoder.GetChars(readableSpan, chars, false);

                var str = new string(chars);
                var lines = str.Split('\n');

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    if (_remainder.Length > 0)
                    {
                        line = _remainder + line;
                        _remainder = string.Empty;
                    }

                    if (i == lines.Length - 1)
                    {
                        _remainder = line;
                        continue;
                    }

                    yield return line;
                }
            }
        }
    }
}
