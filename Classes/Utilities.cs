using System;
using System.Collections.Generic;
using System.Text;

namespace StorybookCabinPOCBlazor
{
    public static class Utilities
    {
        #region TruncateString(string stringToTruncate, int maxLength)
        public static string TruncateString(string stringToTruncate, int maxLength)
        {
            if (stringToTruncate.Length > maxLength)
                return stringToTruncate.Substring(0, maxLength);

            return stringToTruncate;
        }
        #endregion

        #region TruncateStringWithTrailingDots(string stringToTruncate, int maxLength)
        public static string TruncateStringWithTrailingDots(string stringToTruncate, int maxLength)
        {
            if (stringToTruncate.Length > maxLength)
            {
                var TruncatedString = stringToTruncate.Substring(0, maxLength);
                return TruncatedString + "...";
            }

            return stringToTruncate;
        }
        #endregion

        #region ConvertDateToPacificStandardTime
        public static DateTime ConvertDateToPacificStandardTime(DateTime dateToConvert)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateToConvert, "Pacific Standard Time");
        }
        #endregion

        #region public static string GenerateRandomPassword(int PasswordLength)
        public static string GenerateRandomPassword(int PasswordLength)
        {
            string Password = "";
            string _allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$";
            Random randNum = new Random();
            char[] chars = new char[PasswordLength];

            for (int i = 0; i < PasswordLength; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }

            Password = new string(chars);
            return Password;
        } 
        #endregion

        #region FirstWord
        public static String FirstWord(string sourceString)
        {
            string workString = sourceString.Trim();
            workString = workString.Replace(",", " ");
            workString = workString.Replace(".", " ");
            workString = workString.Replace("-", " ");

            int spaceIndex = workString.IndexOf(" ");
            return spaceIndex > -1 ? workString.Substring(0, spaceIndex) : workString;
        }
        #endregion
    }
}