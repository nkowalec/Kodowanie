using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuffmanCode;
using System.Collections;
using ArythmeticCode;

namespace Kodowanie_Huffmana
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Brak argumentów, jako pierwszy parametr podaj ścieżkę do pliku!");

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("aabbaabdcd")))
                {

                    Schema aryth = new Schema(stream);
                    Console.WriteLine(aryth.CodeIt());

                    Console.ReadKey();

                }

                
            }
            else
            {
                using (Stream stream = File.Open(args[0], FileMode.Open))
                {
                    Tree drzewo = new Tree(stream);     //Utwórz drzewo na podstawie strumienia (pliku)
                    Dictionary<string, BitArray> dict = (Dictionary<string, BitArray>)drzewo.ToDictionary();    //Pobierz z gotowego drzewa słownik 'znak' -> 'bit kod'
                    
                    foreach(var item in dict)   
                    {
                        Console.WriteLine($"{item.Key} ---> {item.Value.ToBinaryString()}");    //Wypisz informację o każdej parze 'znak' -> 'kod'
                    }

                    Entropia(drzewo);   //Pisz entropię

                    SredniaWyniku(drzewo);      //Pisz średnią długość wynikową

                    //stream.Seek(0, SeekOrigin.Begin);
                    Console.WriteLine();
                    using(StreamReader reader = new StreamReader(args[0]))
                    {
                        while(reader.Peek() >= 0)
                        {

                            Console.Write(dict[((char)reader.Read()).ToString()].ToBinaryString());     //Zakoduj Przekazany plik, wynik do konsoli (tylko)
                        }
                    }

                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Wypisuje na konsoli średnią długość kodu wynikowego
        /// </summary>
        /// <param name="drzewo"></param>
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

        /// <summary>
        /// Wypisuje na konsoli Entropię dla zadanego drzewa
        /// </summary>
        /// <param name="drzewo"></param>
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
