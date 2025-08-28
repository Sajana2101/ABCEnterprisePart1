using Microsoft.AspNetCore.Mvc;
using ABCEnterprisePart1.Services;

public class ContractsController : Controller
{
    private readonly FileService _fileService;
    private readonly QueueService _queueService;

    public ContractsController(FileService fileService, QueueService queueService)
    {
        _fileService = fileService;
        _queueService = queueService;
    }

    public async Task<IActionResult> Index()
    {
        var files = await _fileService.ListFilesAsync();
        return View(files); 
    }


    // GET: Documents/Create
    public IActionResult Create()
    {
        return View();
    }


    // POST: Documents/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file.";
            return View();
        }

       
        await _fileService.UploadAsync(file.FileName, file.OpenReadStream());

        
        await _queueService.SendMessageAsync($"File uploaded: {file.FileName}");

        TempData["ContractsMessage"] = "File uploaded successfully to abcfile and message sent to queue!";
        return RedirectToAction("Create");
    }
}
