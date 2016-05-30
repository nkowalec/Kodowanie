using System;
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

        private Stream SaveFile()
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();

            if(dialog.ShowDialog() == true)
            {
                return dialog.OpenFile();
            }

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
                default: MessageBox.Show("Ta metoda nie została jeszcze zaimplementowana"); break;
            }
        }

        private void Decompress_Click(object sender, RoutedEventArgs e)
        {
            switch ((string)comboTyp.SelectedValue)
            {
                case TypKodowania.DICT_LZW: UseLZWDecompress(); break;
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
                var stream = SaveFile();
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
                var stream = SaveFile();
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
    }
}
