
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Serialization;

/*
    Created By      :   Sandip variya.
    Created On      :   03-Fab-2021.
    About File      :   Here are all the Generic methods implemented.
 */
namespace MM.Common.Helpers.Common.Services
{
    public static class Utility
    {
        public static readonly RNGCryptoServiceProvider _rngProvider = new RNGCryptoServiceProvider();
        public static string _LOWERCASEALPHABETICAL = "abcdefghjkmnopqrstuvwxyz";
        public static string _UPPERCASEALPHABETICAL = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string _NUMERICAL = "0123456789";
        public static string _SYMBOL = "@%+!#$^?:.(){}[]~-_`";
        public static int keySize = 256;

        #region Generate Password
        /// <summary>
        /// Generate dynamic(random) password.(Created By Variya Sandip)
        /// </summary>
        /// <param name="numOfPass">Numbers of Password.</param>
        /// <param name="lower">Number of lower case characters.</param>
        /// <param name="upper">Number of uper case characters.</param>
        /// <param name="num">Number of num characters.</param>
        /// <param name="symbol">Number of symbol characters.</param>
        /// <returns></returns>
        public static string GeneratePassaword(int numOfPass, int lower, int upper, int num, int symbol)
        {
            string pass = string.Empty;
            for (int i = 0; i < numOfPass; i++)
            {
                pass = GenerateRandomPassword(lower, upper, num, symbol);
            }

            return pass;
        }


        public static int NextInt32()
        {
            var randomBuffer = new byte[4];
            _rngProvider.GetBytes(randomBuffer);
            return BitConverter.ToInt32(randomBuffer, 0);
        }

        // Generate a random real number within range [0.0, 1.0]
        public static double Next()
        {
            var buffer = new byte[sizeof(uint)];
            _rngProvider.GetBytes(buffer); // Fill the buffer
            uint random = BitConverter.ToUInt32(buffer, 0);
            return random / (1.0 + uint.MaxValue);
        }

        // Generate an int within range [min, max - 1] if max > min, and min if min == max
        public static int Next(int min, int max)
        {
            if (min > max) throw new ArgumentException($"Second input ({max}) must be greater than first input ({min}))");
            return (int)(min + (max - min) * Next());
        }

        // Generate an int within range [0, max - 1] if max > 1, and 0 if max == {0, 1}
        public static int Next(int max) => Next(0, max);



        public static string GenerateRandomPassword(int numOfLowerCase, int numOfUpperCase, int numOfNumeric, int numOfSymbol)
        {
            string ordered = RandomLowerCase(numOfLowerCase) + RandomUpperCase(numOfUpperCase) + RandomNumerical(numOfNumeric) + RandomSymbol(numOfSymbol);

            string shuffled = new string(ordered.OrderBy(n => NextInt32()).ToArray());

            return shuffled;
        }

        public static string RandomUpperCase(int length) => GetRandomString(_UPPERCASEALPHABETICAL, length);
        public static string RandomLowerCase(int length) => GetRandomString(_LOWERCASEALPHABETICAL, length);
        public static string RandomNumerical(int length) => GetRandomString(_NUMERICAL, length);
        public static string RandomSymbol(int length) => GetRandomString(_SYMBOL, length);


        private static string GetRandomString(string possibleChars, int len)
        {
            var result = string.Empty;
            for (var position = 0; position < len; position++)
            {
                var index = Next(possibleChars.Length);
                result += possibleChars[index];
            }
            return result;
        }

        #endregion end Generate Password

        #region Generate OTP
        /// <summary>
        /// Generate 4 digit number OTP.(Created By Variya Sandip)
        /// </summary>
        /// <returns>OTP</returns>
        public static int GenerateOTP()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        #endregion

        #region Encrypt/Decrypt Encrypt, decrypt and generate a key using AES256.

        /// <summary>
        /// Generate a private key
        /// From : www.chapleau.info/blog/2011/01/06/usingsimplestringkeywithaes256encryptioninc.html
        /// https://gist.github.com/haeky/5797333
        /// </summary>
        private static string GenerateKey(int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = iKeySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.GenerateIV();
            string ivStr = Convert.ToBase64String(aesEncryption.IV);
            aesEncryption.GenerateKey();
            string keyStr = Convert.ToBase64String(aesEncryption.Key);
            string completeKey = ivStr + "," + keyStr;

            return Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(completeKey));
        }


        /// <summary>
        /// Encrypt
        /// From : www.chapleau.info/blog/2011/01/06/usingsimplestringkeywithaes256encryptioninc.html
        /// https://gist.github.com/haeky/5797333
        /// </summary>
        private static string Encrypt(string PlainText)
        {

            string completeEncodedKey = GenerateKey(keySize);

            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = keySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.IV = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(completeEncodedKey)).Split(',')[0]);
            aesEncryption.Key = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(completeEncodedKey)).Split(',')[1]);
            byte[] plainText = ASCIIEncoding.UTF8.GetBytes(PlainText);
            ICryptoTransform crypto = aesEncryption.CreateEncryptor();
            byte[] cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// Decrypt
        /// From : www.chapleau.info/blog/2011/01/06/usingsimplestringkeywithaes256encryptioninc.html
        /// https://gist.github.com/haeky/5797333
        /// </summary>
        private static string Decrypt(string iEncryptedText)
        {
            string completeEncodedKey = GenerateKey(keySize);

            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = keySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.IV = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(completeEncodedKey)).Split(',')[0]);
            aesEncryption.Key = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(completeEncodedKey)).Split(',')[1]);
            ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64CharArray(iEncryptedText.ToCharArray(), 0, iEncryptedText.Length);
            return ASCIIEncoding.UTF8.GetString(decrypto.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length));
        }

        #endregion Encrypt/Decrypt

        #region Convert List To DataTable 

        /// <summary>
        /// Convert List to DataTable.(Created By Variya Sandip)
        /// How to use this method ConvertListToDataTable(Students);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static DataTable ConvertListToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        #endregion Convert List To DataTable

        #region DataTable To Json
        /// <summary>
        /// Convert DataTable To Json using Newtonsoft (Created By Variya Sandip)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string DataTableToJson(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }

        #endregion DataTable To Json

        #region Convert DataTable To List

        private static List<T> ConvertDataTableToDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        #endregion Convert DataTable To List

        #region Xml Serilization
        /// <summary>
        /// Deserialize strintg
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string input)
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        /// <summary>
        /// Serialize strintg
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectToSerialize"></param>
        /// <returns></returns>
        public static string Serialize<T>(T ObjectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                return textWriter.ToString();
            }
        }
        #endregion

        #region Delete File
        /// <summary>
        /// Delete file from physical path
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        #endregion Delete File

        #region Short Day
        /// <summary>
        /// Return short day name
        /// </summary>
        /// <param name="dayText"></param>
        /// <returns></returns>
        public static string GetShortDay(string dayText)
        {
            string shortDay = string.Empty;
            dayText = dayText.ToUpper();
            if (dayText == "MONDAY")
            {
                shortDay = "MO";
            }
            else if (dayText == "TUESDAY")
            {
                shortDay = "TU";
            }
            else if (dayText == "WEDNESDAY")
            {
                shortDay = "WE";
            }
            else if (dayText == "THURSDAY")
            {
                shortDay = "TH";
            }
            else if (dayText == "FRIDAY")
            {
                shortDay = "FR";
            }
            else if (dayText == "SATURDAY")
            {
                shortDay = "SA";
            }
            else if (dayText == "SUNDAY")
            {
                shortDay = "SU";
            }

            return shortDay;
        }

        #endregion Short Day

        #region Year List
        /// <summary>
        /// Get list of year from current year
        /// </summary>
        /// <returns></returns>
        public static List<int> GetYearList()
        {
            List<int> lstYear = new List<int>();

            for (int i = DateTime.Now.Year; i <= DateTime.Now.Year + 20; i++)
            {
                lstYear.Add(i);
            }
            return lstYear;
        }

        #endregion Year List

        #region Month List

        /// <summary>
        /// Get months
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMonthList()
        {
            List<string> lstMonth = new List<string>();

            for (int i = 1; i <= 12; i++)
            {
                if (i.ToString().Length == 1)
                {
                    lstMonth.Add("0" + i.ToString());
                }
                else
                {
                    lstMonth.Add(i.ToString());
                }
            }
            return lstMonth;
        }

        #endregion Month List 

        #region Day List

        /// <summary>
        /// Get days list
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDayList()
        {
            string[] array = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            List<string> lstDay = array.ToList();
            return lstDay;
        }

        #endregion Day List

        #region Get Day Number
        /// <summary>
        /// Get day no from day name
        /// </summary>
        /// <param name="dayName"></param>
        /// <returns></returns>
        public static int GetDayOfNumber(string dayName)
        {
            int dayNo = 0;
            switch (dayName.Trim().ToUpper())
            {
                case "MONDAY":
                    dayNo = 1;
                    break;
                case "TUESDAY":
                    dayNo = 2;
                    break;
                case "WEDNESDAY":
                    dayNo = 3;
                    break;
                case "THURSDAY":
                    dayNo = 4;
                    break;
                case "FRIDAY":
                    dayNo = 5;
                    break;
                case "SATURDAY":
                    dayNo = 6;
                    break;
                case "SUNDAY":
                    dayNo = 7;
                    break;
                default:
                    break;
            }
            return dayNo;
        }

        #endregion  Get Day Number

        #region Create New GUID

        /// <summary>
        /// Generate GUID
        /// </summary>
        /// <returns></returns>
        public static Guid GenerateNewGUID()
        {
            return Guid.NewGuid();
        }

        #endregion Create New GUID
    }
}
