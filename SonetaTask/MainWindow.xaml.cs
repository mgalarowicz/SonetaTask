using System;
using System.Linq;
using System.Windows;
using LibGit2Sharp;
using Forms = System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Specialized;
using SonetaTask.Properties;


namespace SonetaTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region PROP_CHANGED
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            SavedPaths = Settings.Default.SavedPaths;

            DataContext = this;
        }


        # region PATH_TO_REPO
        private string _pathToRepo;
        public string PathToRepo
        {
            get { return _pathToRepo; }
            set
            {
                _pathToRepo = value;
                OnPropertyChanged(nameof(PathToRepo));
            }
        }
        #endregion


        #region SAVED_PATHS and HANDLE_SAVE_PATHS
        private StringCollection _savedPaths;
        public StringCollection SavedPaths
        {
            get { return _savedPaths; }
            set
            {
                _savedPaths = value;
                OnPropertyChanged(nameof(SavedPaths));
            }
        }

        private void HandleSavedPaths()
        {
            if (Settings.Default.SavedPaths.Contains(PathToRepo))
                return;

            Settings.Default.SavedPaths.Add(PathToRepo);
            Settings.Default.Save();
            SavedPaths = Settings.Default.SavedPaths;
        }
        #endregion


        #region CLICKS
        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Forms.FolderBrowserDialog();
            var result = folderDialog.ShowDialog();
            if (result == Forms.DialogResult.OK)
                PathToRepo = folderDialog.SelectedPath;
        }

        private void LoadClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FillGrid(PathToRepo);
                HandleSavedPaths();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load repository from given path");
            }

            ManuallyRefreshDataContext();
        }

        private void ManuallyRefreshDataContext()
        {
            DataContext = null;
            DataContext = this;
        }
        #endregion


        #region FILL_GRID  AND TASKS LOGIC (FILTERING/SORTING REPO DATA)
        public void FillGrid(string path)
        {
            using (var repo = new Repository(path))
            {
                var filter = new CommitFilter();

                var commitList = repo.Commits.QueryBy(filter);

                var first = commitList.First().Author.When.Date.DayOfYear;
                var last = commitList.Last().Author.When.Date.DayOfYear;
                float res = Math.Abs(first - last + 1);

                var enovaTask1 = commitList.GroupBy(x => new { x.Author.When.Date, x.Author.Name })
                                           .Select(g => new { Date = g.Key.Date.ToShortDateString(), Name = g.Key.Name, Commits = g.Count() });

                var enovaTask2 = commitList.GroupBy(x => x.Author.Name)
                                           .Select(g => new { Name = g.Key, CommitPerDay = (g.Count() / res) });

                grid1.ItemsSource = enovaTask1;
                grid2.ItemsSource = enovaTask2;
            }
        }
#endregion
    }
}
