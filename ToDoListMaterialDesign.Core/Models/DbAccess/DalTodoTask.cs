using System.Collections.Generic;
using System.Linq;
using ToDoListMaterialDesign.Models.Entities;

namespace ToDoListMaterialDesign.Models.DbAccess
{
    public static class DalTodoTask
    {
        public static int Add(TodoTask _entity)
        {
            int addCount = 0;
            using (var context = new EfDbContext())
            {
                TodoTask entity = new TodoTask() { TodoTaskId = EfDbContext.GenerateId() };
                entity.CopyValuesFrom(_entity);
                context.Add(entity);
                addCount = context.SaveChanges();
                _entity.TodoTaskId = entity.TodoTaskId;
            }
            return addCount;
        }

        public static TodoTask CopyValuesFrom(this TodoTask _copyTo, TodoTask _copyFrom)
        {
            foreach (var property in _copyTo.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;
                switch (property.Name)
                {
                    case nameof(_copyTo.TodoTaskId):
                    case nameof(_copyTo.CreateDateTime):
                    case nameof(_copyTo.UpdateDateTime):
                        // Skip Copy Value
                        break;
                    default:
                        property.SetValue(_copyTo, property.GetValue(_copyFrom));
                        break;
                }
            }
            return _copyTo;
        }

        public static TodoTask Find(string _id)
        {
            TodoTask entity = null;
            using (var context = new EfDbContext())
            {
                entity = context.TodoTasks.Find(_id);
            }
            return entity;
        }

        public static int Update(TodoTask _entity)
        {
            int updateCount = 0;
            using (var context = new EfDbContext())
            {
                var entity = context.TodoTasks.Find(_entity.TodoTaskId);
                if (entity != null)
                {
                    entity.CopyValuesFrom(_entity);
                    updateCount = context.SaveChanges();
                }
            }
            return updateCount;
        }

        public static int Upsert(TodoTask _entity)
        {
            int upsertCount = 0;
            using (var context = new EfDbContext())
            {
                var entity = context.TodoTasks.Find(_entity.TodoTaskId);
                if (entity == null)
                {
                    entity = new TodoTask() { TodoTaskId = _entity.TodoTaskId ?? EfDbContext.GenerateId() };
                    entity.CopyValuesFrom(_entity);
                    context.Add(entity);
                    _entity.TodoTaskId = entity.TodoTaskId;
                }
                else
                {
                    entity.CopyValuesFrom(_entity);
                }
                upsertCount = context.SaveChanges();
            }
            return upsertCount;
        }

        public static int Delete(string _id)
        {
            int deleteCount = 0;
            using (var context = new EfDbContext())
            {
                var entity = context.TodoTasks.Find(_id);
                if (entity != null)
                {
                    context.TodoTasks.Remove(entity);
                    deleteCount = context.SaveChanges();
                }
            }
            return deleteCount;
        }

        public static List<TodoTask> SelectAll()
        {
            var entities = new List<TodoTask>();
            using (var context = new EfDbContext())
            {
                entities = context.TodoTasks.OrderBy(o => o.DueDate).ThenBy(o => o.TodoTaskId).ToList();
            }
            return entities;
        }

    }
}
