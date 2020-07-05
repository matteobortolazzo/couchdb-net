using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CouchDB.Driver.Extensions
{
    internal static class StreamReaderExtensions
    {
        public static async IAsyncEnumerable<string> ReadLinesAsync(this Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent();
            var prevRemainder = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                var charRead = await stream
                    .ReadAsync(owner.Memory, cancellationToken)
                    .ConfigureAwait(false);

                if (charRead == 0)
                {
                    continue;
                }

                var str = Encoding.UTF8.GetString(owner.Memory.Span[..charRead]);
                var lines = str.Split('\n');

                // If last line is empty
                var isMessageComplete = lines[^1].Length == 0;
                var currentRemainder = isMessageComplete
                    ? string.Empty
                    : lines[^1];

                IEnumerable<string> filteredList = lines
                    .Where(line => line.Length > 0 && line != currentRemainder);
                
                var i = 0;
                foreach (var line in filteredList)
                {
                    yield return i++ == 0
                        ? prevRemainder + line
                        : line;
                }

                prevRemainder = currentRemainder;
            }
        }
    }
}
