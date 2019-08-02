using System;
using System.Windows;
using Walterlv.ComponentModel;

namespace Walterlv.Windows.Navigating
{
    public class NavigationItem : BindableObject
    {
        private readonly Func<UIElement> _viewCreator;
        private readonly Func<object> _viewModelCreator;
        private UIElement _view;
        private object _viewModel;

        public NavigationItem(Func<UIElement> viewCreator, Func<object> viewModelCreator,
            string title = null)
        {
            _viewCreator = viewCreator ?? throw new ArgumentNullException(nameof(viewCreator));
            _viewModelCreator = viewModelCreator ?? throw new ArgumentNullException(nameof(viewModelCreator));
            Title = title;
        }

        public UIElement View => _view ?? (_view = _viewCreator());

        public object ViewModel => _viewModel ?? (_viewModel = _viewModelCreator());

        public string Title { get; }

        public static NavigationItem Combine<TView, TViewModel>(string title = null)
            where TView : UIElement, new()
            where TViewModel : class, new()
            => new NavigationItem<TView, TViewModel>(() => new TView(), () => new TViewModel(), title);
    }

    public class NavigationItem<TView, TViewModel> : NavigationItem
        where TView : UIElement, new()
        where TViewModel : class, new()
    {
        public NavigationItem(Func<TView> viewCreator, Func<TViewModel> viewModelCreator,
            string title = null)
            : base(viewCreator, viewModelCreator, title)
        {
        }

        public new TView View => (TView)base.View;

        public new TViewModel ViewModel => (TViewModel)base.ViewModel;

        public static NavigationItem Combine(string title = null)
            => new NavigationItem<TView, TViewModel>(() => new TView(), () => new TViewModel(), title);
    }
}
