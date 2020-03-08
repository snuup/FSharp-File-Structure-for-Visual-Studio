using System;
using System.Windows;
using System.Windows.Controls;

namespace Snuup
{
    public static class ScrollIntoViewBehavior
    {
        #region IsBroughtIntoViewWhenSelected

        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
                "IsBroughtIntoViewWhenSelected",
                typeof(bool),
                typeof(ScrollIntoViewBehavior),
                new UIPropertyMetadata(false, ScrollIntoViewBehavior.OnIsBroughtIntoViewWhenSelectedChanged));

        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem) { return (bool) treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty); }

        public static void SetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem, bool value) { treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value); }

        static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool) e.NewValue)
                item.Selected += ScrollIntoViewBehavior.OnTreeViewItemSelected;
            else
                item.Selected -= ScrollIntoViewBehavior.OnTreeViewItemSelected;
        }

        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if (!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}