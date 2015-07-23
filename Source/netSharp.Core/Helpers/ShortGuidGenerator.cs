using System;
using System.Text;

namespace netSharp.Core.Helpers
{
    public static class ShortGuidGenerator
    {
        public static string NewShortGuid()
        {
            int guidLength = 4;

            string charString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

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
