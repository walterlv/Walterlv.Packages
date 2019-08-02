using System.Collections.ObjectModel;
using Walterlv.ComponentModel;
using Walterlv.Windows.Navigating;
using Walterlv.Windows.Sample.ViewModels;
using Walterlv.Windows.Sample.Views;

namespace Walterlv.Windows.Sample
{
    public class MainViewModel : BindableObject
    {
        public ObservableCollection<NavigationItem> PageItems { get; } = new ObservableCollection<NavigationItem>
        {
            NavigationItem.Combine<HomePage, HomeViewModel>("主页"),
            NavigationItem.Combine<FluentPage, FluentViewModel>("Fluent 主题"),
        };
    }
}
