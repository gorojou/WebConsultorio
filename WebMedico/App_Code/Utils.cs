using System;
using System.Collections.Generic;
using System.Linq;

namespace WebMedico.App_Code
{
    public class Utils
    {
        public static string getRandomColor()
        {
            var letters = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

            var color = "#";
            for (var i = 0; i < 6; i++)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                //color += letters[(int)Math.Floor(random.NextDouble() * 16)];
                color += letters[random.Next(0, 15)];
            }
            return color;
        }

        public static string GetColor(string label)
        {
            var labels = new Dictionary<string, string>
            {
                { "Total","rgba(255, 99, 132, 0.4)"},
                { "Trigliceridos","rgba(54, 162, 235, 0.4)"},
                { "HDL","rgba(255, 206, 86, 0.4)"},
                { "LDL","rgba(75, 192, 192, 0.4)"},
                { "UI","rgba(153, 102, 255, 0.4)"},
                { "Insulina","rgba(255, 159, 64, 0.4)"},
                { "Peso","rgba(128,128,0,0.4)"},
                { "IMC","rgba(255,0,255,0.4)"},
                { "Peito","rgba(0,128,128,0.4)"},
                { "Cintura","rgba(128,0,0,0.4)"},
                { "Bra.esq","rgba(0,255,255,0.4)"},
                { "Bra.dto","rgba(0,0,128,0.4)"},
                { "P.esq","rgba(192,192,192,0.4)"},
                { "P.dta","rgba(128,128,128,0.4)"},
                { "Min","rgba(255,0,0,0.4)"},
                { "Max","rgba(0,255,0,0.4)"},
                { "Bpm","rgba(128,0,128,0.4)"},
                { "Valor","rgba(0,0,255,0.4)"}
            };

            var color = labels.SingleOrDefault(l => l.Key == label).Value;

            if (color == null)
            {
                return getRandomColor();
            }
            return color;
        }

        public static string Age(DateTime birth)
        {
            var age = (int)(((TimeSpan)(DateTime.Now - birth)).Days / 365.25);
            return age.ToString();
        }
    }
}