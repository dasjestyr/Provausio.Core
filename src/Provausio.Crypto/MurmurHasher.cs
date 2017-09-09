using System;
using System.Text;

namespace Provausio.Crypto
{
    /// <summary>
    /// Static wrapper for the murmur hashing library.
    /// </summary>
    public static class MurmurHasher
    {
        public static Guid GetGuid(string data, object seed)
        {
            return new Guid(GetHash(data, seed));
        }

        public static byte[] GetHash(string data, object seed)
        {
            var seedHash = seed.GetHashCode();
            var seedValue = BitConverter.ToUInt32(BitConverter.GetBytes(seedHash), 0);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var alg = Murmur.MurmurHash.Create128(seedValue);
            var hashBytes = alg.ComputeHash(dataBytes);

            return hashBytes;
        }

        public static Guid GetGuid(string data)
        {
            return new Guid(GetHash(data));
        }

        public static byte[] GetHash(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var alg = Murmur.MurmurHash.Create128();
            var hashBytes = alg.ComputeHash(dataBytes);

            return hashBytes;

        }
    }
}
