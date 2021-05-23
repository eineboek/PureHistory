using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;

namespace MOSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog FilePrompt;
        private MOReader moReader;
        private List<MOGridData> DataList;
        private List<int> SearchResults;
        private int currentSearchResult = 0;

        private enum SearchMode
        {
            IDS = 0,
            Content = 1,
            Both = 2
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            FilePrompt = new();
            FilePrompt.Title = "Open File";
            FilePrompt.DefaultExt = ".mo";
            FilePrompt.Filter = "MO Files (*.mo) | *.mo";

            bool? dialogResult = FilePrompt.ShowDialog();

            if (dialogResult == true)
            {
                moReader = new(FilePrompt.FileName);
                MOGrid.Items.Clear();
                MOGrid.ItemsSource = null;
                SearchResults = new();
                currentSearchResult = 0;

                DataList = new();

                for (int i = 0; i < moReader.Count; i++)
                {
                    MOLine line = moReader[i];
                    if (line.Index != 0)
                    {
                        DataList.Add(new MOGridData { index = line.Index, identifier = line.Original, content = line.Translated });
                    }
                }
                MOGrid.ItemsSource = DataList;
            }
        }

        private void UnloadData(object sender, RoutedEventArgs e)
        {
            MOGrid.ItemsSource = null;
            DataList = new();
            SearchResults = new();
            currentSearchResult = 0;
            if (moReader != null)
            {
                moReader.Dispose();
            }
        }

        private void DoSearch(object sender, RoutedEventArgs e)
        {
            if (MOGrid.ItemsSource == null)
            {
                MessageBox.Show("There is no data to search in. Please open a file first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (SearchModeCombo.SelectedIndex == -1)
                {
                    MessageBox.Show("There is no search mode selected.", "Search Mode", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    SearchMode searchMode = (SearchMode)SearchModeCombo.SelectedIndex;
                    if (string.IsNullOrWhiteSpace(SearchBox.Text))
                    {
                        MessageBox.Show("The search can't be empty.", "Search", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        string search = SearchBox.Text;
                        SearchResults = new();
                        currentSearchResult = 0;

                        for (int i = 0; i < DataList.Count; i++)
                        {
                            MOGridData data = DataList[i];

                            if (searchMode == SearchMode.IDS || searchMode == SearchMode.Both)
                            {
                                if (data.identifier.Contains(search))
                                {
                                    SearchResults.Add(i);
                                }
                            }
                            if (searchMode == SearchMode.Content || searchMode == SearchMode.Both)
                            {
                                if (data.content.Contains(search))
                                {
                                    SearchResults.Add(i);
                                }
                            }
                        }
                    }

                    if (SearchResults.Count == 0)
                    {
                        MessageBox.Show("No results matching the search have been found.", "Search", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MOGrid.ScrollIntoView(MOGrid.Items[SearchResults[currentSearchResult]]);
                        MOGrid.UpdateLayout();
                        SearchResultsLabel.Text = $"{currentSearchResult + 1}/{SearchResults.Count}";
                    }
                }
            }
        }

        private void PreviousResult(object sender, RoutedEventArgs e)
        {
            currentSearchResult--;
            if (currentSearchResult == -1)
            {
                currentSearchResult = SearchResults.Count - 1;
            }
            MOGrid.UpdateLayout();
            MOGrid.ScrollIntoView(MOGrid.Items[SearchResults[currentSearchResult]]);
            //TODO: Select Search Result in View
            SearchResultsLabel.Text = $"{currentSearchResult + 1}/{SearchResults.Count}";
        }

        private void NextResult(object sender, RoutedEventArgs e)
        {
            currentSearchResult++;
            if (currentSearchResult == SearchResults.Count)
            {
                currentSearchResult = 0;
            }
            MOGrid.UpdateLayout();
            MOGrid.ScrollIntoView(MOGrid.Items[SearchResults[currentSearchResult]]);
            //TODO: Select Search Result in View
            SearchResultsLabel.Text = $"{currentSearchResult + 1}/{SearchResults.Count}";
        }
    }
}