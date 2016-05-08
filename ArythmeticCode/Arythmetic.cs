using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArythmeticCode
{
    public class Schema
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="slownik">Słownik znaków z liczbą wystąpień w tekście</param>
        public Schema(Stream tekst)
        {
            dict = new Dictionary<char, int>();
            text = tekst;
            StreamReader sr = new StreamReader(tekst);

            int znak;
            while((znak = sr.Read()) > -1)
            {
                if (!dict.ContainsKey((char)znak))
                {
                    dict.Add((char)znak, 0);
                }
                dict[(char)znak] += 1;
            }
            

            k = (int)Math.Ceiling(Math.Log(4 * tekst.Length , 2));
            M = (int)Math.Round(Math.Pow(2, k));
            Fn = (int)tekst.Length;
            text = tekst;
        }

        Dictionary<char, int> dict { get; set; }

        int M { get; set; }
        int k { get; set; }

        int Fn { get; set; }

        Stream text;

        public decimal CodeIt()
        {
            List<Przedzial> przedzialy = new List<Przedzial>();
            int before = 0;
            foreach(var item in dict)
            {
                przedzialy.Add(new Przedzial(item.Key, before + 1, before += 1 + item.Value * (M / Fn)));
            }
            var tmp = przedzialy.Last();
            tmp = new Przedzial(tmp.Znak, tmp.From, M);

            StreamReader sr = new StreamReader(text);
            int znak;
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            while((znak = sr.Read()) > -1)
            {
                var przedzial = przedzialy.Where(x => x.Znak == (char)znak).First();
                before = przedzial.From;
                M = przedzial.To;
                przedzialy = new List<Przedzial>();

                foreach (var item in dict)
                {
                    przedzialy.Add(new Przedzial(item.Key, before + 1, before += 1 + item.Value * (M / Fn)));
                }
                var tmp2 = przedzialy.Last();
                tmp2 = new Przedzial(tmp.Znak, tmp.From, M);
            }

            return 0;
        }
    }

    public struct Przedzial
    {
        public char Znak { get; set; }
        public int From { get; set; }
        public int To { get; set; }

        public Przedzial(Char znak, int from, int to)
        {
            Znak = znak;
            From = from;
            To = to;
        }
        public static Przedzial Empty = new Przedzial();
    }
}
