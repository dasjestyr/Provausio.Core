using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Provausio.Crypto
{
    /// <summary>
    /// Helper class for predefining an objects elements for generating a hash.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashedKey<T>
    {
        private readonly object _seed;
        private readonly Expression<Func<T, string>>[] _properties;

        public HashedKey(params Expression<Func<T, string>>[] properties)
        {
            _properties = properties;
        }

        public HashedKey(object seed, params Expression<Func<T, string>>[] properties)
            : this(properties)
        {
            _seed = seed;
        }

        public string GetBase64(T source, bool urlEncode = true)
        {
            var data = GetBytes(source);
            var base64 = Convert.ToBase64String(data);
            return urlEncode
                ? Base64UrlEncoder.Encode(base64)
                : base64;
        }

        public byte[] GetBytes(T source)
        {
            var data = GetHashValue(source);

            var seedHash = _seed?.GetHashCode();
            var seedValue = seedHash.HasValue
                ? BitConverter.ToUInt32(BitConverter.GetBytes(seedHash.Value), 0)
                : 0U;

            return MurmurHasher.GetHash(data, seedValue);
        }

        public Guid GetId(T source)
        {
            var bytes = GetBytes(source);
            return new Guid(bytes);
        }

        public bool AreEqual(T left, T right)
        {
            return GetId(left) == GetId(right);
        }

        private string GetHashValue(T source)
        {
            var values = _properties
                .Select(property => property.Compile()(source))
                .ToList();

            return string.Join("|", values);
        }
    }
}
