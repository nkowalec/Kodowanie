using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuffmanCode;
using System.Collections;

namespace Kodowanie_Huffmana
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Brak argumentów, jako pierwszy parametr podaj ścieżkę do pliku!");
            }
            else
            {
                using (Stream stream = File.Open(args[0], FileMode.Open))
                {
                    Tree drzewo = new Tree(stream);
                    Dictionary<string, BitArray> dict = (Dictionary<string, BitArray>)drzewo.ToDictionary();
                    
                    foreach(var item in dict)
                    {
                        Console.WriteLine($"{item.Key} ---> {item.Value.ToBinaryString()}");
                    }
                }
            }
        }
    }
}
