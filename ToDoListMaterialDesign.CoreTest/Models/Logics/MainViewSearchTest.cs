using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToDoListMaterialDesign.Models.Codes;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;
using ToDoListMaterialDesign.Models.Interface;
using ToDoListMaterialDesign.Models.Logics;

namespace ToDoListMaterialDesign.CoreTest.Models.Logics
{
    /// <summary>
    /// 「Main」画面の一覧データ検索処理のテスト
    /// </summary>
    [TestClass]
    public class MainViewSearchTest
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
        /// 「Main」画面の一覧データ検索処理のテスト
        /// ・検索結果を DueDate、TodoTaskId の順番で取得すること
        /// </summary>
        [TestMethod]
        public void Test0010()
        {
            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();
            var testEntity1 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity2 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity3 = new TodoTask().CopyValuesFrom(testEntity);
            {
                testEntity1.TodoTaskId = "a";
                testEntity1.DueDate = testEntity1.DueDate.Value.AddSeconds(1);

                testEntity2.TodoTaskId = "c";

                testEntity3.TodoTaskId = "b";
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

            IRequest request = new Request();
            MainViewSearch logic = new MainViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((MainViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(3, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity2.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity1.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }

        /// <summary>
        /// 「Main」画面の一覧データ検索処理のテスト
        /// ・検索結果から StatusCode.CODE_FINISHED のレコードは除外されること
        /// </summary>
        [TestMethod]
        public void Test0020()
        {
            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();
            var testEntity1 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity2 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity3 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity4 = new TodoTask().CopyValuesFrom(testEntity);
            {
                int i = 1;

                testEntity1.TodoTaskId = i++.ToString();
                testEntity1.StatusCode = StatusCode.CODE_NOT_YET;

                testEntity2.TodoTaskId = i++.ToString();
                testEntity2.StatusCode = StatusCode.CODE_FINISHED;

                testEntity3.TodoTaskId = i++.ToString();
                testEntity3.StatusCode = StatusCode.NAME_NOT_YET;

                testEntity4.TodoTaskId = i++.ToString();
                testEntity4.StatusCode = StatusCode.NAME_FINISHED;
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

            IRequest request = new Request();
            MainViewSearch logic = new MainViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((MainViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(3, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity1.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity4.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }
    }
}
