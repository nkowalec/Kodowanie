using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HuffmanCode 
{
    public class Node
    {
        public Node(string word, double probability)   //Tworzy pusty liść
        {
            Word = word;
            Probability = probability;
            Level = 0;
            NodeR = null;
            NodeL = null;
            Parent = null;
        }
        
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
        
        public List<bool> GetNodeFullCode()
        {
            List<bool> lista = new List<bool>();
            Node.GetNodeFullCode(this, lista);
            lista.Reverse();
            return lista;
        }
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
        
        public Tree()
        {
            
        }
        
        public Tree(IDictionary<string, int> dict, int length)  //Wersja przyjmująca słownik słów z ilościami wystąpień oraz ilość ogółem
        {
            foreach (var item in dict)
            {
                AddEmptyNode(item.Key, item.Value / length);
            }
            BlockedEmpty = true;
        }
        
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
                    AddEmptyNode(item.Key, item.Value / sr.BaseStream.Length);
                }
                this.GenerateTree();
                BlockedEmpty = true;
            }
        }
        
        public void AddNode(Node node)
        {
            if(this.Contains(node.Word)) throw new Exception($"Struktura zawiera już podane słowo: {node.Word}");
            NodeList.Add(node);
        }
        private void AddNodeCalc(Node nodeL, Node nodeR)
        {
            NodeList.Add(new Node(nodeL, nodeR));
        }
        
        public void AddEmptyNode(string word, double probability)
        {
            if(BlockedEmpty) throw new Exception("Drzewo zostało zablokowane, nie można dodawać pustych liści");
            NodeList.Add(new Node(word, probability));
        }
        
        public bool Contains(string Word)
        {
            return NodeList.Where(x => x.Word == Word).Count() > 0;
        }
        
        public Node this[string word]
        {
            get
            {
                var nodes = NodeList.Where(x => x.Word == word);
                if(nodes.Count() < 1) throw new Exception("Struktura nie zawiera elementu");
                return nodes.First();
            }
        }
        
        public bool IsPrepared()
        {
            var superNodeCollection = NodeList.Where(x => x.Parent == null && x.NodeL != null && x.NodeR != null);
            var childNodes = NodeList.Where(x => x.Parent == null && x.NodeL == null && x.NodeR == null);
            return ( (superNodeCollection.Count() == 1 && childNodes.Count() == 0) || (superNodeCollection.Count() == 0 && childNodes.Count() == 1) );
        }
        
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
        
        public void GenerateTree()      //Generuje drzewo na podstawie Levelu 0;
        {
            while(!IsPrepared())
            {
                List<Node> topNodes = GetTopOrderedNodes();
                AddNodeCalc(topNodes[0], topNodes[1]);
            }  
        }
        
        private List<Node> GetTopOrderedNodes()
        {
            return NodeList.Where(x => x.Parent == null && x.Probability < 1).OrderBy(x => x.Probability).ToList();
        }
    }
}