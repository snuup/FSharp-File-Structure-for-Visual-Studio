using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Snuup
{
    static class Extensions
    {
        //public static string FileExtension(this string filename)
        //{
        //    return Path.GetExtension(filename);
        //}

        public static bool IsSet(this string s) { return !string.IsNullOrWhiteSpace(s); }

        public static T GetService<T, S>(this IServiceProvider sp) { return (T) sp.GetService(typeof(S)); }

        static Guid guidTextView = new Guid(LogicalViewID.TextView);

        /// Go to exact location in a given file.
        public static void NavigateTo(this IServiceProvider serviceProvider, string fileName, int startRow, int startCol, int endRow, int endCol)
        {
            IVsUIHierarchy hierarchy;
            UInt32 itemId;
            IVsWindowFrame windowFrame;
            var isOpened = VsShellUtilities.IsDocumentOpen(
                serviceProvider,
                fileName,
                guidTextView,
                out hierarchy,
                out itemId,
                out windowFrame);

            if (isOpened)
            {
                //var hr = windowFrame.Show();
                //if(hr != S_OK) throw new COMException("windowframe ", hr);

                var vsTextView = VsShellUtilities.GetTextView(windowFrame);
                var vsTextManager = serviceProvider.GetService<IVsTextManager, SVsTextManager>();

                IVsTextLines vsTextLines;
                var hr = vsTextView.GetBuffer(out vsTextLines);
                if (hr != VSConstants.S_OK) throw new COMException("get text lines failed", hr);

                hr = vsTextManager.NavigateToLineAndColumn(vsTextLines, ref guidTextView, startRow, startCol, endRow, endCol);
                if (hr != VSConstants.S_OK) throw new COMException("navigate to failed", hr);
            }
        }
    }
}