using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huffman
{
    /// <summary>
    /// Implementacja gałęzi drzewa
    /// </summary>
    public class Node
    {
        public char Symbol { get; set; }    //znak liścia
        public int Frequency { get; set; }  //Wystąpienia znaku
        public Node Right { get; set; }     //Prawe dziecko
        public Node Left { get; set; }      //Lewe dziecko

        /// <summary>
        /// Metoda zwracająca wynik "listę bitów" dla symbolu
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<bool> Traverse(char symbol, List<bool> data)
        {
            if (Right == null && Left == null)
            {
                if (symbol.Equals(this.Symbol))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null)
                {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(symbol, leftPath);
                }

                if (Right != null)
                {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(symbol, rightPath);
                }

                if (left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }
    }
    public class HuffmanTree
    {
        private List<Node> nodes = new List<Node>();    //lista gałęzi / liści
        public Node Root { get; set; }  //Korzeń drzewa
        public Dictionary<char, int> Frequencies = new Dictionary<char, int>();    //wystąpienia poszczególnych znaków

        /// <summary>
        /// Przygotowanie drzewa dla wprowadzonego tekstu źródłowego
        /// </summary>
        /// <param name="source"></param>
        public void Build(string source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (!Frequencies.ContainsKey(source[i]))
                {
                    Frequencies.Add(source[i], 0);
                }

                Frequencies[source[i]]++;
            }

            foreach (KeyValuePair<char, int> symbol in Frequencies)
            {
                nodes.Add(new Node() { Symbol = symbol.Key, Frequency = symbol.Value });
            }

            while (nodes.Count > 1)
            {
                List<Node> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList<Node>();

                if (orderedNodes.Count >= 2)
                {
                    // Take first two items
                    List<Node> taken = orderedNodes.Take(2).ToList<Node>();

                    // Create a parent node by combining the frequencies
                    Node parent = new Node()
                    {
                        Symbol = '*',
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                this.Root = nodes.FirstOrDefault();

            }

        }
        /// <summary>
        /// Metoda zwracająca zakodowaną wartość dla wprowadzonego tekstu
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public BitArray Encode(string source)
        {
            List<bool> encodedSource = new List<bool>();

            for (int i = 0; i < source.Length; i++)
            {
                List<bool> encodedSymbol = this.Root.Traverse(source[i], new List<bool>());
                encodedSource.AddRange(encodedSymbol);
            }

            BitArray bits = new BitArray(encodedSource.ToArray());

            return bits;
        }

        /// <summary>
        /// Metoda dekodująca Huffman code
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public string Decode(BitArray bits)
        {
            Node current = this.Root;
            string decoded = "";

            foreach (bool bit in bits)
            {
                if (bit)
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }

                if (IsLeaf(current))
                {
                    decoded += current.Symbol;
                    current = this.Root;
                }
            }

            return decoded;
        }

        public bool IsLeaf(Node node)
        {
            return (node.Left == null && node.Right == null);
        }

    }
}