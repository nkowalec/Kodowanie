using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LzwCode
{
    public class LzwCode
    {
        private string Path { get; set; }

        public LzwCode(string _path)
        {
            Path = _path;
        }

        public List<int> Compress()
        {
            // build the dictionary
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string w = string.Empty;
            List<int> compressed = new List<int>();

            using (StreamReader sr = new StreamReader(Path)) {
                foreach (char c in sr.ReadToEnd())
                {
                    string wc = w + c;
                    if (dictionary.ContainsKey(wc))
                    {
                        w = wc;
                    }
                    else
                    {
                        // write w to output
                        compressed.Add(dictionary[w]);
                        // wc is a new sequence; add it to the dictionary
                        dictionary.Add(wc, dictionary.Count);
                        w = c.ToString();
                    }
                }
            }
            // write remaining output if necessary
            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }

        public string Decompress()
        {
            List<int> compressed = new List<int>();
            using (StreamReader sr = new StreamReader(Path))
            {
                foreach(char item in sr.ReadToEnd())
                {
                    compressed.Add((int)item);
                }
            }

            // build the dictionary
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            string w = dictionary[compressed[0]];
            compressed.RemoveAt(0);
            StringBuilder decompressed = new StringBuilder(w);

            foreach (int k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))
                    entry = dictionary[k];
                else if (k == dictionary.Count)
                    entry = w + w[0];

                decompressed.Append(entry);

                // new sequence; add it to the dictionary
                dictionary.Add(dictionary.Count, w + entry[0]);

                w = entry;
            }

            return decompressed.ToString();
        }
    }
}
