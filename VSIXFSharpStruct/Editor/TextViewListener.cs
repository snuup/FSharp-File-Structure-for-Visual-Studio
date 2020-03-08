using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Snuup
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("F#")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    public class FSharpTextViewCreationListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView) { textView.Caret.PositionChanged += this.Caret_PositionChanged; }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            try
            {
                var tw = Package.Instance.GetToolWindow<FileStructureToolWindow>();
                if (tw != null && tw.Control.IsVisible)
                {
                    var line = e.NewPosition.BufferPosition.GetContainingLine();
                    tw.Control.SelectNode(line.LineNumber + 1);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}