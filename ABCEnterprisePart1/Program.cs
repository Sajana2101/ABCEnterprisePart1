using ABCEnterprisePart1.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
var blobContainerName = builder.Configuration["AzureStorage:BlobContainerName"];
var queueName = builder.Configuration["AzureStorage:QueueName"];
var fileShareName = builder.Configuration["AzureStorage:FileShareName"];

builder.Services.AddSingleton<TableService>(sp =>
    new TableService(connectionString));

builder.Services.AddSingleton<BlobService>(sp =>
    new BlobService(connectionString, blobContainerName));

builder.Services.AddSingleton<QueueService>(sp =>
    new QueueService(connectionString, queueName));

builder.Services.AddSingleton<FileService>(sp =>
    new FileService(connectionString, fileShareName));

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();