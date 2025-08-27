using ABCEnterprisePart1.Models;
using ABCEnterprisePart1.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CustomersController : Controller
{
    private readonly TableService _tableService;
    
    private readonly QueueService _queueService;

    // Use the same table name pattern you used for Products
    private const string TableName = "customertable";
    private const string PartitionKey = "Customers";

    public CustomersController(TableService tableService, QueueService queueService)
    {
        _tableService = tableService;
       
        _queueService = queueService;
    }

    // GET: Customers
    public IActionResult Index()
    {
        var table = _tableService.GetTableClient(TableName);

        var customers = table.Query<TableEntity>(c => c.PartitionKey == PartitionKey)
            .Select(e => new Customer
            {
                RowKey = e.RowKey,
                CustomerName = e.GetString("CustomerName"),
                CustomerPhone = e.GetString("CustomerPhone"),
                CustomerEmail = e.GetString("CustomerEmail"),
               // ImageURL = e.ContainsKey("ImageUrl") ? e.GetString("ImageUrl") : null
            })
            .ToList();

        return View(customers);
    }

    // GET: Customers/Details/5
    public IActionResult Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var customer = new Customer
        {
            RowKey = entity.Value.RowKey,
            CustomerName = entity.Value.GetString("CustomerName"),
            CustomerPhone = entity.Value.GetString("CustomerPhone"),
            CustomerEmail = entity.Value.GetString("CustomerEmail"),
          //  ImageURL = entity.Value.ContainsKey("ImageUrl") ? entity.Value.GetString("ImageUrl") : null
        };

        return View(customer);
    }

    // GET: Customers/Create
    public IActionResult Create() => View();

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.CustomerName))
            return View(model);

        var table = _tableService.GetTableClient(TableName);

        var rowKey = Guid.NewGuid().ToString();
        var entity = new TableEntity(PartitionKey, rowKey)
        {
            { "CustomerName",  model.CustomerName ?? "" },
            { "CustomerPhone", model.CustomerPhone ?? "" },
            { "CustomerEmail", model.CustomerEmail ?? "" }
        };

        
       
        await table.AddEntityAsync(entity);

        // Queue message
        await _queueService.SendMessageAsync($"Customer Created");

        TempData["Message"] = "Customer created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Edit/5
    public IActionResult Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var customer = new Customer
        {
            RowKey = entity.Value.RowKey,
            CustomerName = entity.Value.GetString("CustomerName"),
            CustomerPhone = entity.Value.GetString("CustomerPhone"),
            CustomerEmail = entity.Value.GetString("CustomerEmail"),
           // ImageURL = entity.Value.ContainsKey("ImageUrl") ? entity.Value.GetString("ImageUrl") : null
        };

        return View(customer);
    }

    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(model.RowKey))
            return View(model);

        var table = _tableService.GetTableClient(TableName);

        var entity = new TableEntity(PartitionKey, model.RowKey)
        {
            { "CustomerName",  model.CustomerName ?? "" },
            { "CustomerPhone", model.CustomerPhone ?? "" },
            { "CustomerEmail", model.CustomerEmail ?? "" }
        };

        // If a new image is uploaded, replace URL
       

        await table.UpsertEntityAsync(entity);
        await _queueService.SendMessageAsync($"Customer Updated");

        TempData["Message"] = "Customer updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Delete/5
    public IActionResult Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var customer = new Customer
        {
            RowKey = entity.Value.RowKey,
            CustomerName = entity.Value.GetString("CustomerName"),
            CustomerPhone = entity.Value.GetString("CustomerPhone"),
            CustomerEmail = entity.Value.GetString("CustomerEmail"),
           
        };

        return View(customer);
    }

    // POST: Customers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var table = _tableService.GetTableClient(TableName);
        await table.DeleteEntityAsync(PartitionKey, id);

        await _queueService.SendMessageAsync($"Customer Deleted");

        TempData["Message"] = "Customer deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
