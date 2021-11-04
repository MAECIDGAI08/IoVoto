using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility
{
    public static class Utils
    {

        public static string Encrypt(string plainText, string secretKey) {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
            byte[] rMng = rijndaelManaged.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(rMng);
        }

        public static string Decrypt(string encryptedText, string secretKey) {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
            byte[] rMng = rijndaelManaged.CreateDecryptor().TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(rMng);
        }

        /*
        public static string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }*/

        /*
        public static string Encrypt(string plainText, string passPhrase)
        {
            //byte[] initVectorBytes = Encoding.UTF8.GetBytes("pemgail9uzpgzl88");
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(256 / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.KeySize = 256;
            symmetricKey.Mode = CipherMode.ECB;
            symmetricKey.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, null);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherTextBytes);
            return cipherText;
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            //byte[] initVectorBytes = Encoding.UTF8.GetBytes("emiliol9uzpgzl88");
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(256 / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.KeySize = 256;
            symmetricKey.Mode = CipherMode.ECB;
            symmetricKey.Padding = PaddingMode.PKCS7;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, null);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            return plainText;
        }*/

    public static string Eng2ItDate(string date)
        {
            string itDate = "";
            if (!String.IsNullOrEmpty(date) && date.Length == 10) { itDate = date.Substring(8, 2) + "/" + date.Substring(5, 2) + "/" + date.Substring(0, 4); }
            return itDate;
        }
        public static Boolean ValidateField(string arg, string validation)
        {//valida la coerenza dei dati in input lato server
            //arg: array con i name dei campi da validare
            //validation: array con il tipo di validazione che i campi in arg devono subire
            Boolean validationResult = false;
            switch (validation)
            {
                case "none": validationResult = true; break;
                case "required": if (!String.IsNullOrEmpty(arg)) { validationResult = true; } break;
                case "integer": if (String.IsNullOrEmpty(arg) || (!String.IsNullOrEmpty(arg) && (arg.ToString().All(Char.IsDigit) || arg.ToString().Substring(0, 1) == "-" && arg.ToString().Substring(1).All(Char.IsDigit)))) { validationResult = true; } break;
                case "req_integer": if (!String.IsNullOrEmpty(arg) && (arg.ToString().All(Char.IsDigit) || arg.ToString().Substring(0, 1) == "-" && arg.ToString().Substring(1).All(Char.IsDigit))) { validationResult = true; } break;
                case "req_check": if (!String.IsNullOrEmpty(arg) && arg.ToString().All(Char.IsDigit) && Int32.Parse(arg) == 1) { validationResult = true; } break;
				case "req_cf":
                    Regex rcRegex = new Regex(@"^[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]$");
                    Match rcMatch = rcRegex.Match(arg);
                    if (!String.IsNullOrEmpty(arg) && rcMatch.Success) { validationResult = true; }
                    break;
                case "req_date":
                    Regex rdRegex = new Regex(@"^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((19|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((19|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((19|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$");
                    Match rdMatch = rdRegex.Match(arg);
                    if (!String.IsNullOrEmpty(arg) && rdMatch.Success) { validationResult = true; }
                    break;
            }
            return validationResult;
        }

        public static void LogTrace(string ip, string args)
        {
            string path = Path.GetFullPath(Directory.GetCurrentDirectory() + "/Logs/ExtraLog_" + DateTime.Now.ToString("yyyyMM") + ".txt");
            try
            {
                FileStream fileStream = null;
                fileStream = File.Open(path, File.Exists(path) ? FileMode.Append : FileMode.OpenOrCreate);
                using (StreamWriter fs = new StreamWriter(fileStream))
                {
                    fs.WriteLine(DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss") + " - " + ip + " - " + args);
                };
                fileStream.Close();
            }
            catch (IOException)
            { }
        }
    } // end class

    public static class DateTimeExtensions
    {
        public static DateTime UtcToLocal(this DateTime source,
            TimeZoneInfo localTimeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(source, localTimeZone);
        }

        public static DateTime LocalToUtc(this DateTime source,
            TimeZoneInfo localTimeZone)
        {
            source = DateTime.SpecifyKind(source, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(source, localTimeZone);
        }
    }




}
