using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables;

namespace ToDoListMaterialDesign.ViewModels
{
    class MainWindowViewModel : BindableBase, IDisposable
    {
        #region view binding items

        public ReactivePropertySlim<string> WindowTitle { get; private set; }
        public ReadOnlyCollection<ScreenItem> ListBoxItemsSource { get; private set; }
        public ReactivePropertySlim<ScreenItem> ListBoxSelectedItem { get; private set; }

        #endregion

        #region クラス内変数・コンストラクタ

        public class ScreenItem
        {
            public string DisplayName { get; set; }
            public string ViewName { get; set; }
            public object Parameter { get; set; }
        }

        private CompositeDisposable _disposables_ = new CompositeDisposable();

        void IDisposable.Dispose() { this._disposables_.Dispose(); }

        private Prism.Regions.IRegionManager _regionManager_ = null;

        public MainWindowViewModel()
        {
            // nothing to do
        }

        public MainWindowViewModel(Prism.Regions.IRegionManager _rm)
        {
            this._regionManager_ = _rm;
            this.InitializeBindings();
        }

        #endregion

        #region イベント処理

        /// <summary>
        /// 左側メニュー選択時処理
        /// </summary>
        private void ListBox_SelectedItemChanged()
        {
            var selected = this.ListBoxSelectedItem.Value;
            if (selected == null) return;
            var selectedViewName = selected.ViewName;

            var param = new NavigationParameters
            {
                { "ViewName", selectedViewName },
                { "Parameter", selected.Parameter },
            };
            this._regionManager_.RequestNavigate("ContentRegion", selectedViewName, param);
        }

        #endregion

        #region 各種メソッド

        /// <summary>
        /// 左側リストボックスに表示する画面一覧を作成します
        /// </summary>
        private List<ScreenItem> CreateScreenItemList()
        {
            return new List<ScreenItem>
            {
                new ScreenItem { DisplayName = " Main", ViewName = "MainView" },
                new ScreenItem { DisplayName = " Edit", ViewName = "EditView" },
            };
        }

        #region ビューにバインドされている項目を初期化

        /// <summary>
        /// ビューにバインドされている項目を初期化します
        /// </summary>
        private void InitializeBindings()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            this.WindowTitle = new ReactivePropertySlim<string>().AddTo(this._disposables_);
            this.WindowTitle.Value = $"{ Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) } (ver.{ System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion })";

            this.ListBoxSelectedItem = new ReactivePropertySlim<ScreenItem>().AddTo(this._disposables_);
            this.ListBoxSelectedItem.Subscribe(_ => this.ListBox_SelectedItemChanged());
            this.ListBoxItemsSource = new ReadOnlyCollection<ScreenItem>(CreateScreenItemList());
        }

        #endregion

        #endregion

    }
}
