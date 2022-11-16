using System.Security;

namespace UIQ
{
    public static class StringExtensions
    {
        public static string ToMD5(this string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = System.Text.Encoding.UTF8.GetBytes(str);

                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);

                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                  .Replace("-", String.Empty)
                  .ToUpper();

                return md5;
            }
        }

        public static SecureString GetGetSecureString(this string str)
        {
            var secureString = new SecureString();
            (str ?? string.Empty).ToList().ForEach(x => secureString.AppendChar(x));
            return secureString;
        }

        public static string GetFilterPathTraversal(this string str)
        {
            if (str == null) return "";

            return str.Replace("..", "").Replace("/", "").Replace("\\", "").Replace("'", "");
        }
    }
}