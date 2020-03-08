using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop; //using System.ComponentModel.Composition;

namespace Snuup
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(FileStructureToolWindow))]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class Package : Microsoft.VisualStudio.Shell.Package, IVsRunningDocTableEvents
    {
        /// <summary>
        /// FileStructureTWPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "34bbfd9d-0b27-40a4-bf26-3d08bbb284fc";

        #region state

        public static Package Instance;
        uint rdtCookie;
        Timer timer;
        FileStructureToolWindow fileStructureTw;
        bool initial = false;

        #endregion

        #region life

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStructureToolWindow"/> class.
        /// </summary>
        public Package()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Command.Initialize(this);
            base.Initialize();

            // register events to react to document change
            var rdt = this.GetService<IVsRunningDocumentTable, SVsRunningDocumentTable>();
            rdt.AdviseRunningDocTableEvents(this, out this.rdtCookie);

            Instance = this;
        }

        #endregion

        #region core

        void CheckFileStructure(object state)
        {
            try
            {
                var doc = this.DTE.ActiveDocument; // he active document property throws an exception when the project Properties window is opened in Microsoft Visual Studio 2005. https://msdn.microsoft.com/en-us/library/envdte._dte.activedocument.aspx
                if (doc == null)
                {
                    this.timer.Dispose();
                    return;
                }

                if (this.initial || doc.ProjectItem.IsDirty)
                {
                    var td = (TextDocument) doc.Object("TextDocument");
                    if (td == null || td.Language != "F#")
                    {
                        this.timer.Dispose();
                        this.GetToolWindow().Control.Dispatcher.Invoke(() => AstManager.Instance.Clear());
                        this.GetToolWindow().SetFilenameTitle(null);
                        return;
                    }
                    if (this.GetToolWindow().Control.IsVisible)
                        // avoid costly f# syntax processing when window is not visible / used.
                    {
                        this.UpdateFileStructure(td, doc);
                    }
                }
                else
                {
                    Trace.WriteLine("skip check filestructure because doc is not dirty");
                }
                this.initial = false;

                // reactivate
                this.timer.Change(600, Timeout.Infinite);
            }
            catch
            {
            }
        }

        void UpdateFileStructure(TextDocument td, Document doc)
        {
            var start = td.CreateEditPoint(td.StartPoint);
            var text = start.GetText(td.EndPoint);
            var tw = this.GetToolWindow();
            tw.SetFilenameTitle(doc.Name);
            // modifying ObserviceCollection in a timer-pool-thread does not work
            tw.Control.Dispatcher.Invoke(() => AstManager.Instance.SetCodeFile(doc.FullName, text));
        }

        FileStructureToolWindow GetToolWindow() { return this.fileStructureTw ?? (this.fileStructureTw = this.GetToolWindow<FileStructureToolWindow>()); }

        public void ShowFileStructureToolWindow()
        {
            var z = FSharp.Compiler.Range.posOrder;
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = this.FindToolWindow(typeof(FileStructureToolWindow), 0, true);
            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create tool window");
            }
            var windowFrame = (IVsWindowFrame) window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #endregion

        #region util

        public T GetToolWindow<T>() where T : ToolWindowPane
        {
            T toolWindow = (T) this.FindToolWindow(typeof(T), 0, true);
            if (toolWindow?.Frame == null)
            {
                throw new COMException("Failed to create toolwindow.");
            }
            return toolWindow;
        }

        public DTE DTE
        {
            get
            {
                var dte = this.GetService<DTE, SDTE>();
                return dte;
            }
        }

        #endregion

        #region New region

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) { return 0; }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) { return 0; }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie) { return 0; }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs) { return 0; }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            this.timer?.Dispose();
            this.initial = true;

            // 200ms wait time are a throttling: when many docs are opened (upon start), the timers are created and disposed before firing
            // do a onetime callback and reschedule inside the callback
            this.timer = new Timer(this.CheckFileStructure, this, 200, Timeout.Infinite);
            return 0;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            RunningDocumentTable rdt = new RunningDocumentTable(this);
            RunningDocumentInfo doc = rdt.GetDocumentInfo(docCookie);

            return 0;
        }

        public void WriteToOutputPane(string s)
        {
            var op = this.GetOutputPane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "F# FileStructure");
            op.OutputString(s + "\n");
        }

        #endregion
    }
}