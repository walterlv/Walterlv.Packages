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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Walterlv.Windows.Effects;
using Walterlv.Windows.Interop;
using Walterlv.Windows.Sample.Views;

namespace Walterlv.Windows.Sample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            //var blur = new WindowAccentCompositor(this);
            //blur.Color = Color.FromArgb(0x3f, 0x18, 0xa0, 0x5e);
            //blur.IsEnabled = true;

            var childWindow = new Window
            {
                Content = new FluentPage(),
            };
            var handle = new WindowInteropHelper(childWindow).EnsureHandle();
            var wr = new WindowWrapper(handle);
            childWindow.Show();
            TestLayer.Child = wr;
        }
    }
}
