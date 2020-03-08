using System;
using System.Diagnostics;
using FSharpFileAst;

namespace Snuup
{
    class AstManager
    {
        string currentText;
        public string Filename;
        public Node Model;
        public EventHandler<string> codeanalysisreport;

        static AstManager instance;

        AstManager() { this.Model = new Node { Caption = "rooty", IsRoot = true }; }

        public static AstManager Instance => instance ?? (instance = new AstManager());

        public void SetCodeFile(string filename, string text)
        {
            if (this.currentText == text) return;
            this.currentText = text;
            if (filename.IsSet())
            {
                var sw = Stopwatch.StartNew();
                var an = TreeModel.getModel(filename, text);
                sw.Stop();
                this.codeanalysisreport?.Invoke(this, $"F# code analysis took {sw.ElapsedMilliseconds}ms");
                this.Model.Assign(an, true);
            }
            else
            {
                this.Model.Caption = "---";
                this.Model.Children.Clear();
            }
            this.Filename = filename;
        }

        public void Clear()
        {
            this.Model.Caption = "cleared @" + DateTime.Now; // not visible anyway
            this.Model.Children.Clear();
        }
    }
}