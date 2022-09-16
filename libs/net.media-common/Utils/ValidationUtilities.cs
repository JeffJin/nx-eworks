using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace adworks.media_common
{
    public class ValidationUtilities
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        
        public static bool IsValidUserName(string username)
        {
            if (username == null)
            {
                return false;
            }

            var length = username.Length;
            if (length < 3 || length > 32)
            {
                return false;
            }

            // if (!IsLowerAlpha(username[0]))
            // {
            //     return false;
            // }

            // if (!IsLowerAlphanumeric(username[length - 1]))
            // {
            //     return false;
            // }

            if (!Regex.IsMatch(username, "^[a-zA-Z0-9._-]*$"))
            {
                return false;
            }

            if (Regex.IsMatch(username, "[0-9]{5,}"))
            {
                return false;
            }

            // Each username can contain only one of '.', '_', '-'.
            var punctuation = new [] { '.', '_', '-' };
            if (punctuation.Count(c => username.Contains(c)) > 1)
            {
                return false;
            }

            // Each '.', '_', and '-' should be followed by an alpha-numeric.
            for (var i = 0; i < length - 1; i++)
            {
                if (punctuation.Contains(username[i]) && !IsLowerAlphanumeric(username[i + 1]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLowerAlpha(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        private static bool IsLowerAlphanumeric(char c)
        {
            return IsLowerAlpha(c) || (c >= '0' && c <= '9');
        }
    }
}