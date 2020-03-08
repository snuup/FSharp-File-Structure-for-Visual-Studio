using System;
using System.IO;
using System.Threading;
using System.Windows;
using Snuup;

namespace LabApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            Loaded += this.MainWindow_Loaded;
        }

        Timer timer;

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var filename = @"m:/lab.fs";
            if (!File.Exists(filename)) MessageBox.Show("missing file: ....fs see source code in MainWindow.xaml.cs");
            var text = File.ReadAllText(filename);

            var astmgr = AstManager.Instance;
            astmgr.SetCodeFile(filename, text);
            Node root = astmgr.Model;
            root.IsRoot = true;
            Console.WriteLine(root.IsInterfaced);
            this.fsc.SetModel(root);

            this.timer = new Timer((state) => { this.fsc.Dispatcher.Invoke(new Action(() => { this.fsc.SelectNode(150); })); }, null, 1500, Timeout.Infinite);
        }
    }
}