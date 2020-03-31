using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ToDoListMaterialDesign.Models.DbAccess
{
    public class EfDbContext : DbContext
    {
        public DbSet<Entities.Setting> Settings { get; set; }
        public DbSet<Entities.TodoTask> TodoTasks { get; set; }
        public bool BackupRestoreMode { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=ToDoListMaterialDesign.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public override int SaveChanges()
        {
            if (this.BackupRestoreMode == false)
            {
                var now = DateTime.Now;
                SetCreateDateTime(now);
                SetUpdateDateTime(now);
            }
            return base.SaveChanges();
        }

        private void SetUpdateDateTime(DateTime _updateDateTime)
        {
            var entities = this.ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified) && e.Properties.Any(p => p.Metadata.Name == "UpdateDateTime"))
                .Select(e => e.Entity);

            foreach (dynamic entity in entities)
            {
                entity.UpdateDateTime = _updateDateTime;
            }
        }

        private void SetCreateDateTime(DateTime _createDateTime)
        {
            var entities = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Properties.Any(p => p.Metadata.Name == "CreateDateTime"))
                .Select(e => e.Entity);

            foreach (dynamic entity in entities)
            {
                entity.CreateDateTime = _createDateTime;
            }
        }

        /// <summary>
        /// 主キーとして使用する文字列を生成します
        /// </summary>
        public static string GenerateId()
        {
            string datePart = DateTime.Now.ToString("yyyyMMddHHmmssff");
            string guidPart = Guid.NewGuid().ToString("N");
            return datePart + guidPart;
        }
    }
}
