using System.Reflection;
using System.Runtime.InteropServices;
using FSharp.Compiler;
using Microsoft.VisualStudio.Shell;

namespace Snuup
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("4c7302c1-14fd-4f25-a2cf-4e7b720ce0e4")]
    public class FileStructureToolWindow : ToolWindowPane
    {
        public FileStructureControl Control;

        // manages the model
        readonly AstManager astmgr;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStructureToolWindow"/> class.
        /// </summary>
        public FileStructureToolWindow() : base(null)
        {
            // very important! otherwise XAML will not load, reporting assembly load failure
            Assembly.Load("FSharpFileAst");

            this.astmgr = AstManager.Instance;
            this.Control = new FileStructureControl();
            this.Control.SetModel(this.astmgr.Model);
            this.Control.OnNavigateToLine += this.OnNavigateToLine;
            // astmgr.codeanalysisreport += OnCodeAnalysisReport;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = this.Control;
        }

        public void SetFilenameTitle(string filename) { this.Caption = "F# File Structure" + (filename == null ? "" : " -" + filename); }

        void OnNavigateToLine(object sender, Range.range r)
        {
            Snuup.Package.Instance.NavigateTo(this.astmgr.Filename, r.StartLine - 1, r.StartColumn, r.EndLine - 1, r.EndColumn);
            Snuup.Package.Instance.ShowFileStructureToolWindow();
        }

        void OnCodeAnalysisReport(object sender, string msg)
        {
            //Snuup.Package.Instance.WriteToOutputPane($"toolwindow is visible = {this.Control.IsVisible}");
            Snuup.Package.Instance.WriteToOutputPane(msg + "\n");
        }
    }
}