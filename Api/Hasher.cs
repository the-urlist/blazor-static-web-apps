using System;
using System.Security.Cryptography;

namespace Api
{
    public class Hasher
    {
        protected const string HASHER_KEY = "HASHER_KEY";
        protected const string HASHER_SALT = "HASHER_SALT";

        private readonly string _key;
        private readonly string _salt;

        public Hasher()
            : this(Environment.GetEnvironmentVariable(HASHER_KEY),
                    Environment.GetEnvironmentVariable(HASHER_SALT))
        { }

        public Hasher(string key, string salt)
        {
            _salt = salt;
            _key = key;
        }

        public virtual string HashString(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentException("Data parameter was null or empty", "data");

            byte[] keyByte = System.Text.Encoding.UTF8.GetBytes(_key);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(data);
            using var hmacsha256 = new HMACSHA384(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);

            return Convert.ToBase64String(hashmessage);
        }

        public virtual bool Verify(string data, string hashedData)
        {
            return hashedData == HashString(data);
        }
    }
}