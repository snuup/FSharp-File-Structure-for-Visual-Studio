using System;
using System.Windows;
using System.Windows.Controls;
using FSharpFileAst;

namespace Snuup
{
    public class ImageTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try
            {
                var element = container as FrameworkElement;
                var type = (SynType) item;
                return element?.TryFindResource(type.ToString()) as DataTemplate;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}