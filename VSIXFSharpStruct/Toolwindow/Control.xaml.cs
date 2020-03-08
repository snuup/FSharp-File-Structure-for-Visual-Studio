using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FSharp.Compiler;
using AstNode = FSharpFileAst.Node;

namespace Snuup
{
    public partial class FileStructureControl : UserControl
    {
        // holds the line number
        public event EventHandler<Range.range> OnNavigateToLine;

        #region life

        public FileStructureControl()
        {
            var z = new NiceNameGenerator();
            var filename = z.GetType().Assembly.Location;
            var fi = new FileInfo(filename);
            var n = fi.Length;
            Console.WriteLine(n);

            this.InitializeComponent();
            Setter setter = new Setter(ScrollIntoViewBehavior.IsBroughtIntoViewWhenSelectedProperty, true);
            this.treeview.ItemContainerStyle.Setters.Add(setter);
        }


        public void SetModel(Node m)
        {
            //Debugger.Launch();
            foreach (var child in m.Children)
            {
                child.IsRoot = true;
            }
            m.IsRoot = true; // neaningless, we bind the children
            foreach (var c in m.Children)
            {
                c.IsRoot = true;
                Trace.Write("m.IsRoot = {0}", c.IsRoot.ToString());
            }
            this.treeview.ItemsSource = m.Children;
        }

        #endregion

        #region core

        void Navigate()
        {
            var sel = this.treeview.SelectedItem as Node;
            if (sel == null) return;
            //var p = new Pos() {Line = sel.Astnode.Range.StartLine,Column = sel.Astnode.Range.StartColumn };
            OnNavigateToLine?.Invoke(this, sel.Astnode.Range);
        }

        #endregion

        #region ui

        void OnTreeViewKeyDown(object sender, KeyEventArgs e) { this.Navigate(); }

        void OnTreeViewPreviewMouseDblClick(object sender, MouseButtonEventArgs e)
        {
            this.Navigate();
            e.Handled = true;
        }

        #endregion

        static Node FindNode(IEnumerable<Node> nodes, int line)
        {
            Node bestnode = null;
            foreach (var n in nodes)
            {
                var r = n.Astnode.Range;
                if (r.StartLine <= line)
                {
                    bestnode = n;
                }
                else
                {
                    break;
                }
            }
            if (bestnode == null) return null;
            //Trace.WriteLine($"match node {bestnode.Astnode.Range}");
            return FileStructureControl.FindNode(bestnode.Children, line) ?? bestnode;
        }

        public void SelectNode(int lineNumber)
        {
            try
            {
                var n = FileStructureControl.FindNode(this.treeview.ItemsSource as IEnumerable<Node>, lineNumber);
                if (n != null) n.IsSelected = true;

                var c = this.treeview.ItemContainerGenerator.ContainerFromItem(n);
            }
            catch
            {
            }
        }
    }
}