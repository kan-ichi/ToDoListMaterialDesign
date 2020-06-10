using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;

namespace ToDoListMaterialDesign.CoreTest.Models.DbAccess
{
    [TestClass]
    public class DalTodoTaskTest
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

        [TestMethod]
        public void Test0010()
        {
            #region テストデータ準備

            var testEntity = TestUtilLib.GenarateRandomTodoTask();

            using (var context = new EfDbContext()) Assert.AreEqual(0, context.TodoTasks.Count());

            #endregion

            Assert.AreEqual(1, DalTodoTask.Add(testEntity));

            #region データを取得し、結果を確認（レコード登録日時、レコード更新日時 以外は一致するはず）

            List<TodoTask> dbEntities;
            using (var context = new EfDbContext()) dbEntities = context.TodoTasks.ToList();
            Assert.AreEqual(1, dbEntities.Count);

            var dbEntity = dbEntities[0];
            foreach (var property in testEntity.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntity.CreateDateTime):
                    case nameof(testEntity.UpdateDateTime):
                        Assert.AreNotEqual(property.GetValue(testEntity), property.GetValue(dbEntity));
                        break;
                    default:
                        Assert.AreEqual(property.GetValue(testEntity), property.GetValue(dbEntity));
                        break;
                }
            }

            #endregion
        }

        [TestMethod]
        public void Test0020()
        {
            #region テストデータ準備

            var testEntityCopyFrom = TestUtilLib.GenarateRandomTodoTask();
            var testEntityCopyTo = TestUtilLib.GenarateRandomTodoTask();
            testEntityCopyTo.CreateDateTime = DateTime.MinValue.AddMinutes(1);
            testEntityCopyTo.UpdateDateTime = DateTime.MaxValue.AddMinutes(-1);
            testEntityCopyTo.DueDate = testEntityCopyFrom.DueDate.Value.AddMinutes(1);

            foreach (var property in testEntityCopyFrom.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityCopyFrom.CreateDateTime):
                    case nameof(testEntityCopyFrom.UpdateDateTime):
                        break;
                    default:
                        Assert.AreNotEqual(property.GetValue(testEntityCopyFrom), property.GetValue(testEntityCopyTo));
                        break;
                }
            }

            #endregion

            DalTodoTask.CopyValuesFrom(testEntityCopyTo, testEntityCopyFrom);

            #region コピー後の値を比較（レコードID、レコード登録日時、レコード更新日時 以外は一致するはず）

            foreach (var property in testEntityCopyFrom.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityCopyFrom.TodoTaskId):
                    case nameof(testEntityCopyFrom.CreateDateTime):
                    case nameof(testEntityCopyFrom.UpdateDateTime):
                        Assert.AreNotEqual(property.GetValue(testEntityCopyFrom), property.GetValue(testEntityCopyTo));
                        break;
                    default:
                        Assert.AreEqual(property.GetValue(testEntityCopyFrom), property.GetValue(testEntityCopyTo));
                        break;
                }
            }

            #endregion
        }

        private class TodoTaskCanWriteTest : TodoTask
        {
            private string _canWriteTest_;
            public string CanWriteTest { get { return _canWriteTest_; } }
            public TodoTaskCanWriteTest(string _canWriteTest) { _canWriteTest_ = _canWriteTest; }
        }

        [TestMethod]
        public void Test0025()
        {
            #region テストデータ準備

            var testEntityCopyFrom = TestUtilLib.GenarateRandomTodoTask();
            var testEntityCopyTo = new TodoTaskCanWriteTest("dummy");

            #endregion

            DalTodoTask.CopyValuesFrom(testEntityCopyTo, testEntityCopyFrom);

            #region コピー後の値を比較（レコードID、追加プロパティ 以外は一致するはず（レコード登録日時、レコード更新日時 は比較除外））

            foreach (var property in testEntityCopyTo.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityCopyTo.TodoTaskId):
                        Assert.AreNotEqual(property.GetValue(testEntityCopyFrom), property.GetValue(testEntityCopyTo));
                        break;
                    case nameof(testEntityCopyTo.CreateDateTime):
                    case nameof(testEntityCopyTo.UpdateDateTime):
                        break;
                    case nameof(testEntityCopyTo.CanWriteTest):
                        Assert.AreEqual("dummy", property.GetValue(testEntityCopyTo));
                        break;
                    default:
                        Assert.AreEqual(property.GetValue(testEntityCopyFrom), property.GetValue(testEntityCopyTo));
                        break;
                }
            }

            #endregion
        }

        [TestMethod]
        public void Test0030()
        {
            #region テストデータ準備

            var testEntity1 = TestUtilLib.GenarateRandomTodoTask();
            var testEntity2 = TestUtilLib.GenarateRandomTodoTask();

            using (var context = new EfDbContext())
            {
                context.Add(testEntity1);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            var dbEntity1 = DalTodoTask.Find(testEntity1.TodoTaskId);
            var dbEntity2 = DalTodoTask.Find(testEntity2.TodoTaskId);

            Assert.IsNull(dbEntity2);

            #region DBから取得したエンティティと値を比較（全て一致するはず）

            foreach (var property in testEntity1.GetType().GetProperties())
            {
                Assert.AreEqual(property.GetValue(testEntity1), property.GetValue(dbEntity1));
            }

            #endregion
        }

        [TestMethod]
        public void Test0040()
        {
            #region テストデータ準備

            var testEntityBeforeUpdate = TestUtilLib.GenarateRandomTodoTask();
            var testEntityOfUpdate = TestUtilLib.GenarateRandomTodoTask();
            testEntityOfUpdate.TodoTaskId = testEntityBeforeUpdate.TodoTaskId;
            testEntityOfUpdate.DueDate = testEntityBeforeUpdate.DueDate.Value.AddMinutes(1);

            using (var context = new EfDbContext())
            {
                context.Add(testEntityBeforeUpdate);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            Assert.AreEqual(0, DalTodoTask.Update(new TodoTask { TodoTaskId = null }));
            Assert.AreEqual(1, DalTodoTask.Update(testEntityOfUpdate));
            List<TodoTask> dbEntities;

            #region データを取得し、結果を確認（全て一致するはず（レコード登録日時、レコード更新日時 は比較除外））

            using (var context = new EfDbContext()) dbEntities = context.TodoTasks.ToList();
            Assert.AreEqual(1, dbEntities.Count);

            var dbEntity = dbEntities[0];
            foreach (var property in testEntityOfUpdate.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityOfUpdate.TodoTaskId):
                        Assert.AreEqual(property.GetValue(testEntityBeforeUpdate), property.GetValue(dbEntity));
                        Assert.AreEqual(property.GetValue(testEntityOfUpdate), property.GetValue(dbEntity));
                        break;
                    case nameof(testEntityOfUpdate.CreateDateTime):
                    case nameof(testEntityOfUpdate.UpdateDateTime):
                        break;
                    default:
                        Assert.AreNotEqual(property.GetValue(testEntityBeforeUpdate), property.GetValue(dbEntity));
                        Assert.AreEqual(property.GetValue(testEntityOfUpdate), property.GetValue(dbEntity));
                        break;
                }
            }

            #endregion
        }

        [TestMethod]
        public void Test0050()
        {
            #region テストデータ準備

            var testEntityBeforeUpdate = TestUtilLib.GenarateRandomTodoTask();
            var testEntityOfUpdate = TestUtilLib.GenarateRandomTodoTask();
            var testEntityOfAdd1 = TestUtilLib.GenarateRandomTodoTask();
            var testEntityOfAdd2 = new TodoTask();
            testEntityOfUpdate.TodoTaskId = testEntityBeforeUpdate.TodoTaskId;
            testEntityOfUpdate.DueDate = testEntityBeforeUpdate.DueDate.Value.AddMinutes(1);

            using (var context = new EfDbContext())
            {
                context.Add(testEntityBeforeUpdate);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            TodoTask dbEntity;
            Assert.AreEqual(1, DalTodoTask.Upsert(testEntityOfUpdate));

            #region データを取得し、結果を確認（全て一致するはず（レコード登録日時、レコード更新日時 は比較除外））

            using (var context = new EfDbContext()) dbEntity = context.TodoTasks.Find(testEntityOfUpdate.TodoTaskId);

            foreach (var property in testEntityOfUpdate.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityOfUpdate.TodoTaskId):
                        Assert.AreEqual(property.GetValue(testEntityBeforeUpdate), property.GetValue(dbEntity));
                        Assert.AreEqual(property.GetValue(testEntityOfUpdate), property.GetValue(dbEntity));
                        break;
                    case nameof(testEntityOfUpdate.CreateDateTime):
                    case nameof(testEntityOfUpdate.UpdateDateTime):
                        break;
                    default:
                        Assert.AreNotEqual(property.GetValue(testEntityBeforeUpdate), property.GetValue(dbEntity));
                        Assert.AreEqual(property.GetValue(testEntityOfUpdate), property.GetValue(dbEntity));
                        break;
                }
            }

            #endregion

            Assert.AreEqual(1, DalTodoTask.Upsert(testEntityOfAdd1));

            #region データを取得し、結果を確認（全て一致するはず（レコード登録日時、レコード更新日時 は比較除外））

            using (var context = new EfDbContext()) dbEntity = context.TodoTasks.Find(testEntityOfAdd1.TodoTaskId);

            foreach (var property in testEntityOfAdd1.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityOfAdd1.CreateDateTime):
                    case nameof(testEntityOfAdd1.UpdateDateTime):
                        Assert.AreNotEqual(property.GetValue(testEntityOfAdd1), property.GetValue(dbEntity));
                        break;
                    default:
                        Assert.AreEqual(property.GetValue(testEntityOfAdd1), property.GetValue(dbEntity));
                        break;
                }
            }

            #endregion

            Assert.AreEqual(1, DalTodoTask.Upsert(testEntityOfAdd2));

            #region データを取得し、結果を確認（全て一致するはず（レコード登録日時、レコード更新日時 は比較除外））

            using (var context = new EfDbContext()) dbEntity = context.TodoTasks.Find(testEntityOfAdd2.TodoTaskId);

            foreach (var property in testEntityOfAdd2.GetType().GetProperties())
            {
                switch (property.Name)
                {
                    case nameof(testEntityOfAdd2.CreateDateTime):
                    case nameof(testEntityOfAdd2.UpdateDateTime):
                        Assert.AreNotEqual(property.GetValue(testEntityOfAdd2), property.GetValue(dbEntity));
                        break;
                    default:
                        Assert.AreEqual(property.GetValue(testEntityOfAdd2), property.GetValue(dbEntity));
                        break;
                }
            }

            #endregion
        }

        [TestMethod]
        public void Test0060()
        {
            #region テストデータ準備

            var testEntityExist = TestUtilLib.GenarateRandomTodoTask();
            var testEntityNotExist = TestUtilLib.GenarateRandomTodoTask();

            using (var context = new EfDbContext())
            {
                context.Add(testEntityExist);
                context.SaveChanges();
            }
            using (var context = new EfDbContext()) Assert.AreEqual(1, context.TodoTasks.Count());

            #endregion

            Assert.AreEqual(1, DalTodoTask.Delete(testEntityExist.TodoTaskId));
            Assert.AreEqual(0, DalTodoTask.Delete(testEntityNotExist.TodoTaskId));
            using (var context = new EfDbContext()) Assert.AreEqual(0, context.TodoTasks.Count());
        }

        [TestMethod]
        public void Test0070()
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

            List<TodoTask> dbEntities;
            dbEntities = DalTodoTask.SelectAll();

            Assert.AreEqual(3, dbEntities.Count);
            int i = 0;
            Assert.AreEqual(testEntity3.TodoTaskId, dbEntities[i++].TodoTaskId);
            Assert.AreEqual(testEntity2.TodoTaskId, dbEntities[i++].TodoTaskId);
            Assert.AreEqual(testEntity1.TodoTaskId, dbEntities[i++].TodoTaskId);
        }

    }
}
