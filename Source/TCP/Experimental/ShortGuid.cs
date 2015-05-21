using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace netSharp.TCP.Experimental
{
    public static class ShortGuid
    {
        public static string NewShortGuid()
        {
            int guidLength = 4;

            string charString = "abcdefghijklmnopqrstuvwxyz0123456789";

            StringBuilder builder = new StringBuilder();
            char @char;
            Random random = new Random();

            for (int i = 0; i < guidLength; i++)
            {
                @char = charString[random.Next(0,charString.Length)];
                builder.Append(@char);
            }
            return builder.ToString();
        }
    }
}
