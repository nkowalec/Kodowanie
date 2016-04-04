using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HuffmanCode 
{
    public class Node
    {
        /// <summary>
        /// Tworzy liść poziomu zero o określonych wartościach 
        /// </summary>
        /// <param name="word">Trzymane słowo</param>
        /// <param name="probability">Prawdopodobieństwo wystąpienia</param>
        public Node(string word, double probability)   //Tworzy pusty liść
        {
            Word = word;
            Probability = probability;
            Level = 0;
            NodeR = null;
            NodeL = null;
            Parent = null;
        }
        
        /// <summary>
        /// Tworzy element nadrzędny dla przekazanych liści
        /// </summary>
        /// <param name="nodeL"></param>
        /// <param name="nodeR"></param>
        public Node(Node nodeL, Node nodeR)     //Tworzy "Gałąź" dla liści
        {
            if(nodeL.Parent != null) throw new Exception("Liść lewy posiada już Rodzica");
            if(nodeR.Parent != null) throw new Exception("Liść prawy posiada już Rodzica");
            NodeL = nodeL;
            NodeR = nodeR;
            Word = nodeL.Word + nodeR.Word;
            Level = nodeL.Level > nodeR.Level ? nodeL.Level + 1 : nodeR.Level + 1;
            Probability = nodeL.Probability + nodeR.Probability;
            NodeL.bitCode = true;
            NodeR.bitCode = false;
            NodeL.Parent = this;
            NodeR.Parent = this;
            Parent = null;
        }
        
        public string Word { get; private set; } //Słowo do zakodowania
        public int Level { get; private set; } //Poziom w strukturze drzewa
        public double Probability { get; private set; } //Prawdopodobnieństwo wystąpienia znaku
        public Node NodeR { get; private set; } //Referencja do liścia poniżej
        public Node NodeL { get; private set; } //J/W
        public Node Parent { get; private set; }   //Rodzic dla danego liścia
        internal bool bitCode { get; private set; } //Przechowuje wartość bitową dla danego poziomu
        
        /// <summary>
        /// Zwraca pełen kod bitowy znaku (liścia)
        /// </summary>
        /// <returns></returns>
        public List<bool> GetNodeFullCode()
        {
            List<bool> lista = new List<bool>();
            Node.GetNodeFullCode(this, lista);
            lista.Reverse();
            return lista;
        }

        /// <summary>
        /// Wylicza rekurencyjnie kod bitowy dla przekazanego
        /// liścia
        /// </summary>
        /// <param name="node"></param>
        /// <param name="lista"></param>
        private static void GetNodeFullCode(Node node, List<bool> lista)
        {
            if (node.Parent != null)
            {
                lista.Add(node.bitCode);
                Node.GetNodeFullCode(node.Parent, lista);
            }
        }
    }
    
    public class Tree
    {
        private List<Node> NodeList { get; set; } = new List<Node>();
        private bool BlockedEmpty = false;
        
        /// <summary>
        /// Tworzy puste drzewo
        /// </summary>
        public Tree()
        {
            
        }
        
        /// <summary>
        /// Tworzy drzewo z wypełnionym poziomem 0 
        /// na podstawie przekazanego słownika znaków i prawdopodobieństw
        /// </summary>
        /// <param name="dict">Słownik</param>
        /// <param name="length">Długość całego tekstu</param>
        public Tree(IDictionary<string, int> dict, int length)  //Wersja przyjmująca słownik słów z ilościami wystąpień oraz ilość ogółem
        {
            foreach (var item in dict)
            {
                AddEmptyNode(item.Key, (double)item.Value / length);
            }
            BlockedEmpty = true;
        }
        
        /// <summary>
        /// Tworzy drzewo na podstawie pliku
        /// </summary>
        /// <param name="stream">Plik wejściowy</param>
        public Tree(Stream stream)  //Tworzy obiekt drzewa na podstawie strumienia plikowego
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();
            using(StreamReader sr = new StreamReader(stream))
            {
                while (sr.Peek() >= 0)
                {
                    string znak = ((char)sr.Read()).ToString();
                    if(!temp.ContainsKey(znak))
                    {
                        temp.Add(znak, 0);
                    }
                    temp[znak] += 1;
                }
                
                foreach (var item in temp)
                {
                    AddEmptyNode(item.Key, (double)item.Value / sr.BaseStream.Length);
                }
                this.GenerateTree();
                BlockedEmpty = true;
            }
        }
        
        /// <summary>
        /// Dodaje liść do drzewa
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(Node node)
        {
            if(this.Contains(node.Word)) throw new Exception($"Struktura zawiera już podane słowo: {node.Word}");
            NodeList.Add(node);
        }

        /// <summary>
        /// Dodaje gałąź dla przekazanych liści
        /// </summary>
        /// <param name="nodeL">Liść lewy</param>
        /// <param name="nodeR">Liść prawy</param>
        private void AddNodeCalc(Node nodeL, Node nodeR)
        {
            NodeList.Add(new Node(nodeL, nodeR));
        }
        
        /// <summary>
        /// Dodaje pusty liść (poziomu zerowego)
        /// </summary>
        /// <param name="word"></param>
        /// <param name="probability"></param>
        public void AddEmptyNode(string word, double probability)
        {
            if(BlockedEmpty) throw new Exception("Drzewo zostało zablokowane, nie można dodawać pustych liści");
            NodeList.Add(new Node(word, probability));
        }
        
        /// <summary>
        /// Sprawdza czy drzewo zawiera określone słowo
        /// </summary>
        /// <param name="Word">Szukane słowo</param>
        /// <returns></returns>
        public bool Contains(string Word)
        {
            return NodeList.Where(x => x.Word == Word).Count() > 0;
        }
        
        /// <summary>
        /// Zwraca liść o określonym słowie
        /// </summary>
        /// <param name="word">Wyszukiwane Słowo</param>
        /// <returns></returns>
        public Node this[string word]
        {
            get
            {
                var nodes = NodeList.Where(x => x.Word == word);
                if(nodes.Count() < 1) throw new Exception("Struktura nie zawiera elementu");
                return nodes.First();
            }
        }
        
        /// <summary>
        /// Określa, czy drzewo jest już przygotowane (czy jest jeden korzeń)
        /// </summary>
        /// <returns></returns>
        public bool IsPrepared()
        {
            var superNodeCollection = NodeList.Where(x => x.Parent == null && x.NodeL != null && x.NodeR != null);
            var childNodes = NodeList.Where(x => x.Parent == null && x.NodeL == null && x.NodeR == null);
            return ( (superNodeCollection.Count() == 1 && childNodes.Count() == 0) || (superNodeCollection.Count() == 0 && childNodes.Count() == 1) );
        }
        
        /// <summary>
        /// Zmienia drzewo w słownik znaków z odpowiadającymi im kodami
        /// </summary>
        /// <returns></returns>
        public IDictionary ToDictionary()
        {
            if(!IsPrepared()) throw new Exception("Kolekcja nie jest gotowa do eksportu");
            Dictionary<string, BitArray> dict = new Dictionary<string, BitArray>();
            
            foreach (Node node in NodeList.Where(x => x.Level == 0))
            {
                dict.Add(node.Word, new BitArray(node.GetNodeFullCode().ToArray()));
            }
            
            return dict;
        }
        
        /// <summary>
        /// Generuj drzewo na podstawie liści poziomu 0
        /// </summary>
        public void GenerateTree()      //Generuje drzewo na podstawie Levelu 0;
        {
            while(!IsPrepared())
            {
                List<Node> topNodes = GetTopOrderedNodes();
                AddNodeCalc(topNodes[0], topNodes[1]);
            }  
        }
        
        /// <summary>
        /// Zbiera elementy najwyższego poziomu
        /// nie posiadające rodzica (elementu nadrzędnego)
        /// </summary>
        /// <returns></returns>
        private List<Node> GetTopOrderedNodes()
        {
            return NodeList.Where(x => x.Parent == null && x.Probability < 1).OrderBy(x => x.Probability).ToList();
        }

        /// <summary>
        /// Tablica liści najniższego poziomu (znaki wejściowe)
        /// </summary>
        public Node[] CharNodesArray
        {
            get
            {
                return NodeList.Where(x => x.Level == 0).ToArray();
            }
        }
    }
}