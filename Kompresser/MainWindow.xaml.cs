using ArythmeticCode;
using Microsoft.Win32;
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
                case TypKodowania.HUFFMAN: UseHuffmanDecompress(); break;
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
            HuffmanTest.HuffmanTree tree = new HuffmanTest.HuffmanTree();
            string tekst;
            using(var sr = new StreamReader(File))
            {
                tekst = sr.ReadToEnd();
            }
            tree.Build(tekst);
            var bit_array = tree.Encode(tekst);

            byte[] bytes = new byte[bit_array.Length / 8 + (bit_array.Length % 8 == 0 ? 0 : 1)];
            bit_array.CopyTo(bytes, 0);
            string fn;
            StringBuilder sb = new StringBuilder();
            foreach(byte item in bytes)
            {
                sb.Append((char)item);
            }

            textBlock.Text = sb.ToString();

            if (checkBox.IsChecked == true)
            {
                using (var bw = new BinaryWriter(SaveFile(out fn)))
                {
                    bw.Write(bytes);
                }
            }
        }

        private void UseHuffmanDecompress()
        {
            HuffmanTest.HuffmanTree tree = new HuffmanTest.HuffmanTree();
            string tekst;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Wybierz plik dla przygotowania drzewa - źródło";
            if (dialog.ShowDialog() == true)
            {
                using (var sr = new StreamReader(dialog.FileName))
                {
                    tekst = sr.ReadToEnd();
                }
                tree.Build(tekst);

                BitArray bit_array = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(File, FileMode.Open))
                    {
                        fs.CopyTo(ms);
                        bit_array = new BitArray(ms.ToArray());
                    }
                }

                var result = tree.Decode(bit_array);
                textBlock.Text = result;
                string fn;
                if (checkBox.IsChecked == true)
                {
                    using (var bw = new StreamWriter(SaveFile(out fn)))
                    {
                        bw.Write(result);
                    }
                }
            }
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
            StringBuilder sb = new StringBuilder();
            foreach(byte item in result)
            {
                sb.Append((char)item);
            }

            textBlock.Text = sb.ToString();

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
            StringBuilder sb = new StringBuilder();
            foreach (byte item in result)
            {
                sb.Append((char)item);
            }

            textBlock.Text = sb.ToString();

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
