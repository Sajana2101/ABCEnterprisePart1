using ABCEnterprisePart1.Models;
using ABCEnterprisePart1.Services;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProductsController : Controller
{
    private readonly TableService _tableService;
    private readonly BlobService _blobService;
    private readonly QueueService _queueService;
    private const string TableName = "storagetable";
    private const string PartitionKey = "Products";

    public ProductsController(TableService tableService, BlobService blobService, QueueService queueService)
    {
        _tableService = tableService;
        _blobService = blobService;
        _queueService = queueService;
    }

    // GET: Products
    public IActionResult Index(string search)
    {
        var table = _tableService.GetTableClient(TableName);

        var products = table.Query<TableEntity>(p => p.PartitionKey == PartitionKey)
            .Select(e => new Product
            {
                RowKey = e.RowKey,
                ProductName = e.GetString("ProductName"),
                Description = e.GetString("Description"),
                Price = e.ContainsKey("Price") && e["Price"] != null
                    ? Convert.ToDecimal(e["Price"])
                    : 0,
                ImageURL = e.ContainsKey("ImageUrl") ? e.GetString("ImageUrl") : null
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            products = products
                .Where(p => !string.IsNullOrEmpty(p.ProductName) &&
                            p.ProductName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }




        return View(products);
    }


    // GET: Products/Details/5
    public IActionResult Details(string id)
    {
        if (id == null) return NotFound();

        var table = _tableService.GetTableClient(TableName);

        try
        {
            var entity = table.GetEntity<TableEntity>(PartitionKey, id);

            if (entity == null) return NotFound();

            decimal price = 0;

            if (entity.Value.ContainsKey("Price") && entity.Value["Price"] != null)
            {
                var priceValue = entity.Value["Price"];

                if (priceValue is int intVal)
                    price = intVal; // int → decimal
                else if (priceValue is double doubleVal)
                    price = Convert.ToDecimal(doubleVal); // double → decimal
            }

            var product = new Product
            {
                RowKey = entity.Value.RowKey,
                ProductName = entity.Value.GetString("ProductName") ?? "Unnamed Product",
                Description = entity.Value.GetString("Description") ?? "No description available",
                Price = price
            };

            return View(product);
        }
        catch (RequestFailedException)
        {
            return NotFound();
        }
    }


    // GET: Products/Create
    public IActionResult Create() => View();

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
    {
        foreach (var state in ModelState)
        {
            if (state.Value.Errors.Count > 0)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"{state.Key}: {error.ErrorMessage}");
                }
            }
        }


        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Debug"] = string.Join(" | ", errors);
            return View(model);
        }




        if (!ModelState.IsValid || string.IsNullOrEmpty(model.ProductName) || model.Price < 0)
        {
            TempData["Debug"] = "Model state invalid or missing data.";
            return View(model);
        }

        var table = _tableService.GetTableClient(TableName);
        if (table == null)
        {
            TempData["Debug"] = "Table client is null!";
            return View(model);
        }

        var rowKey = Guid.NewGuid().ToString();
        var partitionKey = "Products";

        var entity = new TableEntity(partitionKey, rowKey)
    {
        { "ProductName", model.ProductName },
        { "Description", model.Description ?? "" },

        { "Price", model.Price }
    };

        if (imageFile != null && imageFile.Length > 0)
        {
            using var stream = imageFile.OpenReadStream();
            entity["ImageUrl"] = await _blobService.UploadAsync($"{rowKey}_{imageFile.FileName}", stream);
        }

        try
        {
            await table.AddEntityAsync(entity);
        }
        catch (Exception ex)
        {
            TempData["Debug"] = $"Exception: {ex.Message}";
            return View(model);
        }

        await _queueService.SendMessageAsync("New Product Created");

        return RedirectToAction(nameof(Index));
    }



    // GET: Products/Edit/5
    public IActionResult Edit(string id)
    {
        if (id == null) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var product = new Product
        {
            RowKey = entity.Value.RowKey,
            ProductName = entity.Value.GetString("ProductName"),
            Description = entity.Value.GetString("Description"),
            Price = entity.Value.ContainsKey("Price") && entity.Value["Price"] != null
                    ? Convert.ToDecimal(entity.Value["Price"])
                    : 0,
            ImageURL = entity.Value.ContainsKey("ImageUrl") ? entity.Value.GetString("ImageUrl") : null
        };

        return View(product);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(model);

        var table = _tableService.GetTableClient(TableName);

        var entity = new TableEntity("Products", model.RowKey)
    {
        { "ProductName", model.ProductName },
        { "Description", model.Description ?? "" },
      
        { "Price", model.Price }
    };

        if (imageFile != null && imageFile.Length > 0)
        {
            using var stream = imageFile.OpenReadStream();
            entity["ImageUrl"] = await _blobService.UploadAsync($"{model.RowKey}_{imageFile.FileName}", stream);
        }

        await table.UpsertEntityAsync(entity);
        await _queueService.SendMessageAsync("Product Updated");

        return RedirectToAction(nameof(Index));
    }


    // GET: Products/Delete/5
    public IActionResult Delete(string id)
    {
        if (id == null) return NotFound();

        var table = _tableService.GetTableClient(TableName);
        var entity = table.GetEntity<TableEntity>(PartitionKey, id);

        if (entity == null) return NotFound();

        var product = new Product
        {
            RowKey = entity.Value.RowKey,
            ProductName = entity.Value.GetString("ProductName"),
            Description = entity.Value.GetString("Description"),
            Price = entity.Value.ContainsKey("Price") && entity.Value["Price"] != null
                    ? Convert.ToDecimal(entity.Value["Price"])
                    : 0,
            ImageURL = entity.Value.ContainsKey("ImageUrl") ? entity.Value.GetString("ImageUrl") : null
        };

        return View(product);
    }


    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var table = _tableService.GetTableClient(TableName);
        await table.DeleteEntityAsync(PartitionKey, id);

        await _queueService.SendMessageAsync("Product Deleted");

        return RedirectToAction(nameof(Index));
    }
}
