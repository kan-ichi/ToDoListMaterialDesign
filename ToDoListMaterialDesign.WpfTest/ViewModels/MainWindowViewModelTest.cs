using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ToDoListMaterialDesign.CoreTest;
using ToDoListMaterialDesign.ViewModels;

namespace ToDoListMaterialDesign.WpfTest.ViewModels
{
    /// <summary>
    /// メインウィンドウ（シェル画面）のテスト
    /// </summary>
    [TestClass]
    public class MainWindowViewModelTest
    {
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
            using (var vm = new MainWindowViewModel()) { }
        }

        /// <summary>
        /// 左側リストボックスに表示する画面一覧の確認（一覧の任意のアイテムを選択できることも確認）
        /// </summary>
        [TestMethod]
        public void TestMethod0020()
        {
            var vm = new MainWindowViewModel(new Prism.Regions.RegionManager());
            {
                int i = 0;
                Assert.AreEqual(" Main", vm.ListBoxItemsSource[i++].DisplayName);
                Assert.AreEqual(" Edit", vm.ListBoxItemsSource[i++].DisplayName);
            }

            vm.ListBoxSelectedItem.Value = vm.ListBoxItemsSource[0];
        }
    }
}
