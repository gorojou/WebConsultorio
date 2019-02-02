using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMedico.Extensions
{
    public static class DateTimeExtensions
    {
        private static string DefaultDateTimeFormat = "MM/d/yy hh:mm tt";
        public static string ConsultaSaladeEsperaDateTimeFormat = "d/M/yyyy hh:mm tt";
        public static string CustomDateTimeToString(this DateTime input, string DateTimeFormat = "")
        {
            try
            {
                string result = string.Empty;
                string format = DateTimeFormat;
                if (format == "")
                {
                    format = DefaultDateTimeFormat;
                }
                result = input.ToString(format);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}