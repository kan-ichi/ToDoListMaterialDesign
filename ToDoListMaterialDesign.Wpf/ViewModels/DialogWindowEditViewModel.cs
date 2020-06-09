using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ToDoListMaterialDesign.Models.Codes;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;

namespace ToDoListMaterialDesign.ViewModels
{
    class DialogWindowEditViewModel : BindableBase, IDisposable, IDialogAware
    {
        #region view binding items

        public SnackbarMessageQueue SnackBarMessageQueue { get; private set; }

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
        public ReactiveCommand CancelClick { get; private set; }
        public ReactiveCommand DeleteClick { get; private set; }
        public ReactiveCommand AddClick { get; private set; }

        #endregion

        #region IDialogAware インターフェイスメンバー

        /// <summary>ダイアログのCloseを要求するAction。</summary>
        public event Action<IDialogResult> RequestClose;

        /// <summary>ダイアログがClose可能かを取得します。</summary>
        /// <returns></returns>
        public bool CanCloseDialog() { return true; }

        /// <summary>ダイアログClose時のイベントハンドラ。</summary>
        public void OnDialogClosed() { }

        /// <summary>ダイアログOpen時のイベントハンドラ。</summary>
        /// <param name="_parameters">IDialogServiceに設定されたパラメータを表すIDialogParameters。</param>
        public void OnDialogOpened(IDialogParameters _parameters)
        {
            var entity = _parameters?.GetValue<TodoTask>("TodoTask");
            this.DisplayEntity(entity);
        }

        public string Title => "各項目を入力してください";

        #endregion

        #region クラス内変数・コンストラクタ

        private TodoTask EditingTodoTask { get; set; } = new TodoTask();

        private CompositeDisposable _disposables_ = new CompositeDisposable();

        void IDisposable.Dispose() { this._disposables_.Dispose(); }

        public DialogWindowEditViewModel()
        {
            this.InitializeBindings();
        }

        #endregion

        #region イベント処理

        /// <summary>
        /// ボタン〔更新〕押下処理
        /// </summary>
        private void Update_Click()
        {
            var param = new DialogParameters();
            param.Add("TodoTask", this.CollectScreenInformation(new TodoTask()));
            this.RequestClose?.Invoke(new DialogResult(ButtonResult.OK, param));
        }

        /// <summary>
        /// ボタン〔キャンセル〕押下処理
        /// </summary>
        private void Cancel_Click()
        {
            this.RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        /// <summary>
        /// ボタン〔削除〕押下処理
        /// </summary>
        private void Delete_Click()
        {
            DalTodoTask.Delete(this.TodoTaskId.Value);
            this.RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        /// <summary>
        /// ボタン〔追加〕押下処理
        /// </summary>
        private void Add_Click()
        {
            var addEntity = this.CollectScreenInformation(new TodoTask());
            int addCount = DalTodoTask.Add(addEntity);

            System.Media.SystemSounds.Asterisk.Play();
            this.SnackBarMessageQueue.Enqueue($"データを追加しました。");
        }

        #endregion

        #region 各種メソッド

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

            this.TodoTaskId = new ReactivePropertySlim<string>().AddTo(this._disposables_);

            this.DueDate = new ReactiveProperty<DateTime?>().AddTo(this._disposables_).SetValidateAttribute(() => this.DueDate);
            this.DueDateHourItemsSource = new ReadOnlyCollection<string>(new List<string> { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23" });
            this.DueDateHour = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.DueDateHour);
            this.DueDateMinuteItemsSource = new ReadOnlyCollection<string>(new List<string> { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59" });
            this.DueDateMinute = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.DueDateMinute);
            this.Status = new ReactiveProperty<bool>().AddTo(this._disposables_).SetValidateAttribute(() => this.Status);
            this.Subject = new ReactiveProperty<string>().AddTo(this._disposables_).SetValidateAttribute(() => this.Subject);

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

            this.CancelClick = new ReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Cancel_Click);

            this.DeleteClick = new ReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Delete_Click);

            this.AddClick = inputHasErrors
                .CombineLatestValuesAreAllFalse()
                .ToReactiveCommand().AddTo(this._disposables_)
                .WithSubscribe(this.Add_Click);
        }

        #endregion

        #endregion

    }
}
