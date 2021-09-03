using Walterlv.ComponentModel;

namespace Walterlv.Windows.Navigating
{
    public class NavigationView<TView> : BindableObject, INavigationView where TView : new()
    {
        public NavigationView()
        {
        }
    }
}
