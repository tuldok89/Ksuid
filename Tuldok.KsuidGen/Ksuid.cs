using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tuldok.Base62;

namespace Tuldok.KsuidGen
{
    public class Ksuid
    {
        static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        const int Epoch = 1400000000;
        const int TimestampLength = 4;
        const int PayloadLength = 16;
        const int MaxEncodedLength = 27;

        /// <summary>
        /// Generate a new KSUID
        /// </summary>
        /// <returns>New KSUID value.</returns>
        public async Task<string> Generate()
        {
            var uid = new KsuidGenerator(random);
            return await uid.NextId();
        }

        /// <summary>
        /// Parse a Ksuid and display its component parts
        /// </summary>
        /// <param name="ksuid">The Ksuid to parse.</param>
        /// <returns>TThe component parts of the Ksuid.</returns>
        public (DateTimeOffset Utc, long Timestamp, string Payload) Parse(string ksuid)
        {
            var parser = new KsuidParser();
            return parser.Parse(ksuid);
        }

        private class KsuidGenerator
        {
            RNGCryptoServiceProvider random;

            public KsuidGenerator(RNGCryptoServiceProvider random)
            {
                this.random = random;
            }

            private byte[] MakeTimeStamp()
            {
                var utc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timestamp = (int)(utc - Epoch);
                return BitConverter.GetBytes(timestamp);
            }

            private byte[] MakePayload()
            {
                var bytes = new byte[PayloadLength];
                random.GetBytes(bytes);

                return bytes;
            }

            public async Task<string> NextId()
            {
                var timestamp = MakeTimeStamp();
                var payload = MakePayload();
                var encoder = new Base62Converter();
                var memStream = new MemoryStream();

                await memStream.WriteAsync(timestamp, 0, TimestampLength);
                await memStream.WriteAsync(payload, 0, PayloadLength);

                var uid = Base62Converter.Encode(memStream.ToArray());

                if (uid.Length > MaxEncodedLength)
                {
                    return uid.Substring(0, MaxEncodedLength);
                }

                return uid;
            }
        }

        private class KsuidParser
        {
            private long DecodeTimestamp(byte[] decodedKsuid)
            {
                var timestamp = new byte[TimestampLength];
                Array.Copy(decodedKsuid, 0, timestamp, 0, TimestampLength);

                return BitConverter.ToInt32(timestamp, 0) + Epoch;
            }

            private string DecodePayload(byte[] decodedKsuid)
            {
                var payload = new byte[PayloadLength];
                Array.Copy(decodedKsuid, TimestampLength, payload, 0, decodedKsuid.Length - TimestampLength);

                return Encoding.ASCII.GetString(payload);
            }

            public (DateTimeOffset, long, string) Parse(string ksuid)
            {
                var converter = new Base62Converter();
                var bytes = Base62Converter.Decode(ksuid.ToCharArray());
                var timestamp = DecodeTimestamp(bytes);
                var payload = DecodePayload(bytes);
                var utc = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                return (utc, timestamp, payload);
            }
        }
    }
}
