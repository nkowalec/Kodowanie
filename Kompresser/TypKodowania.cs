using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kompresser
{
    class TypKodowania
    {
        public const string HUFFMAN = "Huffman";
        public const string ARYTHMETIC = "Arytmetyczne";
        public const string DICT_LZW = "Słownikowe (LZW)";

        public static IEnumerable<string> GetList()
        {
            var pola =typeof(TypKodowania).GetFields().Where(x => x.IsLiteral && !x.IsInitOnly);
            foreach(var item in pola)
            {
                yield return (string)item.GetValue(null);
            }
        }
    }
}
