using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ToDoListMaterialDesign.Models.Codes;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;
using ToDoListMaterialDesign.Models.Interface;
using ToDoListMaterialDesign.Models.Logics;

namespace ToDoListMaterialDesign.ViewModels
{
    class MainViewModel : IDisposable, INavigationAware
    {
        #region view binding items

        public ObservableCollection<DataGridItem> DataGridItemsSource { get; private set; }
        public ReactivePropertySlim<DataGridItem> DataGridSelectedItem { get; private set; }

        public ReactiveCommand FinishClick { get; private set; }
        public ReactiveCommand EditClick { get; private set; }

        #endregion

        #region items for test

        public IDialogService TestDialogService { get; set; } = null;
        public IRequest TestRequest { get; set; } = null;
        public IResponse TestResponse { get; set; } = null;

        #endregion

        #region 画面遷移時の処理

        public string ThisViewName { get; private set; }

        /// <summary>Viewを表示した後呼び出されます。</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.ThisViewName = Convert.ToString(navigationContext.Parameters["ViewName"]);
            this.OnNavigatedTo(navigationContext.Parameters["Parameter"]);
        }

        /// <summary>表示するViewを判別します。</summary>
		/// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            string viewName = Convert.ToString(navigationContext.Parameters["ViewName"]);
            return this.ThisViewName == viewName;
        }

        /// <summary>別のViewに切り替わる前に呼び出されます。</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // nothing to do
        }

        #endregion

        #region クラス内変数・コンストラクタ

        private TodoTask EditingTodoTask { get; set; } = new TodoTask();

        public class DataGridItem
        {
            public TodoTask TodoTask { set; get; }
            public string TodoTaskStatusName { get => StatusCode.GetNameByCode(TodoTask.StatusCode); }
        }

        private IDialogService _dlgService_ = null;

        private CompositeDisposable _disposables_ = new CompositeDisposable();

        void IDisposable.Dispose() { this._disposables_.Dispose(); }

        public MainViewModel(IDialogService _dialogService)
        {
            this._dlgService_ = this.TestDialogService ?? _dialogService;
            this.InitializeBindings();
        }

        #endregion

        #region イベント処理

        /// <summary>
        /// 画面に遷移して来たタイミングで行う初期化処理
        /// </summary>
        private void OnNavigatedTo(object _parameter)
        {
            this.DisplaySearchResult();
        }

        /// <summary>
        /// データグリッド選択時処理
        /// </summary>
        private void DataGrid_SelectedItemChanged()
        {
            var entity = this.DataGridSelectedItem.Value?.TodoTask;
            this.EditingTodoTask = entity;
        }

        /// <summary>
        /// ボタン〔完了〕押下処理
        /// </summary>
        private void Finish_Click()
        {
            var entity = this.EditingTodoTask;
            entity.StatusCode = StatusCode.CODE_FINISHED;
            DalTodoTask.Update(entity);
            this.DisplaySearchResult();
        }

        /// <summary>
        /// ボタン〔編集〕押下処理
        /// </summary>
        private void Edit_Click()
        {
            var param = new DialogParameters { { "TodoTask", this.EditingTodoTask } };
            var retResult = ButtonResult.Cancel;
            IDialogParameters retParameters = null;
            this._dlgService_.ShowDialog("DialogWindowEditView", param, r => { retResult = r.Result; retParameters = r.Parameters; });
            if (retResult == ButtonResult.OK)
            {
                var entity = retParameters?.GetValue<TodoTask>("TodoTask");
                DalTodoTask.Update(entity);
            }
            this.DisplaySearchResult();
        }

        #endregion

        #region 各種メソッド

        /// <summary>
        /// データを検索して、画面に表示します
        /// </summary>
        private void DisplaySearchResult()
        {
            MainViewSearch logic = new MainViewSearch();
            IRequest request = this.TestRequest ?? new Request(); // logic for test
            IResponse response = this.TestResponse ?? logic.DoProcess(request); // logic for test

            this.DataGridItemsSource.Clear();

            if (response.IsSucceed == false) return;
            MainViewSearch.Result result = (MainViewSearch.Result)response.Result;

            foreach (var entity in result.TodoTasks) this.DataGridItemsSource.Add(new DataGridItem { TodoTask = entity });
        }

        #region ビューにバインドされている項目を初期化

        /// <summary>
        /// ビューにバインドされている項目を初期化します
        /// </summary>
        private void InitializeBindings()
        {
            this.DataGridItemsSource = new ObservableCollection<DataGridItem>();
            this.DataGridSelectedItem = new ReactivePropertySlim<DataGridItem>().AddTo(this._disposables_);

            this.DataGridSelectedItem.Subscribe(_ => this.DataGrid_SelectedItemChanged());

            this.FinishClick = new ReactiveCommand().WithSubscribe(this.Finish_Click);
            this.EditClick = new ReactiveCommand().WithSubscribe(this.Edit_Click);
        }

        #endregion

        #endregion


    }
}
