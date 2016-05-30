using ArythmeticCode;
using HuffmanCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LZW = LzwCode;

namespace Kompresser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Main Elements
        private string File { get; set; } 
        public MainWindow()
        {
            InitializeComponent();

            PrepareComboTyp();
        }

        private void PrepareComboTyp()
        {
            var lista = TypKodowania.GetList();
            comboTyp.ItemsSource = lista;
        }

        private void WyborPliku_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = false;
            bool? res = dialog.ShowDialog();

            if (res == true)
            {
                File = dialog.FileName;
                fileLabel.Content = System.IO.Path.GetFileName(dialog.FileName);
                labelTyp.Visibility = Visibility.Visible;
                comboTyp.Visibility = Visibility.Visible;
            }
        }

        private void comboTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Compress.Visibility != Visibility.Visible)
            {
                Compress.Visibility = Visibility.Visible;
                Decompress.Visibility = Visibility.Visible;
            }
        }

        private Stream SaveFile(out string filename)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            
            if(dialog.ShowDialog() == true)
            {
                filename = dialog.FileName;
                return dialog.OpenFile();
            }
            filename = null;
            return null;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            checkBox.Content = checkBox.IsChecked == true ? "TAK" : "NIE";
        }
        #endregion

        #region Compress/Decompress Btn's
        private void Compress_Click(object sender, RoutedEventArgs e)
        {
            switch ((string)comboTyp.SelectedValue)
            {
                case TypKodowania.DICT_LZW: UseLZWCompress(); break;
                case TypKodowania.HUFFMAN: UseHuffmanCompress(); break;
                case TypKodowania.ARYTHMETIC: UseArythmCompress(); break;
                default: MessageBox.Show("Ta metoda nie została jeszcze zaimplementowana"); break;
            }
        }

        private void Decompress_Click(object sender, RoutedEventArgs e)
        {
            switch ((string)comboTyp.SelectedValue)
            {
                case TypKodowania.DICT_LZW: UseLZWDecompress(); break;
                case TypKodowania.ARYTHMETIC: UseArythmDecompress(); break;
                default: MessageBox.Show("Ta metoda nie została jeszcze zaimplementowana"); break;
            }
        }
        #endregion

        #region LZW
        private void UseLZWCompress()
        {
            LZW.LzwCode lzw = new LzwCode.LzwCode(File);
            List<int> comp = lzw.Compress();
            foreach (var item in comp)
            {
                textBlock.Text += (char)item;
            }
            if (checkBox.IsChecked == true)
            {
                string filename;
                var stream = SaveFile(out filename);
                if(stream != null)
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(textBlock.Text);
                    }
                }
            }
        }
        private void UseLZWDecompress()
        {
            LZW.LzwCode lzw = new LZW.LzwCode(File);
            textBlock.Text = lzw.Decompress();

            if (checkBox.IsChecked == true)
            {
                string filename;
                var stream = SaveFile(out filename);
                if (stream != null)
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(textBlock.Text);
                    }
                }
            }
        }
        #endregion

        #region Huffman
        private void UseHuffmanCompress()
        {
            Tree drzewo = new Tree(new FileStream(File, FileMode.Open));
            Dictionary<string, BitArray> dict = (Dictionary<string, BitArray>)drzewo.ToDictionary();

            List<bool> dane = new List<bool>();
            using(StreamReader sr = new StreamReader(File))
            {
                while(sr.Peek() >= 0)
                {
                    foreach(bool val in dict[((char)sr.Read()).ToString()])
                    {
                        dane.Add(val);
                    }
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                
                foreach(var item in dict)
                {
                    bw.Write(item.Key);
                    foreach (bool val in item.Value) bw.Write(val);
                    bw.Write('\n');
                }

                foreach(bool val in dane)
                {
                    bw.Write(val);
                }
                
                ms.Seek(0, SeekOrigin.Begin);

                StreamReader sw = new StreamReader(ms);
                textBlock.Text = sw.ReadToEnd();
                
                if(checkBox.IsChecked == true)
                {
                    string filename;
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(SaveFile(out filename));
                }
            }
        }

        private void UseHuffmanDecompress()
        {

        }
        #endregion

        #region Arythmetic

        private void UseArythmCompress()
        {
            ArythmeticCode.Compression.ArithmeticCompressor cmp = new ArythmeticCode.Compression.ArithmeticCompressor();

            List<byte> daneI = new List<byte>();
            using(BinaryReader br = new BinaryReader(new FileStream(File, FileMode.Open)))
            {
                for(int i = 0; i < br.BaseStream.Length; i++)
                {
                    daneI.Add(br.ReadByte());
                }
            }

            var result = cmp.Compress(daneI);

            if(checkBox.IsChecked == true)
            {
                string filename;
                using(BinaryWriter sw = new BinaryWriter(SaveFile(out filename)))
                {
                    sw.Write(result);
                }
            }
        }

        private void UseArythmDecompress()
        {
            //var temp = @"C:\tmp\mojplik.nk";//System.IO.Path.GetTempFileName();
            ArythmeticCode.Compression.ArithmeticCompressor cmp = new ArythmeticCode.Compression.ArithmeticCompressor();

            List<byte> daneI = new List<byte>();
            using (BinaryReader br = new BinaryReader(new FileStream(File, FileMode.Open)))
            {
                for (int i = 0; i < br.BaseStream.Length; i++)
                {
                    daneI.Add(br.ReadByte());
                }
            }
            var result = cmp.Decompress(daneI);

            if (checkBox.IsChecked == true)
            {
                string filename;
                using (BinaryWriter sw = new BinaryWriter(SaveFile(out filename)))
                {
                    sw.Write(result);
                }
            }
        }

        #endregion
    }
}
