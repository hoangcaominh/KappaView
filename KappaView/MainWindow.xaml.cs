using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace KappaView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OpenFileDialog BrowseProfile;
        private ResourceReader reader;
        private string path;

        public MainWindow()
        {
            InitializeComponent();

            path = "";
            // BrowseProfile
            BrowseProfile = new OpenFileDialog();
            string ProfileFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "profiles");
            if (!Directory.Exists(ProfileFolderPath))
                Directory.CreateDirectory(ProfileFolderPath);
            BrowseProfile.InitialDirectory = ProfileFolderPath;
            BrowseProfile.DefaultExt = "json";
            BrowseProfile.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
        }

        private async void Select(object sender, RoutedEventArgs e)
        {
            if (BrowseProfile.ShowDialog() == true)
            {
                ProfileName.Text = "Validating file...";
                TextVersion.Text = TextTarget.Text = TextPlatform.Text = "";
                path = BrowseProfile.FileName;

                await Task.Run(() => reader = new ResourceReader(path));
                if (reader.IsOkay())
                {
                    ProfileName.Text = BrowseProfile.SafeFileName;
                    TextVersion.Text = reader.Version.ToString();
                    TextTarget.Text = reader.Target;
                    TextPlatform.Text = reader.Platform.ToString();
                }
                else
                {
                    ProfileName.Text = "";
                }
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (reader == null || !reader.IsOkay())
            {
                MessageBox.Show("No profile is selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Status form = new Status(reader);
            form.ShowDialog();
        }
    }
}
