using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace KappaView
{
    /// <summary>
    /// Interaction logic for Status.xaml
    /// </summary>
    public partial class Status : Window
    {
        private readonly ResourceReader reader;
        private List<Tuple<string, object>> data;

        public Status(ResourceReader reader)
        {
            InitializeComponent();

            this.reader = reader;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            info.Text = "Looking for " + reader.Target + "...";
            Update();
        }

        private async void Update()
        {
            while (!reader.GetProcess())
                await Task.Delay(50);

            bool exit = false;

            while (reader.GetProcess())
            {
                await Task.Run(() =>
                {
                    data = reader.Read();
                    if (data == null)
                    {
                        exit = true;
                        return;
                    }
                });
                await Task.Delay(50);

                if (exit)
                    break;

                info.Clear();
                foreach (var i in data)
                {
                    info.Text += i.Item1 + ": " + i.Item2.ToString() + '\n';
                }
            }

            info.Text += "\n" + reader.Target + " terminated, stopped reading.";
        }
    }
}
