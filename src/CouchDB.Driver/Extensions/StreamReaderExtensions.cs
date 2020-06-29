using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CouchDB.Driver.Extensions
{
    internal static class StreamReaderExtensions
    {
        public static async Task<string> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
        {
            if (reader.EndOfStream)
            {
                return string.Empty;
            }    

            var result = new StringBuilder();
            var lastChar = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false);

            while (!reader.EndOfStream && lastChar != '\n')
            {
                try
                {
                    var newChar = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false);
                    if (lastChar == '\r' && newChar == '\n')
                    {
                        return result.ToString();
                    }

                    result.Append(lastChar);
                    lastChar = newChar;
                }
                catch (EndOfStreamException)
                {
                    result.Append(lastChar);
                    return result.ToString();
                }
            }

            result.Append(lastChar);
            return result.ToString();
        }

        public static async Task<char> ReadCharAsync(this StreamReader reader, CancellationToken cancellationToken)
        {
            var buffer = new char[1];
            await reader.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            return buffer[0];
        }
    }
}
