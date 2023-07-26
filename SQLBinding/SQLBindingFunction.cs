using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureFunctions.Entities;
using AzureFunctions.Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace SQLBinding
{
    public class SQLBindingFunction
    {
        private const string Route = "todo";
        [FunctionName("GetAllTodoItems")]
        [OpenApiOperation(tags: new[] { "Todo" })]
        public IActionResult GetAllTodoItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route)] HttpRequest req,
            [Sql("SELECT * FROM [dbo].[ToDos]",
            "SqlConnectionString")] IEnumerable<Object> toDoItems,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            return new OkObjectResult(toDoItems);
        }

        [FunctionName("GetTodoItemById")]
        [OpenApiOperation(tags: new[] { "Todo" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(Guid))]
        public IActionResult GetTodoItemById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetTodoItemById")] HttpRequest req,
            [Sql("SELECT * FROM [dbo].[ToDos] where Id = @Id",
            connectionStringSetting: "SqlConnectionString",
            commandType: System.Data.CommandType.Text,
            parameters: "@Id={Query.id}"
            )] IEnumerable<Object> toDoItems,
            ILogger log)
        {
            var todoItem = toDoItems?.FirstOrDefault();
            if (todoItem == null)
            {
                var id = req.Query["Id"];
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todoItem);
        }
        [FunctionName("PostTodoItem")]
        [OpenApiOperation(tags: new[] { "Todo" })]
        [OpenApiRequestBody(contentType: "text/json", bodyType: typeof(ToDoCreateModel))]
        public async Task<IActionResult> PostTodoItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Route)] HttpRequest req,
            [Sql("[dbo].[ToDos]", "SqlConnectionString")] IAsyncCollector<ToDo> todos,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);
            ToDo todo = new ToDo
            {
                TaskDescription = input.TaskDescription
            };
            await todos.AddAsync(todo);
            return new CreatedAtRouteResult("GetTodoItemById", new { id = todo.Id }, todo);
        }

        [FunctionName("UpdateTodoItem")]
        [OpenApiOperation(tags: new[] { "Todo" })]
        [OpenApiRequestBody(contentType: "text/json", bodyType: typeof(ToDoUpdateModel))]
        public async Task<IActionResult> PutTodoItem(
             [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route)] HttpRequest req,
             [Sql("[dbo].[ToDos]", "SqlConnectionString")] IAsyncCollector<ToDo> todos,
             ILogger log)
        {
            log.LogInformation("Updating a todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            List<ToDo> incomingToDoItems = JsonConvert.DeserializeObject<List<ToDo>>(requestBody);
            ToDo toDoItem = incomingToDoItems[0];
            ToDo newToDoItem = incomingToDoItems[1];
            if (toDoItem.Id != newToDoItem.Id)
            {
                log.LogWarning($"Item {toDoItem.Id} not found");
                return new NotFoundResult();
            }
            if (!string.IsNullOrEmpty(newToDoItem.TaskDescription))
                toDoItem.TaskDescription = newToDoItem.TaskDescription;
            toDoItem.IsCompleted = newToDoItem.IsCompleted;
            await todos.AddAsync(toDoItem);
            return new NoContentResult();
        }
        [FunctionName("DeleteTodoItem")]
        [OpenApiOperation(tags: new[] { "Todo" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(Guid))]
        public IActionResult DeleteTodoItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Route)] HttpRequest req,
            [Sql(commandText: "DeleteToDo", commandType: System.Data.CommandType.StoredProcedure,
                parameters: "@Id={Query.id}", connectionStringSetting: "SqlConnectionString")]
                IEnumerable<ToDo> toDoItems,
                ILogger log)
        {
            log.LogInformation("Deleting a todo list item");
            return new NoContentResult();
        }
    }
}
