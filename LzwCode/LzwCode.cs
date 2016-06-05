using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LzwCode
{
    /// <summary>
    /// Klasa implementująca kodowanie LZW (słownikowe)
    /// </summary>
    public class LzwCode
    {
        private string Path { get; set; }   //ścieżka do pliku poddawanego działaniom 

        public LzwCode(string _path)
        {
            Path = _path;
        }

        /// <summary>
        /// Metoda wykonująca kompresję 
        /// </summary>
        /// <returns></returns>
        public List<int> Compress()
        {
            // Budowanie słownika startowego
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string w = string.Empty;
            List<int> compressed = new List<int>();

            using (StreamReader sr = new StreamReader(Path, Encoding.ASCII)) {
                //dla każdego znaku w pliku wejściowym
            //    int s;
             //   while((s = sr.Read()) > 0)
                 foreach (char c in sr.ReadToEnd())
                {
                   // char c = (char)s;
                    string wc = w + c;  //pobierz nowy ciąg
                    if (dictionary.ContainsKey(wc))     //sprawdź ciąg w słowniku
                    {
                        w = wc;
                    }
                    else
                    {
                        // dodaj do wyniku
                        compressed.Add(dictionary[w]);    
                        // dodaj ciąg do słownika
                        dictionary.Add(wc, dictionary.Count);
                        w = c.ToString();
                    }
                }
            }
            // dopisz pozostałą wartość, jeśli jest to potrzebne
            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }

        /// <summary>
        /// Metoda wykonująca dekompresję
        /// </summary>
        /// <returns></returns>
        public string Decompress()
        {
            List<int> compressed = new List<int>();
            //Uzupełnij listę na podstawie pliku
            using (StreamReader sr = new StreamReader(Path))
            {
                foreach(char item in sr.ReadToEnd())
                {
                    compressed.Add((int)item);
                }
            }

            // zbuduj słownik startowy
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            string w = dictionary[compressed[0]];   //pobierz pierwszą wartość ze słownika i odczytaj znak ze słownika
            compressed.RemoveAt(0);     // usuń pobraną wartość
            StringBuilder decompressed = new StringBuilder(w);  //utwórz nowy "konstruktor ciągów"

            //Dla każdej wartości w zakodowanym ciągu
            foreach (int k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))      //jeśli wartość znajduje się w słowniku to pobierz znak
                    entry = dictionary[k];
                else if (k == dictionary.Count)     //jeśli nie ma wartości to utwórz nowy ciąg
                    entry = w + w[0];   

                decompressed.Append(entry);

                // dodaj nową wartość do słownika
                dictionary.Add(dictionary.Count, w + entry[0]);

                // nadpisz wartość bierzącą
                w = entry;
            }

            return decompressed.ToString();
        }
    }
}
