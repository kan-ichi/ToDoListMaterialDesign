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
    /// 「Main」画面のテスト
    /// </summary>
    [TestClass]
    public class MainViewModelTest
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
            using (var vm = new MainViewModel(new DummyDialogService())) { }
        }

        /// <summary>
        /// 画面初期表示時の確認（表示データなしの場合）
        /// </summary>
        [TestMethod]
        public void TestMethod0020()
        {
            var vm = new MainViewModel(new DummyDialogService());
            vm.IsNavigationTarget(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region 初期表示の確認

            // データベースにレコードが存在しないので、データグリッドの表示行数もゼロ
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

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
            testEntity.StatusCode = StatusCode.CODE_NOT_YET;

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

            var vm = new MainViewModel(new DummyDialogService()) { TestRequest = new Request(), TestResponse = mockResponse.Object };
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region データ検索エラーの確認

            // データ検索が失敗しているので、データグリッドの表示行数もゼロ
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

            #endregion
        }

        /// <summary>
        /// ボタン〔完了〕押下処理の確認
        /// </summary>
        [TestMethod]
        public void TestMethod0040()
        {
            #region テストデータ準備

            var testEntity1 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity2 = TestUtilLib.GenarateRandomTodoTask();
            {
                testEntity1.StatusCode = StatusCode.CODE_NOT_YET;
                testEntity2.StatusCode = StatusCode.CODE_FINISHED;
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(2, context.TodoTasks.Count());

            #endregion

            var vm = new MainViewModel(new DummyDialogService());
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region 初期表示の確認

            // データグリッドには、ステータス「not yet」のレコードのみが表示されている
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);
            Assert.AreEqual(testEntity1.TodoTaskId, vm.DataGridItemsSource[0].TodoTask.TodoTaskId);

            // 行が未選択状態 ⇒ 行を選択
            Assert.IsNull(vm.DataGridSelectedItem.Value);
            vm.DataGridSelectedItem.Value = vm.DataGridItemsSource[0];
            Assert.AreEqual(testEntity1.TodoTaskId, vm.DataGridSelectedItem.Value.TodoTask.TodoTaskId);
            Assert.AreEqual(StatusCode.NAME_NOT_YET, vm.DataGridSelectedItem.Value.TodoTaskStatusName);

            #endregion

            #region ボタン〔完了〕押下

            vm.FinishClick.Execute();

            // 全てのレコードがステータス「finished」になったので、データグリッドの表示行数もゼロ
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

            #endregion
        }

        /// <summary>
        /// ボタン〔編集〕押下処理の確認（編集画面で右上［×］ボタン押下のケース）
        /// </summary>
        [TestMethod]
        public void TestMethod0050()
        {
            #region テストデータ準備

            var testEntityOrg = TestUtilLib.GenarateRandomTodoTask();
            testEntityOrg.StatusCode = StatusCode.CODE_NOT_YET;

            using (var context = new EfDbContext())
            {
                context.Add(testEntityOrg);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            var vm = new MainViewModel(new DummyDialogService());
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region ボタン〔編集〕押下 ⇒ 編集画面右上［×］ボタン押下で戻る

            // 編集前、データグリッドにはテストデータの行が存在する
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);

            vm.EditClick.Execute();

            // 編集前と変わらず、データグリッドにはテストデータの行が存在する
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);

            #endregion
        }

        /// <summary>
        /// ボタン〔編集〕押下処理の確認（編集画面でステータス「finished」に変更するケース）
        /// </summary>
        [TestMethod]
        public void TestMethod0060()
        {
            #region テストデータ準備

            var testEntityOrg = TestUtilLib.GenarateRandomTodoTask();
            testEntityOrg.StatusCode = StatusCode.CODE_NOT_YET;

            using (var context = new EfDbContext())
            {
                context.Add(testEntityOrg);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            #region ロジックの応答を設定

            var testEntityUpd = testEntityOrg.CopyValuesFrom(testEntityOrg);
            testEntityUpd.StatusCode = StatusCode.CODE_FINISHED;

            var result = new DialogParameters
            {
                { nameof(DialogWindowEditViewModel.Result), new DialogWindowEditViewModel.Result { TodoTask = testEntityUpd } }
            };

            #endregion

            var vm = new MainViewModel(new DummyDialogService())
            {
                TestDialogResult = new DialogResult(ButtonResult.OK),
                TestDialogParameters = result,
            };
            vm.OnNavigatedTo(new Prism.Regions.NavigationContext(null, new Uri("dummy", UriKind.Relative)));

            #region ボタン〔編集〕押下 ⇒ 編集画面〔更新〕押下で戻る

            // 編集前、データグリッドにはテストデータの行が存在する
            Assert.AreEqual(1, vm.DataGridItemsSource.Count);

            vm.EditClick.Execute();

            // 全てのレコードがステータス「finished」になったので、データグリッドの表示行数もゼロ
            Assert.AreEqual(0, vm.DataGridItemsSource.Count);

            #endregion
        }
    }
}
