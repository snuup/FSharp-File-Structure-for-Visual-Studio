using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AstNode = FSharpFileAst.Node;

namespace Snuup
{
    public class Node : INotifyPropertyChanged
    {
        #region state

        string caption;
        readonly ObservableCollection<Node> children;
        AstNode astnode;
        bool isselected;
        bool isinterfaced;
        bool isroot;

        #endregion

        #region life

        public Node(IEnumerable<Node> cn = null) { this.children = new ObservableCollection<Node>(cn ?? Enumerable.Empty<Node>()); }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #region dp

        public string Caption
        {
            get { return this.caption; }
            set
            {
                if (value == this.caption) return;
                this.caption = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return this.isselected; }
            set
            {
                if (value == this.isselected) return;
                this.isselected = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsRoot
        {
            get { return this.isroot; }
            set
            {
                if (value == this.isroot) return;
                this.isroot = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Node> Children { get { return this.children; } }

        public bool IsInterfaced
        {
            get { return this.isinterfaced; }
            set
            {
                if (value == this.isinterfaced) return;
                this.isinterfaced = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region payload

        public AstNode Astnode
        {
            get { return this.astnode; }
            set
            {
                if (value == this.astnode) return;
                this.astnode = value;
                this.OnPropertyChanged();
            }
        }

        public void Assign(AstNode an, bool isroot)
        {
            // update caption
            this.Caption = an.Caption;
            this.Astnode = an;
            this.IsInterfaced = an.IsInterfaced;

            // remove excessive children
            while (this.Children.Count > an.ChildCount)
            {
                this.Children.RemoveAt(this.Children.Count - 1);
            }

            // ensure child this and update it recursively
            int i = 0;
            foreach (var ann in an.ChildrenEnum)
            {
                Node nn;
                if (i > this.Children.Count - 1)
                {
                    nn = new Node();
                    nn.IsRoot = isroot;
                    this.Children.Add(nn);
                }
                else
                {
                    nn = this.Children[i];
                }
                nn.Assign(ann, false);
                i++;
            }
        }

        #endregion

        #region diag

        public override string ToString() { return this.astnode.Range.ToString(); }

        #endregion
    }
}