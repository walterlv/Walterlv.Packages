using System;
using System.Windows.Input;

namespace Walterlv.Windows.Input
{
    /// <summary>
    /// 表示一个必须提供参数才能执行的命令。
    /// </summary>
    public class ActionCommand<T> : ICommand
    {
        /// <summary>
        /// 创建 <see cref="ActionCommand"/> 的新实例，当 <see cref="ICommand"/> 被执行时，将调用参数传入的动作。
        /// </summary>
        public ActionCommand(Action<T> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// 用于接受所提供的参数并执行的委托。
        /// </summary>
        private readonly Action<T> _action;

        /// <summary>
        /// 使用指定的参数执行此命令。
        /// 框架中没有约定参数值是否允许为 null，这由参数定义时的泛型类型约定（C#8.0）或由命令的实现者约定。
        /// </summary>
        public void Execute(T t) => _action(t);

        void ICommand.Execute(object parameter) => Execute((T)parameter);

        bool ICommand.CanExecute(object parameter) => true;

        /// <inheritdoc />
        /// <summary>
        /// 当命令的可执行性改变时发生。
        /// </summary>
        public event EventHandler? CanExecuteChanged;
    }
}
