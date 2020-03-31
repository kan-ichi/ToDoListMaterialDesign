using System.Windows;
using MahApps.Metro.Controls;
using Prism.Services.Dialogs;

namespace ToDoListMaterialDesign
{
    public partial class DialogWindow : MetroWindow, IDialogWindow
    {
        public IDialogResult Result { get; set; }

        /// <summary>WindowのLoadイベントハンドラ。</summary>
        /// <param name="sender">イベントのソース。</param>
        /// <param name="e">イベントデータを格納しているRoutedEventArgs。</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is IDialogAware)
                this.Title = (this.DataContext as IDialogAware).Title;

            this.Loaded -= this.Window_Loaded;
        }

        /// <summary>コンストラクタ。</summary>
        public DialogWindow()
        {
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            this.Loaded += this.Window_Loaded;
        }
    }
}