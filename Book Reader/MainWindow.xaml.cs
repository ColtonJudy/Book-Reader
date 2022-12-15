//Colton Judy
//Book Reader - Prototype application for viewing public domain books

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Book_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeBookReader();
        }

        List<string> bookTitles = new();
        List<string> bookAuthors = new();
        List<string> bookFileNames = new();
        List<string> bookURLs = new();

        string[]? selectedBookContents;
        List<string> pages = new();
        int currentPageNumber = 0;

        //initializes the book reader by loading all necessary data
        public void InitializeBookReader()
        {
            LoadBookData();

            LoadTitles(bookTitles);
            LoadAuthors();  
            LoadFontFamilies();
            LoadFontSizes();
            LoadFontWeights();
            LoadLinesPerPage();
        }

        //loads data from BOOK_INDEX.txt
        public void LoadBookData()
        {
            string fileContents = LoadFile("BOOK_INDEX.txt");

            //seperate fileContents string by title, author, fileName, and URL
            StringReader stringReader = new(fileContents);
            string? line;
            int lineNum = 0;

            while((line = stringReader.ReadLine()) != null) {
                if (lineNum % 4 == 0)
                    bookTitles.Add(line);
                else if (lineNum % 4 == 1)
                {
                    bookAuthors.Add(line);
                }
                else if (lineNum % 4 == 2)
                    bookFileNames.Add(line);
                else if (lineNum % 4 == 3)
                    bookURLs.Add(line);

                lineNum++;
            }
        }

        //loads book titles into listView
        public void LoadTitles(List<string> bookTitles)
        {
            List<string> sortedBookTitles = new(bookTitles);
            sortedBookTitles.Sort();
            titlesListView.ItemsSource = sortedBookTitles;
        }

        //loads book authors into listview
        public void LoadAuthors()
        {
            List<string> sortedBookAuthors = new();

            //remove duplicates
            foreach(string author in bookAuthors)
            {
                if (!sortedBookAuthors.Contains(author))
                    sortedBookAuthors.Add(author);
            }

            sortedBookAuthors.Sort();
            sortedBookAuthors.Insert(0, "All");
            authorsListView.ItemsSource = sortedBookAuthors;

            authorsListView.SelectedIndex = 0;
        }

        //loads font families into listview
        public void LoadFontFamilies()
        {
            List<FontFamily> fontFamilies = new(Fonts.SystemFontFamilies);

            fontFamilies.Sort(new FontFamilyComparer());

            fontFamilyListView.ItemsSource = fontFamilies;

            fontFamilyListView.SelectedIndex = fontFamilies.IndexOf(pageTextBlock.FontFamily);
        }

        //loads font sizes into listview
        public void LoadFontSizes()
        {
            List<int> fontSizes = new();

            for (int i = 12; i <= 40; i+=2)
            {
                fontSizes.Add(i);
            }

            fontSizeListView.ItemsSource = fontSizes;

            fontSizeListView.SelectedIndex = fontSizes.IndexOf(20);
        }

        //loads font weights into listview
        public void LoadFontWeights()
        {
            List<string> fontWeights = new() { "Thin", "Normal", "Bold" };
            fontWeightListView.ItemsSource = fontWeights;

            fontWeightListView.SelectedIndex = fontWeights.IndexOf(pageTextBlock.FontWeight.ToString());
        }

        //loads lines per page into listview
        public void LoadLinesPerPage()
        {
            List<int> linesPerPage = new();

            for (int i = 10; i <= 50; i += 10)
            {
                linesPerPage.Add(i);
            }

            linesPerPageListView.ItemsSource = linesPerPage;

            linesPerPageListView.SelectedIndex = linesPerPage.IndexOf(20);
        }

        public void ImportNewBook(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Import", "Import");
        }

        //resets the screen
        public void ResetScreen(object sender, RoutedEventArgs e)
        {
            //reset and clear all loaded content
            bookTitles = new List<string>();
            bookAuthors = new List<string>();
            bookFileNames = new List<string>();
            bookURLs = new List<string>();

            titlesListView.SelectedIndex = -1;
            if (selectedBookContents != null)
                Array.Clear(selectedBookContents);
            pages = new List<string>();
            currentPageNumber = 0;
            pageTextBlock.Text = "";

            pageTextBlock.FontFamily = new FontFamily("Segoe UI");
            pageTextBlock.FontSize = 12;
            pageTextBlock.FontWeight = FontWeights.Normal;

            InitializeBookReader();
        }

        //loads the book in the user's default browser
        public void LoadBookInBrowser(object sender, RoutedEventArgs e)
        {
            if(titlesListView.SelectedItem == null)
            {
                MessageBox.Show("No Book Selected", "Error");
                return;
            }

            string currentBook = titlesListView.SelectedItem.ToString() ?? "";
            int URLindex = bookTitles.IndexOf(currentBook);
            string URL = bookURLs[URLindex];

            Process.Start(new ProcessStartInfo
            {
                FileName = URL,
                UseShellExecute = true
            });
        }

        //exits the program
        public void ExitProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //toggles dark mode
        public void ToggleDarkMode(object sender, RoutedEventArgs e)
        {
            if (darkModeToggle.IsChecked)
                MessageBox.Show("Dark Mode is now on", "Dark Mode");
            else
                MessageBox.Show("Dark Mode is now off", "Dark Mode");
        }

        //shows the about page
        public void About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Project developed by Colton Judy", "About");
        }

        //Gets content of selected book
        private void TitlesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //remove existing content
            if(selectedBookContents != null)
                Array.Clear(selectedBookContents);

            string rawFileContents;
            if (titlesListView.SelectedItem == null || titlesListView.SelectedItem.ToString == null)
                return;
            else
            {
                string fileName = bookFileNames[bookTitles.IndexOf(titlesListView.SelectedItem.ToString() ?? "")];

                rawFileContents = LoadFile(fileName);
                selectedBookContents = rawFileContents.Split("\n");

            }

            LoadPages();

            DisplayPage(0);
        }

        //loads the file and returns the contents as a string
        public static string LoadFile(string fileName)
        {
            string fileContents;
            var assembly = Assembly.GetExecutingAssembly();
            var fileNames = assembly.GetManifestResourceNames();
            var resourceName = fileNames.Single(str => str.EndsWith(fileName));
            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new(stream))
                    {
                        fileContents = reader.ReadToEnd();
                    }
                }
                else
                {
                    fileContents = "error opening file";
                }
            }

            return fileContents;
        }

        //converts the book contents into an array of pages based on linesPerPage
        public void LoadPages()
        {
            if (pages != null && selectedBookContents != null)
            {
                //remove existing content
                pages = new List<string>();
                currentPageNumber = 0;

                int currentLine = 0;
                while (currentLine < selectedBookContents.Length)
                {
                    string page = "";

                    for (int i = 0; i < Convert.ToInt32(linesPerPageListView.SelectedItem) && currentLine < selectedBookContents.Length; i++)
                    {
                        page += (selectedBookContents[currentLine] + "\n");
                        currentLine++;
                    }

                    pages.Add(page);
                }
            }
        }

        //displays the page based on a page number
        public void DisplayPage(int pageNumber)
        {
            pageTextBlock.Text = pages[pageNumber];

            pageCountLabel.Content = $"Page {currentPageNumber+1} of {pages.Count}";
        }

        //filters book titles by author selection
        private void AuthorsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if the user has selected ALL
            if (authorsListView.SelectedIndex == 0)
            {
                LoadTitles(bookTitles);
            }
            else
            {
                //clear loaded content
                titlesListView.SelectedIndex = -1;
                if(selectedBookContents != null)
                    Array.Clear(selectedBookContents);
                pages = new List<string>();
                currentPageNumber = 0;
                pageTextBlock.Text = "";


                //display filtered bookTitles based on selected author
                List<string> filteredBookTitles = new();

                for(int i = 0; i < bookTitles.Count; i++)
                {
                    if (bookAuthors[i] == authorsListView.SelectedItem.ToString())
                    {
                        filteredBookTitles.Add(bookTitles[i]);
                    }
                }

                LoadTitles(filteredBookTitles);
            }
        }

        //loads the selected font family
        private void FontFamilyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageTextBlock.FontFamily = new FontFamily(fontFamilyListView.SelectedItem.ToString());
        }

        //loads the selected font size
        private void FontSizeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageTextBlock.FontSize = Convert.ToDouble(fontSizeListView.SelectedItem);
        }

        //loads the selected font weight
        private void FontWeightListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(fontWeightListView.SelectedItem.ToString())
            {
                case "Thin":
                    pageTextBlock.FontWeight = FontWeights.Thin;
                    break;
                case "Normal":
                    pageTextBlock.FontWeight = FontWeights.Normal;
                    break;
                case "Bold":
                    pageTextBlock.FontWeight = FontWeights.Bold;
                    break;
                default:
                    pageTextBlock.FontWeight = FontWeights.Normal;
                    break;
            }
        }

        //loads the select number of lines per page
        private void LinesPerPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(selectedBookContents != null)
            {
                LoadPages();

                DisplayPage(0);
                currentPageNumber = 0;
            }
        }

        //goes back a page
        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (pages != null && currentPageNumber > 0)
            {
                DisplayPage(--currentPageNumber);
            }
        }

        //goes to the first page
        private void CurrentPage_Click(object sender, RoutedEventArgs e)
        {
            if(pages != null && pages.Count > 0)
            {
                DisplayPage(0);
                currentPageNumber = 0;
            }
        }

        //goes forward a page
        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (pages != null && currentPageNumber < pages.Count - 1)
            {
                DisplayPage(++currentPageNumber);
            }
        }
    }

    //compares font families
    public class FontFamilyComparer : IComparer<FontFamily>
    {
        //allows the user to compare font family names alphabetically
        public int Compare(FontFamily? x, FontFamily? y)
        {
            if (x != null && y != null)
                return x.ToString().CompareTo(y.ToString());
            else
                return 0;
        }
    }
}
