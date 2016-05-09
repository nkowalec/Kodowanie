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
            text = tekst;
        }

        private void GetPropabilityFromTxt()
        {
            text.Seek(0, SeekOrigin.Begin);
            var dictTmp = new Dictionary<char, int>();
            dict = new Dictionary<char, double>();
            StreamReader sr = new StreamReader(text);
            int znak;
            while ((znak = sr.Read()) > -1)
            {
                if (!dictTmp.ContainsKey((char)znak))
                {
                    dictTmp.Add((char)znak, 0);
                }
                dictTmp[(char)znak] += 1;
            }

            foreach (var item in dictTmp)
            {
                dict.Add(item.Key, (double)item.Value / text.Length);
            }
        }

        Dictionary<char, double> dict { get; set; }

        Stream text;

        public double CodeIt()
        {
            GetPropabilityFromTxt();
            List<Przedzial> przedzialy = new List<Przedzial>();

            double From = 0;
            double To = 1;
            Przedzial current = Przedzial.Empty;
            text.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(text);
            int znak = 0;

            while ((znak = sr.Read()) > -1)
            {
                bool first = true;
                double probability = 0;

                foreach (var item in dict)
                {
                    Przedzial p = Przedzial.Empty;
                    if (first)
                    {
                        p = new Przedzial(item.Key, From, From + ((To - From) * (probability += item.Value)));
                        first = false;
                    }
                    else
                    {
                        p = new Przedzial(item.Key, From + ((To - From) * probability), From + ((To - From) * (probability += item.Value)));
                    }
                    przedzialy.Add(p);
                }
                current = przedzialy.Where(x => x.Znak == (char)znak).First();
                From = current.From;
                To = current.To;
                WritePrzedzialy(przedzialy);
                przedzialy.Clear();
            }

            return current.From;
        }

        public string DecodeIt(double num)
        {
            Console.WriteLine(num);
            StringBuilder sb = new StringBuilder();
            List<Przedzial> przedzialy = new List<Przedzial>();
            Przedzial p = Przedzial.Empty;

            double From = 0;
            double To = 1;

            while (p.From != num)
            {
                bool first = true;
                double probability = 0;

                foreach (var item in dict)
                {
                    p = Przedzial.Empty;
                    if (first)
                    {
                        p = new Przedzial(item.Key, From, From + ((To - From) * (probability += item.Value)));
                        first = false;
                    }
                    else
                    {
                        p = new Przedzial(item.Key, From + ((To - From) * probability), From + ((To - From) * (probability += item.Value)));
                    }
                    przedzialy.Add(p);
                }

                p = przedzialy.Where(x => x.Contains(num)).First();
                From = p.From;
                To = p.To;
                sb.Append(p.Znak);
                przedzialy.Clear();
            }
            return sb.ToString();
        }

        private void WritePrzedzialy(List<Przedzial> przedzialy)
        {
            bool first = true;
            foreach(var item in przedzialy)
            {
                if (first)
                {
                    Console.Write($"0 - {item.Znak} - {item.To}");
                    first = false;
                }else
                {
                    Console.Write($" - {item.Znak} - {item.To}");
                }
            }
            Console.WriteLine();
        }
    }

    public struct Przedzial
    {
        public char Znak { get; set; }
        public double From { get; set; }
        public double To { get; set; }

        public Przedzial(Char znak, double from, double to)
        {
            Znak = znak;
            From = from;
            To = to;
        }

        public bool Contains(double value)
        {
            return From <= value && To > value;
        }

        public static Przedzial Empty = new Przedzial();
    }
}
