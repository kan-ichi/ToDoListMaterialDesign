using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;
using ToDoListMaterialDesign.Models.Interface;
using ToDoListMaterialDesign.Models.Logics;

namespace ToDoListMaterialDesign.CoreTest.Models.Logics
{
    /// <summary>
    /// 「Edit」画面の一覧データ検索処理のテスト
    /// </summary>
    [TestClass]
    public class EditViewSearchTest
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
        /// 「Edit」画面の一覧データ検索処理のテスト
        /// ・検索結果を DueDate、TodoTaskId の順番で取得すること
        /// </summary>
        [TestMethod]
        public void Test0010()
        {
            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();
            var testEntity1 = new TodoTask().CopyValuesFrom(testEntity);
            testEntity1.TodoTaskId = "a";
            testEntity1.DueDate = testEntity1.DueDate.Value.AddSeconds(1);
            var testEntity2 = new TodoTask().CopyValuesFrom(testEntity);
            testEntity2.TodoTaskId = "c";
            var testEntity3 = new TodoTask().CopyValuesFrom(testEntity);
            testEntity3.TodoTaskId = "b";

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(3, context.TodoTasks.Count());

            #endregion

            var parameter = new EditViewSearch.Parameter();
            IRequest request = new Request { Parameter = parameter };
            EditViewSearch logic = new EditViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((EditViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(3, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity2.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity1.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }

        /// <summary>
        /// 「Edit」画面の一覧データ検索処理のテスト
        /// ・検索条件 SearchConditionsText のテスト（Like検索）
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
            var testEntity5 = new TodoTask().CopyValuesFrom(testEntity);
            {
                int i = 1;

                testEntity1.TodoTaskId = i++.ToString();
                testEntity1.Subject = "検索条件";

                testEntity2.TodoTaskId = i++.ToString();
                testEntity2.Subject = "_検索条件";

                testEntity3.TodoTaskId = i++.ToString();
                testEntity3.Subject = "検索条件_";

                testEntity4.TodoTaskId = i++.ToString();
                testEntity4.Subject = "_検索条件_";

                testEntity5.TodoTaskId = i++.ToString();
                testEntity5.Subject = "検索_条件";
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.Add(testEntity4);
                context.Add(testEntity5);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(5, context.TodoTasks.Count());

            #endregion

            var parameter = new EditViewSearch.Parameter { SearchConditionsText = "検索条件" };
            IRequest request = new Request { Parameter = parameter };
            EditViewSearch logic = new EditViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((EditViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(4, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity1.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity2.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity4.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }

        /// <summary>
        /// 「Edit」画面の一覧データ検索処理のテスト
        /// ・検索条件 SearchConditionsDateFrom のテスト（日付単位で、GraterOrEqual検索）
        /// </summary>
        [TestMethod]
        public void Test0030()
        {
            var testDateTime = DateTime.Now;

            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();
            var testEntity1 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity2 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity3 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity4 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity5 = new TodoTask().CopyValuesFrom(testEntity);
            {
                int i = 1;

                testEntity1.TodoTaskId = i++.ToString();
                testEntity1.DueDate = testDateTime.Date.AddMilliseconds(-1); // 昨日の 23:59:59.999

                testEntity2.TodoTaskId = i++.ToString();
                testEntity2.DueDate = testDateTime.Date; // 本日の 00:00:00.000

                testEntity3.TodoTaskId = i++.ToString();
                testEntity3.DueDate = testDateTime; // 本日の今の時刻

                testEntity4.TodoTaskId = i++.ToString();
                testEntity4.DueDate = testDateTime.Date.AddDays(1).AddMilliseconds(-1); // 本日の 23:59:59.999

                testEntity5.TodoTaskId = i++.ToString();
                testEntity5.DueDate = testDateTime.Date.AddDays(1); // 明日の 00:00:00.000
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.Add(testEntity4);
                context.Add(testEntity5);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(5, context.TodoTasks.Count());

            #endregion

            var parameter = new EditViewSearch.Parameter { SearchConditionsDateFrom = testDateTime };
            IRequest request = new Request { Parameter = parameter };
            EditViewSearch logic = new EditViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((EditViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(4, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity2.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity4.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity5.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }

        /// <summary>
        /// 「Edit」画面の一覧データ検索処理のテスト
        /// ・検索条件 SearchConditionsDateTo のテスト（日付単位で、LessOrEqual検索）
        /// </summary>
        [TestMethod]
        public void Test0040()
        {
            var testDateTime = DateTime.Now;

            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();
            var testEntity1 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity2 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity3 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity4 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity5 = new TodoTask().CopyValuesFrom(testEntity);
            {
                int i = 1;

                testEntity1.TodoTaskId = i++.ToString();
                testEntity1.DueDate = testDateTime.Date.AddMilliseconds(-1); // 昨日の 23:59:59.999

                testEntity2.TodoTaskId = i++.ToString();
                testEntity2.DueDate = testDateTime.Date; // 本日の 00:00:00.000

                testEntity3.TodoTaskId = i++.ToString();
                testEntity3.DueDate = testDateTime; // 本日の今の時刻

                testEntity4.TodoTaskId = i++.ToString();
                testEntity4.DueDate = testDateTime.Date.AddDays(1).AddMilliseconds(-1); // 本日の 23:59:59.999

                testEntity5.TodoTaskId = i++.ToString();
                testEntity5.DueDate = testDateTime.Date.AddDays(1); // 明日の 00:00:00.000
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.Add(testEntity4);
                context.Add(testEntity5);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(5, context.TodoTasks.Count());

            #endregion

            var parameter = new EditViewSearch.Parameter { SearchConditionsDateTo = testDateTime };
            IRequest request = new Request { Parameter = parameter };
            EditViewSearch logic = new EditViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((EditViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(4, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity1.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity2.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
                Assert.AreEqual(testEntity4.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }

        /// <summary>
        /// 「Edit」画面の一覧データ検索処理のテスト
        /// ・検索条件が複合する場合のテスト
        /// </summary>
        [TestMethod]
        public void Test0050()
        {
            var subjectMatch = "検索条件";
            var subjectUnMatch = "検索_条件";
            var testDateTime = DateTime.Now;
            var yesterday = testDateTime.Date.AddDays(-1);
            var today = testDateTime.Date;
            var tommorow = testDateTime.Date.AddDays(1);

            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();
            var testEntity1 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity2 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity3 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity4 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity5 = new TodoTask().CopyValuesFrom(testEntity);
            var testEntity6 = new TodoTask().CopyValuesFrom(testEntity);
            {
                int i = 1;

                testEntity1.TodoTaskId = i++.ToString();
                testEntity1.Subject = subjectMatch;
                testEntity1.DueDate = yesterday;

                testEntity2.TodoTaskId = i++.ToString();
                testEntity2.Subject = subjectUnMatch;
                testEntity2.DueDate = yesterday;

                testEntity3.TodoTaskId = i++.ToString();
                testEntity3.Subject = subjectMatch;
                testEntity3.DueDate = today;

                testEntity4.TodoTaskId = i++.ToString();
                testEntity4.Subject = subjectUnMatch;
                testEntity4.DueDate = today;

                testEntity5.TodoTaskId = i++.ToString();
                testEntity5.Subject = subjectMatch;
                testEntity5.DueDate = tommorow;

                testEntity6.TodoTaskId = i++.ToString();
                testEntity6.Subject = subjectUnMatch;
                testEntity6.DueDate = tommorow;
            }

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.Add(testEntity2);
                context.Add(testEntity3);
                context.Add(testEntity4);
                context.Add(testEntity5);
                context.Add(testEntity6);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(6, context.TodoTasks.Count());

            #endregion

            var parameter = new EditViewSearch.Parameter
            {
                SearchConditionsText = subjectMatch,
                SearchConditionsDateFrom = testDateTime,
                SearchConditionsDateTo = testDateTime,
            };
            IRequest request = new Request { Parameter = parameter };
            EditViewSearch logic = new EditViewSearch();
            IResponse response = logic.DoProcess(request);
            var searchResults = ((EditViewSearch.Result)response.Result).TodoTasks;

            Assert.AreEqual(1, searchResults.Count);
            {
                int i = 0;
                Assert.AreEqual(testEntity3.TodoTaskId, searchResults[i++].TodoTaskId);
            }
        }
    }
}
