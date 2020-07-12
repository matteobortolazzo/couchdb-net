using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CouchDB.Driver.ChangesFeed
{
    internal static class StreamExtensions
    {
        public static async IAsyncEnumerable<string> ReadLinesAsync(this Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent();
            using var decoder = new LinesDecoder(owner.Memory);

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

        private class LinesDecoder: IDisposable
        {
            private readonly Decoder _uniDecoder;
            private readonly Memory<byte> _streamMemory;
            private string _remainder = string.Empty;
            private readonly IMemoryOwner<char> _decodingMemory;

            public LinesDecoder(Memory<byte> streamMemory)
            {
                _uniDecoder = Encoding.UTF8.GetDecoder();
                _streamMemory = streamMemory;
                _decodingMemory = MemoryPool<char>.Shared.Rent(streamMemory.Length);
            }

            public IEnumerable<string> ReadLines(int readCharCount)
            {
                Span<byte> readableStreamSpan = _streamMemory.Span[..readCharCount];
                var charCount = _uniDecoder.GetCharCount(readableStreamSpan, false);
                _ = _uniDecoder.GetChars(readableStreamSpan, _decodingMemory.Memory.Span, false);

                Span<char> decodedSpan = _decodingMemory.Memory.Span[..charCount];
                var str = new string(decodedSpan);
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

            public void Dispose()
            {
                _decodingMemory.Dispose();
            }
        }
    }
}
