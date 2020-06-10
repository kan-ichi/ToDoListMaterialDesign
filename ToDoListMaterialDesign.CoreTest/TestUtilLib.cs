using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;

namespace ToDoListMaterialDesign.CoreTest
{
    /// <summary>
    /// テスト用ユーティリティーライブラリ
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    class TestUtilLib
    {
        /// <summary>
        /// テスト実施中のフォルダパスを取得します
        /// </summary>
        public static string GetAppPath()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            //URIを通常のパス形式に変換する
            Uri u = new Uri(path);
            path = u.LocalPath + Uri.UnescapeDataString(u.Fragment);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// .NET Framework でテスト実施時、e_sqlite3.dll がテスト用フォルダに存在しないため、「x64」「x86」どちらかからコピーします
        /// </summary>
        public static void PrepareESqlite3Dll()
        {
            string eSqlite3DllName = "e_sqlite3.dll";
            string testingFolderName = TestUtilLib.GetAppPath();

            if (testingFolderName.Contains(@"\netcoreapp"))
            {
                // .NET Core アプリケーションの場合は e_sqlite3.dll が存在しないので、処理を抜ける
                return;
            }

            string eSqlite3DllPath;
            if (Environment.Is64BitProcess)
            {
                eSqlite3DllPath = Path.Combine(testingFolderName, "x64", eSqlite3DllName);
            }
            else
            {
                eSqlite3DllPath = Path.Combine(testingFolderName, "x86", eSqlite3DllName);
            }

            string eSqlite3DllCopyToPath = Path.Combine(testingFolderName, eSqlite3DllName);
            if (!File.Exists(eSqlite3DllCopyToPath))
            {
                File.Delete(eSqlite3DllCopyToPath);
                File.Copy(eSqlite3DllPath, eSqlite3DllCopyToPath);
            }
        }

        /// <summary>
        /// データベースの全レコードを削除します
        /// </summary>
        public static void TruncateTables()
        {
            using (var context = new EfDbContext { BackupRestoreMode = true })
            {
                context.Database.ExecuteSqlCommand("delete from t_TodoTask");
                context.SaveChanges();

                context.Database.ExecuteSqlCommand("delete from m_Setting");
                context.SaveChanges();
            }
        }

        /// <summary>
        /// データベースに TodoTask を登録します
        /// </summary>
        public static TodoTask AddTodoTaskToDb(TodoTask _entity)
        {
            using (var context = new EfDbContext())
            {
                context.Add<TodoTask>(_entity);
                context.SaveChanges();
            }

            return _entity;
        }

        /// <summary>
        /// データベースにランダムな TodoTask を登録します
        /// </summary>
        public static TodoTask AddTodoTaskRandomToDb()
        {
            return AddTodoTaskToDb(GenarateRandomTodoTask());
        }

        /// <summary>
        /// データベースに Setting を登録します
        /// </summary>
        public static Setting AddSettingToDb(Setting _setting)
        {
            using (var context = new EfDbContext())
            {
                context.Add<Setting>(_setting);
                context.SaveChanges();
            }

            return _setting;
        }

        /// <summary>
        /// データベースにランダムな Setting を登録します
        /// </summary>
        public static Setting AddSettingRandomToDb()
        {
            return AddSettingToDb(GenarateRandomSetting());
        }

        /// <summary>
        /// ランダムな TodoTask エンティティを生成します
        /// </summary>
        public static TodoTask GenarateRandomTodoTask()
        {
            string datePart = DateTime.Now.ToString("yyyyMMddHHmmssff");
            string guidPart = Guid.NewGuid().ToString("N");

            var entity = new TodoTask
            {
                TodoTaskId = datePart + guidPart,
                Subject = "Subject-" + datePart + "-" + guidPart,
                DueDate = DateTime.Now,
                StatusCode = "StatusCode-" + datePart + "-" + guidPart,
            };

            return entity;
        }

        /// <summary>
        /// ランダムな Setting エンティティを生成します
        /// </summary>
        public static Setting GenarateRandomSetting()
        {
            string datePart = DateTime.Now.ToString("yyyyMMddHHmmssff");
            string guidPart = Guid.NewGuid().ToString("N");

            var entity = new Setting
            {
                SettingId = datePart + guidPart,
                Value = "Value-" + datePart + "-" + guidPart,
            };

            return entity;
        }

        /// <summary>
        /// ミリ秒を切り捨てます
        /// </summary>
        public static DateTime TruncMillSecond(DateTime _date)
        {
            return new DateTime(_date.Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second, 0);
        }

        /// <summary>
        /// ミリ秒を切り捨てます
        /// </summary>
        public static TimeSpan TruncMillSecond(TimeSpan _time)
        {
            return new TimeSpan(_time.Hours, _time.Minutes, _time.Seconds, 0);
        }

    }
}
