using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}
public class StatisticsModel : PageModel
{
    /// <summary>Текущий год в двухзначном формате (как хранит БД: 26, 25...)</summary>
    public int DefaultYear { get; private set; }

    public void OnGet()
    {
        DefaultYear = DateTime.Today.Year % 100;
    }
}