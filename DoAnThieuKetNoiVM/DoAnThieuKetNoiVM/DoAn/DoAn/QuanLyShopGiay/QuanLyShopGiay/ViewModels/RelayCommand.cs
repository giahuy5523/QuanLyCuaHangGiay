using System;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    /// <summary>
    /// Triển khai ICommand tái sử dụng cho mọi nút bấm / hành động trong ViewModel.
    /// Dùng: new RelayCommand(Execute) hoặc new RelayCommand(Execute, CanExecute).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // ── Constructor không có CanExecute ───────────────────────────────────
        public RelayCommand(Action<object> execute)
            : this(execute, null) { }

        // ── Constructor đầy đủ ────────────────────────────────────────────────
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ── ICommand.CanExecute ───────────────────────────────────────────────
        public bool CanExecute(object parameter)
            => _canExecute == null || _canExecute(parameter);

        // ── ICommand.Execute ──────────────────────────────────────────────────
        public void Execute(object parameter)
            => _execute(parameter);

        // ── Raise CanExecuteChanged ───────────────────────────────────────────
        /// <summary>Gọi khi muốn WPF đánh giá lại CanExecute (vd: bật/tắt nút).</summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>Bắt buộc WPF cập nhật trạng thái tất cả Command ngay lập tức.</summary>
        public static void RaiseCanExecuteChanged()
            => CommandManager.InvalidateRequerySuggested();
    }
}