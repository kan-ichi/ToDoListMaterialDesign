using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace ToDoListMaterialDesign.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (_, __) =>
            {
                this.MenuListBox.SelectedIndex = 0;
            };
        }

        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            this.MaterialDesignDrawerHost.IsLeftDrawerOpen = false;
        }
    }
}
