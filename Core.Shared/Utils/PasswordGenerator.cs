using System.Text;

namespace Core.Shared.Utils
{
    public static class PasswordGenerator
    {
        static readonly string AlphaNumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateRandomPassword(int size = 6)
        {
            var rnd = new Random();
            var ret = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                ret.Append(AlphaNumerics.AsSpan(rnd.Next(AlphaNumerics.Length), 1));
            }

            return $"_p#{ret}";
        }
    }
}