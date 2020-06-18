using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ToDoListMaterialDesign.ViewModels.EditViewModel;

namespace ToDoListMaterialDesign.Views
{
    /// <summary>
    /// EditView.xaml の相互作用ロジック
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public partial class EditView : UserControl
    {
        public EditView()
        {
            InitializeComponent();

            // DatePicker の表示を日本語にする
            this.SearchConditionsDateFrom.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
            this.SearchConditionsDateTo.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
            this.DueDate.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        #region 全選択チェックボックスに関するロジック

        private bool _isCancelCheckBoxAllUnchecked_;

        private void CheckBoxAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (DataGridItem item in this.DataGrid.ItemsSource) item.IsChecked = true;
            this.DataGrid.Items.Refresh();
        }

        private void CheckBoxAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this._isCancelCheckBoxAllUnchecked_)
            {
                this._isCancelCheckBoxAllUnchecked_ = false;
                return;
            }

            foreach (DataGridItem item in this.DataGrid.ItemsSource) item.IsChecked = false;
            this.DataGrid.Items.Refresh();
        }

        private void CheckBoxEach_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.CheckBoxAll.IsChecked == false) return;
            this._isCancelCheckBoxAllUnchecked_ = true;
            this.CheckBoxAll.IsChecked = false;
        }

        #endregion

    }
}
