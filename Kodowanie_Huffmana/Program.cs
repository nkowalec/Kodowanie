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


                    //stream.Seek(0, SeekOrigin.Begin);
                    Console.WriteLine();
                    using(StreamReader reader = new StreamReader(args[0]))
                    {
                        while(reader.Peek() >= 0)
                        {

                            Console.Write(dict[((char)reader.Read()).ToString()].ToBinaryString());
                        }
                    }
                    
                    Entropia(drzewo);

                    SredniaWyniku(drzewo);

                    Console.ReadKey();
                }
            }
        }

        private static void SredniaWyniku(Tree drzewo)
        {
            Console.WriteLine();
            double suma = 0;

            foreach(Node node in drzewo.CharNodesArray)
            {
                suma += node.Probability * node.GetNodeFullCode().Count;
            }

            Console.WriteLine("Średnia długość kodu: " + suma);
        }

        private static void Entropia(Tree drzewo)
        {
            Console.WriteLine();
            double suma = 0;
            foreach(Node node in drzewo.CharNodesArray)
            {
                suma += (node.Probability * Math.Log(1 / node.Probability, 2));
            }

            Console.WriteLine("Entropia wynosi: " + suma);
        }
    }
}
