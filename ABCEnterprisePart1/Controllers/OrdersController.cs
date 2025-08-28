
using ABCEnterprisePart1.Models;
using ABCEnterprisePart1.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



public class OrdersController : Controller
{
    private readonly TableService _tableService;
    private readonly QueueService _queueService;
    private const string TableName = "orderstable";
    private const string PartitionKey = "Orders";

    public OrdersController(TableService tableService, QueueService queueService)
    {
        _tableService = tableService;
        _queueService = queueService;
    }

   
    public IActionResult Index()
    {
        var table = _tableService.GetTableClient(TableName);

        var orders = table.Query<TableEntity>(o => o.PartitionKey == PartitionKey)
            .Select(e => new Order
            {
                RowKey = e.RowKey,
                CustomerName = e.GetString("CustomerName"),
            
                ProductName = e.GetString("ProductName")
            })
            .ToList();

        return View(orders);
    }


    private void PopulateDropdowns()
    {
        var customerTable = _tableService.GetTableClient("customertable");
        var customers = customerTable.Query<TableEntity>(c => c.PartitionKey == "Customers")
            .Select(e => e.GetString("CustomerName"))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();
        ViewBag.CustomerList = new SelectList(customers);
        var productTable = _tableService.GetTableClient("storagetable");
        var products = productTable.Query<TableEntity>(p => p.PartitionKey == "Products")
            .Select(e => e.GetString("ProductName"))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();
        ViewBag.ProductList = new SelectList(products);
    }

  
    public IActionResult Details(string id)
    {
        if (id == null) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var order = new Order
        {
            RowKey = entity.Value.RowKey,
           
            CustomerName = entity.Value.GetString("CustomerName"),
         
            ProductName = entity.Value.GetString("ProductName")
        };

        return View(order);
    }

    // GET: Orders/Create
    public IActionResult Create()
    {
        PopulateDropdowns(); 
        return View();
    }

    // POST: Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    
    public async Task<IActionResult> Create(Order model)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(model.CustomerName) || string.IsNullOrEmpty(model.ProductName))
        {
            PopulateDropdowns();
            return View(model);
        }

        var table = _tableService.GetTableClient("orderstable");
        var orderId = Guid.NewGuid().ToString();

      

        var entity = new TableEntity(PartitionKey, orderId)  
{
    { "CustomerName", model.CustomerName },
    { "ProductName", model.ProductName }
};


        await table.AddEntityAsync(entity);

        await _queueService.SendMessageAsync($"Order Created: {model.CustomerName} - {model.ProductName}");

        TempData["Message"] = "Order created successfully!";
        return RedirectToAction(nameof(Index));
    }


    // GET: Orders/Edit/5
    public IActionResult Edit(string id)
    {
        if (id == null) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var order = new Order
        {
            RowKey = entity.Value.RowKey,
        
            CustomerName = entity.Value.GetString("CustomerName"),
          
            ProductName = entity.Value.GetString("ProductName")
        };

        PopulateDropdowns(); 
        return View(order);
    }

  
    // POST: Orders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Order model)
    {
        if (!ModelState.IsValid)
        {
            PopulateDropdowns(); 
            return View(model);
        }

        var table = _tableService.GetTableClient(TableName);

        var entity = new TableEntity(PartitionKey, model.RowKey)
        {
          
            { "CustomerName", model.CustomerName ?? string.Empty },
            { "ProductName", model.ProductName ?? string.Empty }
        };

        await table.UpsertEntityAsync(entity);
        TempData["Message"] = "Order updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Orders/Delete/5
    public IActionResult Delete(string id)
    {
        if (id == null) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var order = new Order
        {
            RowKey = entity.Value.RowKey,
          
            CustomerName = entity.Value.GetString("CustomerName"),
         
            ProductName = entity.Value.GetString("ProductName")
        };

        return View(order);
    }

    // POST: Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var table = _tableService.GetTableClient(TableName);
        await table.DeleteEntityAsync(PartitionKey, id);

        TempData["OrdersMessage"] = "Order deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}