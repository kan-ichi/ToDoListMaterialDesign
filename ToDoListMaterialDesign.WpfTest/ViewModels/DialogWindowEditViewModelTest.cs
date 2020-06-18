using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using ToDoListMaterialDesign.CoreTest;
using ToDoListMaterialDesign.Models.Codes;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Interface;
using ToDoListMaterialDesign.Models.Logics;
using ToDoListMaterialDesign.ViewModels;

namespace ToDoListMaterialDesign.WpfTest.ViewModels
{
    /// <summary>
    /// 「DialogWindowEdit」画面のテスト
    /// </summary>
    [TestClass]
    public class DialogWindowEditViewModelTest
    {
        #region テスト用定義・変数

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        private class DummyDialogService : IDialogService
        {
            public void Show(string _name, IDialogParameters _parameters, Action<IDialogResult> _callback) { _callback.Invoke(new DialogResult()); }
            public void ShowDialog(string _name, IDialogParameters _parameters, Action<IDialogResult> _callback) { _callback.Invoke(new DialogResult()); }
        }

        #endregion

        #region テスト準備・後始末

        [ClassInitialize()]
        public static void ClassInitialize(TestContext test_context)
        {
            TestUtilLib.PrepareESqlite3Dll();
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            TestUtilLib.TruncateTables();
        }

        #endregion

        /// <summary>
        /// Dispose メソッドが呼び出されることの確認
        /// </summary>
        [TestMethod]
        public void TestMethod0010()
        {
            using (var vm = new DialogWindowEditViewModel()) { vm.OnDialogOpened(null); }
        }

        /// <summary>
        /// 画面初期表示時の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0020()
        {
            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            var dialogParameters = new DialogParameters { { nameof(DialogWindowEditViewModel.Parameter), new DialogWindowEditViewModel.Parameter
            {
                TodoTask = testEntity,
            } } };

            var vm = new DialogWindowEditViewModel();
            vm.OnDialogOpened(dialogParameters);

            #region 初期表示の確認

            Assert.IsTrue(vm.CanCloseDialog());
            Assert.IsNotNull(vm.Title);

            Assert.AreEqual(testEntity.TodoTaskId, vm.TodoTaskId.Value);
            Assert.AreEqual(testEntity.DueDate.Value.Date, vm.DueDate.Value.Value.Date);
            Assert.AreEqual(testEntity.DueDate.Value.ToString("HH"), vm.DueDateHour.Value);
            Assert.AreEqual(testEntity.DueDate.Value.ToString("mm"), vm.DueDateMinute.Value);
            Assert.AreEqual(false, vm.Status.Value);
            Assert.AreEqual(testEntity.Subject, vm.Subject.Value);

            // 次のボタンは活性状態
            Assert.IsTrue(vm.UpdateClick.CanExecute());
            Assert.IsTrue(vm.CancelClick.CanExecute());
            Assert.IsTrue(vm.DeleteClick.CanExecute());
            Assert.IsTrue(vm.AddClick.CanExecute());

            #endregion

            #region 入力必須項目をクリアすると、ボタンが非活性状態となることの確認

            vm.OnDialogOpened(dialogParameters);
            vm.DueDate.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());
            Assert.IsTrue(vm.CancelClick.CanExecute());
            Assert.IsTrue(vm.DeleteClick.CanExecute());
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.OnDialogOpened(dialogParameters);
            vm.DueDateHour.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());
            Assert.IsTrue(vm.CancelClick.CanExecute());
            Assert.IsTrue(vm.DeleteClick.CanExecute());
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.OnDialogOpened(dialogParameters);
            vm.DueDateMinute.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());
            Assert.IsTrue(vm.CancelClick.CanExecute());
            Assert.IsTrue(vm.DeleteClick.CanExecute());
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.OnDialogOpened(dialogParameters);
            vm.Subject.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());
            Assert.IsTrue(vm.CancelClick.CanExecute());
            Assert.IsTrue(vm.DeleteClick.CanExecute());
            Assert.IsFalse(vm.AddClick.CanExecute());

            #endregion

            #region 追加確認（［期日］が null の場合の表示）

            testEntity.DueDate = null;
            vm.OnDialogOpened(dialogParameters);
            Assert.IsNull(vm.DueDate.Value);
            Assert.IsNull(vm.DueDateHour.Value);
            Assert.IsNull(vm.DueDateMinute.Value);

            #endregion

            vm.OnDialogClosed();
        }

        /// <summary>
        /// ボタン〔更新〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0030()
        {
            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            var dialogParameters = new DialogParameters { { nameof(DialogWindowEditViewModel.Parameter), new DialogWindowEditViewModel.Parameter
            {
                TodoTask = testEntity,
            } } };

            var vm = new DialogWindowEditViewModel();
            vm.OnDialogOpened(dialogParameters);

            vm.UpdateClick.Execute();

            // この画面でレコードが更新・登録・削除される訳ではない
            using (var context = new EfDbContext()) Assert.AreEqual(0, context.TodoTasks.Count());

            // カバレッジ 100% にするためのコード
            vm.Status.Value = true;
            vm.RequestClose += new Action<IDialogResult>(delegate { });
            vm.UpdateClick.Execute();
        }

        /// <summary>
        /// ボタン〔キャンセル〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0040()
        {
            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            var dialogParameters = new DialogParameters { { nameof(DialogWindowEditViewModel.Parameter), new DialogWindowEditViewModel.Parameter
            {
                TodoTask = testEntity,
            } } };

            var vm = new DialogWindowEditViewModel();
            vm.OnDialogOpened(dialogParameters);

            vm.CancelClick.Execute();

            // この画面でレコードが更新・登録・削除される訳ではない
            using (var context = new EfDbContext()) Assert.AreEqual(0, context.TodoTasks.Count());

            // カバレッジ 100% にするためのコード
            vm.RequestClose += new Action<IDialogResult>(delegate { });
            vm.CancelClick.Execute();
        }

        /// <summary>
        /// ボタン〔削除〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0050()
        {
            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            using (var context = new EfDbContext())
            {
                context.Add(testEntity);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            var dialogParameters = new DialogParameters { { nameof(DialogWindowEditViewModel.Parameter), new DialogWindowEditViewModel.Parameter
            {
                TodoTask = testEntity,
            } } };

            var vm = new DialogWindowEditViewModel();
            vm.OnDialogOpened(dialogParameters);

            vm.DeleteClick.Execute();

            // この画面でレコードが削除される
            using (var context = new EfDbContext()) Assert.AreEqual(0, context.TodoTasks.Count());

            // カバレッジ 100% にするためのコード
            vm.RequestClose += new Action<IDialogResult>(delegate { });
            vm.DeleteClick.Execute();
        }

        /// <summary>
        /// ボタン〔追加〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0060()
        {
            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            var dialogParameters = new DialogParameters { { nameof(DialogWindowEditViewModel.Parameter), new DialogWindowEditViewModel.Parameter
            {
                TodoTask = testEntity,
            } } };

            var vm = new DialogWindowEditViewModel();
            vm.OnDialogOpened(dialogParameters);

            // データベースにレコードが存在しない状態
            using (var context = new EfDbContext()) Assert.AreEqual(0, context.TodoTasks.Count());

            vm.AddClick.Execute();

            // この画面でレコードが登録される
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());
        }
    }
}
