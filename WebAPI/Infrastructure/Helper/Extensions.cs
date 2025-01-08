using System;

namespace WebAPI.Infrastructure.Helper
{
    public static class Extensions
    {
        public static string ToDataImageUrl(this byte[] source)
        {
            return string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(source));
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool IsValidEmail(this string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public static TKey GetRandomKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Random random)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                throw new ArgumentException("The dictionary is either null or empty.");
            }

            // Get a list of keys
            List<TKey> keys = new List<TKey>(dictionary.Keys);

            // Select a random index
            int randomIndex = random.Next(keys.Count);

            // Return the key at the random index
            return keys[randomIndex];
        }
    }
}
