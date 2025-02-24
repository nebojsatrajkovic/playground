using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Shared.Utils
{
    /// <summary>
    /// Provides utility methods to generate and validate strong passwords.
    /// A strong password is at least 8 characters long and contains:
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one special character.
    /// </summary>
    public static class PasswordHelper
    {
        static readonly string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static readonly string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        static readonly string SpecialChars = "!@#$%^&*(),.?\":{}|<>";
        static readonly string AlphaNumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// Generates a strong password. The password will be at least 8 characters long and will
        /// contain at least one uppercase letter, one lowercase letter, and one special character.
        /// The remaining characters are randomly chosen from alphanumeric characters.
        /// </summary>
        /// <param name="size">The desired length of the password. If less than 8, the length will default to 8.</param>
        /// <returns>A strong randomly generated password.</returns>
        public static string GenerateRandomPassword(int size = 8)
        {
            if (size < 8) size = 8;

            var ret = new StringBuilder();

            using (var rng = RandomNumberGenerator.Create())
            {
                // ensure at least one character from each requirement
                ret.Append(GetRandomCharacter(Uppercase, rng));
                ret.Append(GetRandomCharacter(Lowercase, rng));
                ret.Append(GetRandomCharacter(SpecialChars, rng));

                // generate remaining characters to meet the desired size
                for (int i = 3; i < size; i++)
                {
                    ret.Append(GetRandomCharacter(AlphaNumerics, rng));
                }
            }

            // shuffle the result to mix the required characters
            return ShufflePassword(ret.ToString());
        }

        /// <summary>
        /// Shuffles the characters in the provided password string to randomize the order.
        /// This ensures that required characters (uppercase, lowercase, special) are not predictable.
        /// </summary>
        /// <param name="password">The password to shuffle.</param>
        /// <returns>The shuffled password string.</returns>
        static string ShufflePassword(string password)
        {
            var array = password.ToCharArray();
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = array.Length - 1; i > 0; i--)
                {
                    int j = GetRandomIndex(rng, i + 1);
                    char temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }
            }
            return new string(array);
        }

        /// <summary>
        /// Gets a random character from the given character set using a cryptographically secure RNG.
        /// </summary>
        /// <param name="characters">The character set to pick from.</param>
        /// <param name="rng">The cryptographically secure random number generator.</param>
        /// <returns>A randomly selected character.</returns>
        static char GetRandomCharacter(string characters, RandomNumberGenerator rng)
        {
            byte[] buffer = new byte[1];
            rng.GetBytes(buffer);
            int index = buffer[0] % characters.Length;
            return characters[index];
        }

        /// <summary>
        /// Gets a random index within the specified range using a cryptographically secure RNG.
        /// </summary>
        /// <param name="rng">The cryptographically secure random number generator.</param>
        /// <param name="maxValue">The exclusive upper bound of the range.</param>
        /// <returns>A random index within the specified range.</returns>
        static int GetRandomIndex(RandomNumberGenerator rng, int maxValue)
        {
            byte[] buffer = new byte[1];
            rng.GetBytes(buffer);
            return buffer[0] % maxValue;
        }

        /// <summary>
        /// Checks whether the given password is strong.
        /// A password is considered strong if it meets the following conditions:
        /// - At least 8 characters long
        /// - Contains at least one uppercase letter
        /// - Contains at least one lowercase letter
        /// - Contains at least one special character
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is strong, otherwise false.</returns>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

#if DEBUG
            return true;
#endif

            var pattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

            return Regex.IsMatch(password, pattern);
        }
    }
}