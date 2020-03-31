using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToDoListMaterialDesign.Models.DbAccess;
using ToDoListMaterialDesign.Models.Entities;
using ToDoListMaterialDesign.Models.Interface;

namespace ToDoListMaterialDesign.Models.Logics
{
    class EditViewSearch
    {
        public struct Parameter
        {
            public string SearchConditionsText { get; set; }
            public DateTime? SearchConditionsDateFrom { get; set; }
            public DateTime? SearchConditionsDateTo { get; set; }
        }

        public struct Result
        {
            public List<TodoTask> TodoTasks { get; set; }
        }

        public IResponse DoProcess(IRequest _request)
        {
            IResponse response = new Response();
            var parameter = (Parameter)_request.Parameter;
            var result = new Result();

            var entities = new List<TodoTask>();
            using (var context = new EfDbContext())
            {
                IQueryable<TodoTask> query = context.TodoTasks.OrderBy(o => o.DueDate).ThenBy(o => o.TodoTaskId);

                if (!(string.IsNullOrEmpty(parameter.SearchConditionsText)))
                {
                    query = query.Where(w => w.Subject.Contains(parameter.SearchConditionsText));
                }
                if (parameter.SearchConditionsDateFrom.HasValue)
                {
                    query = query.Where(w => (parameter.SearchConditionsDateFrom.Value.Date <= w.DueDate.Value.Date));
                }
                if (parameter.SearchConditionsDateTo.HasValue)
                {
                    query = query.Where(w => (w.DueDate.Value.Date <= parameter.SearchConditionsDateTo.Value.Date));
                }

                entities = query.ToList();
            }
            result.TodoTasks = entities;

            response.Result = result;
            response.IsSucceed = true;
            return response;
        }

    }
}
