using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
namespace LZStringCSharp
{
    public class LZString
    {
        public static byte[] FiletoByteArray(string str)
        {
            byte[] resp = Convert.FromBase64String(str);
            return resp;
        }

        public static string FiletoBase64String(byte[] bytes)
        {
            string resp = Convert.ToBase64String(bytes);
            return resp;
        }
    }
}