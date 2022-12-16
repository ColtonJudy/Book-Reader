using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Book_Reader
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        public string bookTitle = "";
        public string bookAuthor = "";
        public string bookURL = "";
        public string bookPath = "";

        public ImportWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                bookPathTextbox.Text = openFileDialog.FileName;
            }
        }

        private void Import(object sender, RoutedEventArgs e)
        {
            bookTitle = bookTitleTextbox.Text;
            bookAuthor = bookAuthorTextbox.Text;
            bookURL = bookURLTextbox.Text;
            bookPath = bookPathTextbox.Text;

            //ADD ERROR HANDLING
            DialogResult = true;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
