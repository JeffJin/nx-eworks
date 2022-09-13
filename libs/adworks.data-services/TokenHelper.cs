using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using adworks.data_services.Identity;
using adworks.media_common;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace adworks.data_services
{
    public static class TokenHelper
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static JwtSecurityToken GenerateToken(User user, string audience, string issuer, string key, int duration = 1440)
        {
            string orgName = null;
            if (user.Organization != null && !string.IsNullOrWhiteSpace(user.Organization.Name))
            {
                orgName = user.Organization.Name;
            }

            return GenerateToken(user.Id, user.Email, user.UserName, orgName, audience, issuer, key, duration);
        }
        
        public static JwtSecurityToken GenerateToken(string userId, string userEmail, string userName, string orgName, string audience, string issuer, string key, int duration = 1440)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, audience), 
                new Claim(JwtRegisteredClaimNames.Iss, issuer), 
                new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), 
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(duration).ToUnixTimeSeconds().ToString()), 

                new Claim(ClaimTypes.NameIdentifier, userId), 
                new Claim(ClaimTypes.Email, userEmail), 
                new Claim(ClaimTypes.Name, userName),
            };

            if (!string.IsNullOrWhiteSpace(orgName))
            {
                claims.Add(new Claim(TokenClaimTypes.Organization, orgName));
            }
            else
            {
                claims.Add(new Claim(TokenClaimTypes.Organization, DtoHelper.DefaultOrganization));
            }
            
            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)), 
                new JwtPayload(claims));
           
            return token;
        }
        
        public static string Encrypt(string text, User user)
        {
            var password = new PasswordHasher<User>();
            return password.HashPassword(user, text);
        }

        public static string Encrypt(string plainText, string encryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(plainText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, string encryptionKey)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}