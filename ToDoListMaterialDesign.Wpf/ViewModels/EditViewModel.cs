using MaterialDesignThemes.Wpf;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ToDoListMaterialDesign.Models.Codes;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;
using ToDoListMaterialDesign.Models.Interface;
using ToDoListMaterialDesign.Models.Logics;

namespace ToDoListMaterialDesign.ViewModels
{
    class EditViewModel : IDisposable, INavigationAware
    {
        #region view binding items

        public SnackbarMessageQueue SnackBarMessageQueue { get; private set; }

        public ReactiveProperty<string> SearchConditionsText { get; private set; }
        public ReactiveProperty<DateTime?> SearchConditionsDateFrom { get; private set; }
        public ReactiveProperty<DateTime?> SearchConditionsDateTo { get; private set; }
        public ReactiveCommand SearchClick { get; private set; }
        public ReactiveCommand SearchConditionsClearClick { get; private set; }

        public ObservableCollection<DataGridItem> DataGridItemsSource { get; private set; }
        public ReactivePropertySlim<DataGridItem> DataGridSelectedItem { get; private set; }

        public ReactivePropertySlim<bool> IsDataGridAllSelected { get; private set; }
        public ReactiveCommand DeleteClick { get; private set; }

        public ReactivePropertySlim<string> TodoTaskId { get; private set; }
        [Required]
        public ReactiveProperty<DateTime?> DueDate { get; private set; }
        public ReadOnlyCollection<string> DueDateHourItemsSource { get; private set; }
        [Required]
        public ReactiveProperty<string> DueDateHour { get; private set; }
        public ReadOnlyCollection<string> DueDateMinuteItemsSource { get; private set; }
        [Required]
        public ReactiveProperty<string> DueDateMinute { get; private set; }
        public ReactiveProperty<bool> Status { get; private set; }
        [Required]
        public ReactiveProperty<string> Subject { get; private set; }

        public ReactiveCommand UpdateClick { get; private set; }
        public ReactiveCommand AddClick { get; private set; }

        #endregion

        #region items for test

        public IDialogResult TestDialogResult { get; set; } = null;
        public IDialogParameters TestDialogParameters { get; set; } = null;
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
            return false;
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
            public bool IsChecked { set; get; }
            public TodoTask TodoTask { set; get; }
            public string TodoTaskStatusName { get => StatusCode.GetNameByCode(TodoTask.StatusCode); }
        }

        private IDialogService _dlgService_ = null;

        private CompositeDisposable _disposables_ = new CompositeDisposable();

        void IDisposable.Dispose() { this._disposables_.Dispose(); }

        public EditViewModel(IDialogService _dialogService)
        {
            this._dlgService_ = _dialogService;
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
            this.DisplayEntity(new TodoTask());
        }

        /// <summary>
        /// ボタン〔検索〕押下処理
        /// </summary>
        private void Search_Click()
        {
            this.DisplaySearchResult();
            this.DisplayEntity(new TodoTask());
        }

        /// <summary>
        /// ボタン〔検索条件クリア〕押下処理
        /// </summary>
        private void SearchConditionsClear_Click()
        {
            this.SearchConditionsText.Value = string.Empty;
            this.SearchConditionsDateFrom.Value = null;
            this.SearchConditionsDateTo.Value = null;
        }

        /// <summary>
        /// ボタン〔選択した行を削除〕押下処理
        /// </summary>
        private void Delete_Click()
        {
            var delList = this.DataGridItemsSource.Where(w => w.IsChecked == true).ToList();

            if (delList.Count == 0)
            {
                System.Media.SystemSounds.Asterisk.Play();
                this.SnackBarMessageQueue.Enqueue($"チェックボックスで削除行を選択してください。");
                return;
            }

            foreach (var deleteEntity in delList.Select(s => s.TodoTask))
            {
                DalTodoTask.Delete(deleteEntity.TodoTaskId);
            }

            System.Media.SystemSounds.Asterisk.Play();
            this.DisplaySearchResult();
            this.DisplayEntity(new TodoTask());
            this.SnackBarMessageQueue.Enqueue($"データを削除しました。");
        }

        /// <summary>
        /// データグリッド選択時処理
        /// </summary>
        private void DataGrid_SelectedItemChanged()
        {
            var entity = this.DataGridSelectedItem.Value?.TodoTask;
            if (entity == null) return;
            this.DisplayEntity(entity);
        }

        /// <summary>
        /// ボタン〔更新〕押下処理
        /// </summary>
        private void Update_Click()
        {
            var updateEntity = this.CollectScreenInformation(new TodoTask());
            int updateCount = DalTodoTask.Update(updateEntity);

            System.Media.SystemSounds.Asterisk.Play();
            this.DisplaySearchResult();
            this.DisplayEntity(new TodoTask());
            this.SnackBarMessageQueue.Enqueue($"データを更新しました。");
        }

        /// <summary>
        /// ボタン〔追加〕押下処理
        /// </summary>
        private void Add_Click()
        {
            var addEntity = this.CollectScreenInformation(new TodoTask());
            int addCount = DalTodoTask.Add(addEntity);

            System.Media.SystemSounds.Asterisk.Play();
            this.DisplaySearchResult();
            this.DisplayEntity(new TodoTask());
            this.SnackBarMessageQueue.Enqueue($"データを追加しました。");
        }

        #endregion

        #region 各種メソッド

        /// <summary>
        /// データを検索して、画面に表示します
        /// </summary>
        private void DisplaySearchResult()
        {
            EditViewSearch.Parameter parameter = new EditViewSearch.Parameter
            {
                SearchConditionsText = this.SearchConditionsText.Value,
                SearchConditionsDateFrom = this.SearchConditionsDateFrom.Value,
                SearchConditionsDateTo = this.SearchConditionsDateTo.Value,
            };
            EditViewSearch logic = new EditViewSearch();
            IRequest request = this.TestRequest ?? new Request { Parameter = parameter }; // logic for test
            IResponse response = this.TestResponse ?? logic.DoProcess(request); // logic for test

            this.DataGridItemsSource.Clear();

            if (response.IsSucceed == false) return;
            EditViewSearch.Result result = (EditViewSearch.Result)response.Result;

            foreach (var entity in result.TodoTasks) this.DataGridItemsSource.Add(new DataGridItem { TodoTask = entity });
        }

        /// <summary>
        /// エンティティの内容を画面に表示します
        /// </summary>
        private void DisplayEntity(TodoTask _entity)
        {
            this.EditingTodoTask.CopyValuesFrom(_entity);

            this.TodoTaskId.Value = _entity.TodoTaskId;
            this.DueDate.Value = _entity.DueDate;
            this.DueDateHour.Value = _entity.DueDate?.Hour.ToString("00");
            this.DueDateMinute.Value = _entity.DueDate?.Minute.ToString("00");
            this.Status.Value = new StatusCode(_entity.StatusCode).IsFinished;
            this.Subject.Value = _entity.Subject;
        }

        /// <summary>
        /// 画面の情報を集めて、エンティティにセットします
        /// </summary>
        private TodoTask CollectScreenInformation(TodoTask _entity)
        {
            _entity.CopyValuesFrom(this.EditingTodoTask);

            _entity.TodoTaskId = this.TodoTaskId.Value;
            _entity.DueDate = this.DueDate.Value?.Date.AddHours(Convert.ToInt32(this.DueDateHour.Value)).AddMinutes(Convert.ToInt32(this.DueDateMinute.Value));
            _entity.StatusCode = this.Status.Value ? StatusCode.CODE_FINISHED : StatusCode.CODE_NOT_YET;
            _entity.Subject = this.Subject.Value;

            return _entity;
        }

        #region ビューにバインドされている項目を初期化

        /// <summary>
        /// ビューにバインドされている項目を初期化します
        /// </summary>
        private void InitializeBindings()
        {
            this.SnackBarMessageQueue = new SnackbarMessageQueue();

            this.SearchConditionsText = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.SearchConditionsText);
            this.SearchConditionsDateFrom = new ReactiveProperty<DateTime?>().AddTo(this._disposables_).SetValidateAttribute(() => this.SearchConditionsDateFrom);
            this.SearchConditionsDateTo = new ReactiveProperty<DateTime?>().AddTo(this._disposables_).SetValidateAttribute(() => this.SearchConditionsDateTo);

            this.DataGridItemsSource = new ObservableCollection<DataGridItem>();
            this.DataGridSelectedItem = new ReactivePropertySlim<DataGridItem>().AddTo(this._disposables_);
            this.TodoTaskId = new ReactivePropertySlim<string>().AddTo(this._disposables_);
            this.IsDataGridAllSelected = new ReactivePropertySlim<bool>().AddTo(this._disposables_);

            this.DueDate = new ReactiveProperty<DateTime?>().AddTo(this._disposables_).SetValidateAttribute(() => this.DueDate);
            this.DueDateHourItemsSource = new ReadOnlyCollection<string>(Enumerable.Range(0, 24).Select(s => s.ToString("00")).ToList());
            this.DueDateHour = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.DueDateHour);
            this.DueDateMinuteItemsSource = new ReadOnlyCollection<string>(Enumerable.Range(0, 60).Select(s => s.ToString("00")).ToList());
            this.DueDateMinute = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.DueDateMinute);
            this.Status = new ReactiveProperty<bool>().AddTo(this._disposables_).SetValidateAttribute(() => this.Status);
            this.Subject = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.Subject);

            this.SearchClick = new ReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Search_Click);

            this.SearchConditionsClearClick = new[]
            {
                this.SearchConditionsText.Select(s => !string.IsNullOrWhiteSpace(s)),
                this.SearchConditionsDateFrom.Select(s => s != null),
                this.SearchConditionsDateTo.Select(s => s != null),
            }
            .CombineLatest(x => x.Any(y => y))
            .ToReactiveCommand().AddTo(this._disposables_)
            .WithSubscribe(this.SearchConditionsClear_Click);

            this.DeleteClick = new ReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Delete_Click);

            this.DataGridSelectedItem.Subscribe(_ => this.DataGrid_SelectedItemChanged());

            var inputHasErrors = new[]
            {
                this.DueDate.ObserveHasErrors,
                this.DueDateHour.ObserveHasErrors,
                this.DueDateMinute.ObserveHasErrors,
                this.Status.ObserveHasErrors,
                this.Subject.ObserveHasErrors,
            };

            this.UpdateClick = inputHasErrors
                .CombineLatestValuesAreAllFalse()
                .CombineLatest(this.TodoTaskId, (x, y) => x & (!string.IsNullOrWhiteSpace(y)))
                .ToReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Update_Click);

            this.AddClick = inputHasErrors
                .CombineLatestValuesAreAllFalse()
                .ToReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Add_Click);
        }

        #endregion

        #endregion

    }
}
