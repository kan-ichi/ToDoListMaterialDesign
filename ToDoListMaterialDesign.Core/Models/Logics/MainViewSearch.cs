using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToDoListMaterialDesign.Models.Codes;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;
using ToDoListMaterialDesign.Models.Interface;

namespace ToDoListMaterialDesign.Models.Logics
{
    class MainViewSearch
    {
        public struct Result
        {
            public List<TodoTask> TodoTasks { get; set; }
        }

        public IResponse DoProcess(IRequest _request)
        {
            IResponse response = new Response();
            var result = new Result();

            var entities = new List<TodoTask>();
            using (var context = new EfDbContext())
            {
                IQueryable<TodoTask> query = context.TodoTasks.OrderBy(o => o.DueDate).ThenBy(o => o.TodoTaskId);
                query = query.Where(w => w.StatusCode != StatusCode.CODE_FINISHED);
                entities = query.ToList();
            }
            result.TodoTasks = entities;

            response.Result = result;
            response.IsSucceed = true;
            return response;
        }

    }
}
