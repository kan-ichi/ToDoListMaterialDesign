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
    /// 「Edit」画面のテスト
    /// </summary>
    [TestClass]
    public class EditViewModelTest
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
            using (var vm = new EditViewModel(new DummyDialogService())) { }
        }

        /// <summary>
        /// 画面初期表示時の確認（表示データなしの場合）
        /// </summary>
        [TestMethod]
        public void TestMethod0020()
        {
            var vm = new EditViewModel(new DummyDialogService());
            vm.IsNavigationTarget(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region 初期表示の確認

            // データベースにレコードが存在しないので、データグリッドの表示行数もゼロ
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

            // 次の入力欄は空の状態
            Assert.IsNull(vm.SearchConditionsText.Value);
            Assert.IsNull(vm.SearchConditionsDateFrom.Value);
            Assert.IsNull(vm.SearchConditionsDateTo.Value);
            Assert.IsNull(vm.DueDate.Value);
            Assert.IsNull(vm.DueDateHour.Value);
            Assert.IsNull(vm.DueDateMinute.Value);
            Assert.IsFalse(vm.Status.Value);
            Assert.IsNull(vm.Subject.Value);

            // 次のボタンは活性状態
            Assert.IsTrue(vm.SearchClick.CanExecute());
            Assert.IsTrue(vm.DeleteClick.CanExecute());

            // 次のボタンは非活性状態
            Assert.IsFalse(vm.SearchConditionsClearClick.CanExecute());
            Assert.IsFalse(vm.UpdateClick.CanExecute());
            Assert.IsFalse(vm.AddClick.CanExecute());

            #endregion

            vm.OnNavigatedFrom(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));
        }

        /// <summary>
        /// データ検索エラーが発生したケースの確認
        /// </summary>
        [TestMethod]
        public void TestMethod0030()
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

            #region ロジックの応答を設定

            var mockResponse = new Mock<IResponse>();
            mockResponse.Setup(c => c.IsSucceed).Returns(false);

            #endregion

            var vm = new EditViewModel(new DummyDialogService()) { TestRequest = new Request { Parameter = new EditViewSearch.Parameter() }, TestResponse = mockResponse.Object };
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region データ検索エラーの確認

            // データ検索が失敗しているので、データグリッドの表示行数もゼロ
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

            #endregion
        }

        /// <summary>
        /// ボタン〔検索〕〔検索条件クリア〕押下処理の確認
        /// ・データグリッドには、条件に一致したレコードのみが表示されること
        /// ・検索条件クリアが、正常に行われること
        /// </summary>
        [TestMethod]
        public void TestMethod0040()
        {
            var subjectMatch = "検索条件";
            var subjectUnMatch = "検索_条件";
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var tommorow = today.AddDays(1);

            #region テストデータ準備

            var testEntity1 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity2 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity3 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity4 = TestUtilLib.GenarateRandomTodoTask();
            {
                testEntity1.Subject = subjectMatch;
                testEntity1.DueDate = yesterday;

                testEntity2.Subject = subjectMatch;
                testEntity2.DueDate = today;
                testEntity2.StatusCode = StatusCode.CODE_FINISHED;

                testEntity3.Subject = subjectUnMatch;
                testEntity3.DueDate = tommorow.AddMilliseconds(-1);

                testEntity4.Subject = subjectMatch;
                testEntity4.DueDate = tommorow;
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.Add(testEntity4);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(4, context.TodoTasks.Count());

            #endregion

            var vm = new EditViewModel(new DummyDialogService());
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region 初期表示の確認

            // データグリッドには、全てのレコードが表示される（絞り込み条件を入力していないため）
            Assert.AreEqual(4, vm.DataGridItemsSource.Count);

            #endregion

            #region ［検索ワード］のみを入力し、ボタン〔検索〕押下

            vm.SearchConditionsText.Value = subjectMatch;
            vm.SearchClick.Execute();

            // データグリッドには、条件に一致したレコードのみが表示される
            Assert.AreEqual(3, vm.DataGridItemsSource.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity1.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity2.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity4.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
            }

            #endregion

            #region ボタン〔検索条件クリア〕押下後、［検索開始日］のみを入力し、ボタン〔検索〕押下

            vm.SearchConditionsClearClick.Execute();
            vm.SearchConditionsDateFrom.Value = today;
            vm.SearchClick.Execute();

            // データグリッドには、条件に一致したレコードのみが表示される
            Assert.AreEqual(3, vm.DataGridItemsSource.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity2.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity4.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
            }

            #endregion

            #region ボタン〔検索条件クリア〕押下後、［検索終了日］のみを入力し、ボタン〔検索〕押下

            vm.SearchConditionsClearClick.Execute();
            vm.SearchConditionsDateTo.Value = today;
            vm.SearchClick.Execute();

            // データグリッドには、条件に一致したレコードのみが表示される
            Assert.AreEqual(3, vm.DataGridItemsSource.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity1.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity2.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
            }

            #endregion

            #region ボタン〔検索条件クリア〕押下後、全ての検索項目を入力し、ボタン〔検索〕押下

            vm.SearchConditionsClearClick.Execute();
            vm.SearchConditionsText.Value = subjectMatch;
            vm.SearchConditionsDateFrom.Value = today;
            vm.SearchConditionsDateTo.Value = today;
            vm.SearchClick.Execute();

            // データグリッドには、条件に一致したレコードのみが表示される
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);
            Assert.AreEqual(testEntity2.TodoTaskId, vm.DataGridItemsSource[0].TodoTask.TodoTaskId);
            Assert.AreEqual(StatusCode.NAME_FINISHED, vm.DataGridItemsSource[0].TodoTaskStatusName);

            #endregion
        }

        /// <summary>
        /// ボタン〔選択した行を削除〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0050()
        {
            #region テストデータ準備

            var testEntity1 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity2 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity3 = TestUtilLib.GenarateRandomTodoTask();
            {
                var testDateTime = DateTime.Now;
                int i = 0;

                testEntity1.StatusCode = StatusCode.CODE_FINISHED;
                testEntity1.DueDate = testDateTime.AddMilliseconds(i++);

                testEntity2.StatusCode = StatusCode.CODE_FINISHED;
                testEntity2.DueDate = testDateTime.AddMilliseconds(i++);

                testEntity3.StatusCode = StatusCode.CODE_FINISHED;
                testEntity3.DueDate = testDateTime.AddMilliseconds(i++);
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(3, context.TodoTasks.Count());

            #endregion

            var vm = new EditViewModel(new DummyDialogService());
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region チェックボックスで削除行を選択せずに、ボタン〔選択した行を削除〕押下

            Assert.AreEqual(3, vm.DataGridItemsSource.Count);

            // データグリッドの行を選択すると、各入力項目に値がセットされる
            Assert.AreEqual(testEntity2.TodoTaskId, vm.DataGridItemsSource[1].TodoTask.TodoTaskId);
            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[1];
            Assert.AreEqual(testEntity2.TodoTaskId, vm.TodoTaskId.Value);
            Assert.AreEqual(testEntity2.DueDate.Value.Date, vm.DueDate.Value.Value.Date);
            Assert.AreEqual(testEntity2.DueDate.Value.ToString("HH"), vm.DueDateHour.Value);
            Assert.AreEqual(testEntity2.DueDate.Value.ToString("mm"), vm.DueDateMinute.Value);
            Assert.AreEqual(testEntity2.Subject, vm.Subject.Value);


            // ボタン〔選択した行を削除〕を押下したが、チェックボックスで削除行を選択していないため
            // 削除処理は行われず、画面の表示状態は変化しない
            vm.DeleteClick.Execute();

            Assert.AreEqual(3, vm.DataGridItemsSource.Count);
            Assert.AreEqual(testEntity2.TodoTaskId, vm.TodoTaskId.Value);
            Assert.AreEqual(testEntity2.DueDate.Value.Date, vm.DueDate.Value.Value.Date);
            Assert.AreEqual(testEntity2.DueDate.Value.ToString("HH"), vm.DueDateHour.Value);
            Assert.AreEqual(testEntity2.DueDate.Value.ToString("mm"), vm.DueDateMinute.Value);
            Assert.AreEqual(testEntity2.Subject, vm.Subject.Value);

            #endregion

            #region チェックボックスで削除行を選択して、ボタン〔選択した行を削除〕押下

            Assert.AreEqual(3, vm.DataGridItemsSource.Count);

            // チェックボックスで削除行を選択し、ボタン〔選択した行を削除〕を押下
            // データグリッドからその行が無くなり、各入力項目の値もクリアされる
            Assert.AreEqual(testEntity2.TodoTaskId, vm.DataGridItemsSource[1].TodoTask.TodoTaskId);
            vm.DataGridItemsSource[1].IsChecked = true;
            vm.DeleteClick.Execute();

            Assert.AreEqual(2, vm.DataGridItemsSource.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity1.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, vm.DataGridItemsSource[i++].TodoTask.TodoTaskId);
            }
            Assert.IsNull(vm.TodoTaskId.Value);
            Assert.IsNull(vm.DueDate.Value);
            Assert.IsNull(vm.DueDateHour.Value);
            Assert.IsNull(vm.DueDateMinute.Value);
            Assert.IsFalse(vm.Status.Value);
            Assert.IsNull(vm.Subject.Value);

            #endregion

            #region チェックボックスで削除行を全て選択して、ボタン〔選択した行を削除〕押下

            Assert.AreEqual(2, vm.DataGridItemsSource.Count);

            for (int i = 0; i < vm.DataGridItemsSource.Count; i++) vm.DataGridItemsSource[i].IsChecked = true;
            vm.DeleteClick.Execute();

            // データグリッドから全ての行が無くなる
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

            #endregion
        }

        /// <summary>
        /// ボタン〔更新〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0060()
        {
            var inputDueDate = DateTime.MinValue.Date;
            var inputDueDateHour = "23";
            var inputDueDateMinute = "59";
            var inputStatus = true;
            var inputSubject = "ButtonUpdateTest";

            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            using (var context = new EfDbContext())
            {
                context.Add(testEntity);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            var vm = new EditViewModel(new DummyDialogService());
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region 初期表示の確認

            // データグリッドの表示内容は、更新前の状態
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);
            {
                var dataGridItem = vm.DataGridItemsSource[0];
                Assert.AreEqual(testEntity.TodoTaskId, dataGridItem.TodoTask.TodoTaskId);
                Assert.AreEqual(testEntity.DueDate, dataGridItem.TodoTask.DueDate);
                Assert.AreEqual(testEntity.StatusCode, dataGridItem.TodoTask.StatusCode);
                Assert.AreEqual(testEntity.Subject, dataGridItem.TodoTask.Subject);
            }

            // ボタンは非活性状態
            Assert.IsFalse(vm.UpdateClick.CanExecute());

            #endregion

            #region データグリッドの行を選択・各入力項目の有無によるボタン活性状態変更の確認

            // データグリッドの行を選択すると、各入力項目に値がセットされる
            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            Assert.AreEqual(testEntity.TodoTaskId, vm.TodoTaskId.Value);
            Assert.AreEqual(testEntity.DueDate.Value.Date, vm.DueDate.Value.Value.Date);
            Assert.AreEqual(testEntity.DueDate.Value.ToString("HH"), vm.DueDateHour.Value);
            Assert.AreEqual(testEntity.DueDate.Value.ToString("mm"), vm.DueDateMinute.Value);
            Assert.AreEqual(false, vm.Status.Value);
            Assert.AreEqual(testEntity.Subject, vm.Subject.Value);

            // ボタンは活性状態
            Assert.IsTrue(vm.UpdateClick.CanExecute());

            // 入力必須項目をクリアすると、ボタンは非活性状態となる
            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.DueDate.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());

            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.DueDateHour.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());

            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.DueDateMinute.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());

            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.Subject.Value = null;
            Assert.IsFalse(vm.UpdateClick.CanExecute());

            #endregion

            #region 画面の各項目を入力して〔更新〕を押下し、その結果をデータグリッドで確認

            vm.DueDate.Value = inputDueDate;
            vm.DueDateHour.Value = inputDueDateHour;
            vm.DueDateMinute.Value = inputDueDateMinute;
            vm.Status.Value = inputStatus;
            vm.Subject.Value = inputSubject;

            vm.UpdateClick.Execute();

            // データグリッドの表示内容は、各項目に入力した値に変化する
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);
            {
                var dataGridItem = vm.DataGridItemsSource[0];
                Assert.AreEqual(testEntity.TodoTaskId, dataGridItem.TodoTask.TodoTaskId);
                Assert.AreEqual(inputDueDate.AddHours(Convert.ToInt32(inputDueDateHour)).AddMinutes(Convert.ToInt32(inputDueDateMinute)), dataGridItem.TodoTask.DueDate);
                Assert.AreEqual(StatusCode.CODE_FINISHED, dataGridItem.TodoTask.StatusCode);
                Assert.AreEqual(inputSubject, dataGridItem.TodoTask.Subject);
            }

            // 各入力項目の値はクリアされ、ボタンも非活性状態となる
            Assert.IsNull(vm.TodoTaskId.Value);
            Assert.IsNull(vm.DueDate.Value);
            Assert.IsNull(vm.DueDateHour.Value);
            Assert.IsNull(vm.DueDateMinute.Value);
            Assert.IsFalse(vm.Status.Value);
            Assert.IsNull(vm.Subject.Value);
            Assert.IsFalse(vm.UpdateClick.CanExecute());

            #endregion
        }

        /// <summary>
        /// ボタン〔追加〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0070()
        {
            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            var vm = new EditViewModel(new DummyDialogService());
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region 画面の各項目を入力して〔追加〕を押下し、その結果をデータグリッドで確認

            // 必須項目入力前、ボタンは非活性状態
            Assert.IsFalse(vm.AddClick.CanExecute());

            // 必須項目入力後、ボタンは活性状態となる
            vm.DueDate.Value = testEntity.DueDate.Value.Date;
            Assert.IsFalse(vm.AddClick.CanExecute());
            vm.DueDateHour.Value = testEntity.DueDate.Value.ToString("HH");
            Assert.IsFalse(vm.AddClick.CanExecute());
            vm.DueDateMinute.Value = testEntity.DueDate.Value.ToString("mm");
            Assert.IsFalse(vm.AddClick.CanExecute());
            vm.Subject.Value = testEntity.Subject;
            Assert.IsTrue(vm.AddClick.CanExecute());
            Assert.IsFalse(vm.UpdateClick.CanExecute()); // ボタン〔更新〕は非活性

            vm.AddClick.Execute();

            // データグリッドに、入力した行が追加される
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);
            {
                var dataGridItem = vm.DataGridItemsSource[0];
                Assert.AreNotEqual(testEntity.TodoTaskId, dataGridItem.TodoTask.TodoTaskId); // 新しいレコードなので、レコードIDは一致しない
                Assert.AreEqual(testEntity.DueDate.Value.Date.AddHours(testEntity.DueDate.Value.Hour).AddMinutes(testEntity.DueDate.Value.Minute), dataGridItem.TodoTask.DueDate);
                Assert.AreEqual(StatusCode.CODE_NOT_YET, dataGridItem.TodoTask.StatusCode);
                Assert.AreEqual(testEntity.Subject, dataGridItem.TodoTask.Subject);
            }

            // 各入力項目の値はクリアされ、ボタンも非活性状態となる
            Assert.IsNull(vm.TodoTaskId.Value);
            Assert.IsNull(vm.DueDate.Value);
            Assert.IsNull(vm.DueDateHour.Value);
            Assert.IsNull(vm.DueDateMinute.Value);
            Assert.IsFalse(vm.Status.Value);
            Assert.IsNull(vm.Subject.Value);
            Assert.IsFalse(vm.AddClick.CanExecute());

            #endregion

            #region データグリッドの行を選択し、入力項目を変更せずに〔追加〕を押下する。もう一行レコードが追加されることをデータグリッドで確認

            // データグリッドの行を選択すると、各入力項目に値がセットされる
            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            Assert.AreEqual(testEntity.DueDate.Value.Date, vm.DueDate.Value.Value.Date);
            Assert.AreEqual(testEntity.DueDate.Value.ToString("HH"), vm.DueDateHour.Value);
            Assert.AreEqual(testEntity.DueDate.Value.ToString("mm"), vm.DueDateMinute.Value);
            Assert.AreEqual(false, vm.Status.Value);
            Assert.AreEqual(testEntity.Subject, vm.Subject.Value);

            // ボタンは活性状態
            Assert.IsTrue(vm.AddClick.CanExecute());

            // 入力必須項目をクリアすると、ボタンは非活性状態となる
            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.DueDate.Value = null;
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.DueDateHour.Value = null;
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.DueDateMinute.Value = null;
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            vm.Subject.Value = null;
            Assert.IsFalse(vm.AddClick.CanExecute());

            vm.AddClick.Execute();

            // データグリッドに、入力した行が追加される
            Assert.AreEqual(2, vm.DataGridItemsSource.Count);
            {
                var dataGridItem = vm.DataGridItemsSource[1];
                Assert.AreNotEqual(testEntity.TodoTaskId, dataGridItem.TodoTask.TodoTaskId); // 新しいレコードなので、レコードIDは一致しない
                Assert.AreEqual(testEntity.DueDate.Value.Date.AddHours(testEntity.DueDate.Value.Hour).AddMinutes(testEntity.DueDate.Value.Minute), dataGridItem.TodoTask.DueDate);
                Assert.AreEqual(StatusCode.CODE_NOT_YET, dataGridItem.TodoTask.StatusCode);
                Assert.AreEqual(testEntity.Subject, dataGridItem.TodoTask.Subject);
            }

            #endregion
        }
    }
}
