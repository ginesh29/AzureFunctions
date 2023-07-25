using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using HttpTrigger.Entities;
using HttpTrigger.ViewModels;

namespace HttpTrigger
{
    public class HttpTriggerFunction
    {
        private const string Route = "todo";
        private readonly AppDbContext _context;
        public HttpTriggerFunction(AppDbContext context)
        {
            _context = context;
        }

        [FunctionName("GetAllTodoItems")]
        [OpenApiOperation(operationId: "RunGet", tags: new[] { "Todo" })]
        public async Task<IActionResult> GetAllTodoItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var todoItems = await _context.Todos.ToListAsync();
            return new OkObjectResult(todoItems);
        }

        [FunctionName("GetTodoItemById")]
        [OpenApiOperation(operationId: "RunGetById", tags: new[] { "Todo" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        public async Task<IActionResult> GetTodoItemById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route + "/{id}")] HttpRequest req,
            ILogger log, Guid id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo);
        }

        [FunctionName("PostTodoItem")]
        [OpenApiOperation(operationId: "RunPost", tags: new[] { "Todo" })]
        [OpenApiRequestBody(contentType: "text/json", bodyType: typeof(ToDoCreateModel))]
        public async Task<IActionResult> PostTodoItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Route)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);
            var todo = new ToDo { TaskDescription = input.TaskDescription };
            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
            return new CreatedAtRouteResult("GetTodoItemById", routeValues: new { id = todo.Id }, value: todo);
        }

        [FunctionName("UpdateTodoItem")]
        [OpenApiOperation(operationId: "RunPut", tags: new[] { "Todo" })]
        [OpenApiRequestBody(contentType: "text/json", bodyType: typeof(ToDoUpdateModel))]
        public async Task<IActionResult> UpdateTodoItem(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Updating a todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoUpdateModel>(requestBody);
            var todo = await _context.Todos.FindAsync(input.Id);
            if (todo == null)
            {
                log.LogWarning($"Item {input.Id} not found");
                return new NotFoundResult();
            }

            todo.IsCompleted = input.IsCompleted;
            if (!string.IsNullOrEmpty(input.TaskDescription))
                todo.TaskDescription = input.TaskDescription;
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        [FunctionName("DeleteTodoItem")]
        [OpenApiOperation(operationId: "RunDelete", tags: new[] { "Todo" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        public async Task<IActionResult> DeleteTodoItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Route + "/{id}")] HttpRequest req,
            ILogger log, Guid id)
        {
            log.LogInformation("Deleting a todo list item");
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                log.LogWarning($"Item {id} not found");
                return new NotFoundResult();
            }
            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}
