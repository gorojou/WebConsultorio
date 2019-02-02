using System;
using System.Web.Mvc;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMedico.Controllers;
namespace WebMedico.Hubs
{
    /// <summary>
    /// Defines method helpers to be used in Views
    /// </summary>
    public static class CustomWebHelpers
    {
        /// <summary>
        /// Returns the date antiquity in string format.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static MvcHtmlString GetDateDifferenceStringFormatted(DateTime date)
        {
            var difference = DateTimeOffset.Now - date;
            var strDate = string.Empty;

            if (difference.Days > 1)
                strDate += difference.Days > 2 ? (difference.Days - 1) + " d, " : "1 d";

            if (difference.Hours > 1)
                strDate += difference.Hours > 2 ? (difference.Hours) + " h, " : "1 h, ";

            if (difference.Minutes > 1)
                strDate += difference.Minutes > 2 ? (difference.Minutes) + " m atrás" : "1 m atrás";

            if (difference.Seconds > 0 & strDate == string.Empty)
                strDate += "Agora";

            return MvcHtmlString.Create(strDate);
        }

        public static string Age(DateTime birth)
        {
            var age = (int)(((TimeSpan)(DateTime.Now - birth)).Days / 365.25);
            return age.ToString();
        }
    }
}