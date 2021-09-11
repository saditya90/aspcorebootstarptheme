using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BootstrapThemes.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SemaphoreSlim _initializingSemaphore = new SemaphoreSlim(1);
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IndexModel> _logger;
        private readonly IMemoryCache _cache;
        private const string ThemeCacheKey = "themecolorskey";

        public IndexModel(ILogger<IndexModel> logger,
            IWebHostEnvironment env,
            IMemoryCache cache)
        {
            _logger = logger;
            _env = env;
            _cache = cache;
        }

        [BindProperty]
        public Input ModelInput { get; set; } = new Input();

        public class Input
        {
            public string Primary { get; set; } = "#a0b8de";
            public string Secondary { get; set; } = "#6c757d";
            public string Danger { get; set; } = "#dc3545";
            public string Success { get; set; } = "#28a745";
            public string Warning { get; set; } = "#ffc107";
            public string Info { get; set; } = "#17a2b8";
        }

        public void OnGet()
        {
            //TODO: Should query actual data source.
            if (_cache.TryGetValue(ThemeCacheKey, out Input value))
                ModelInput = value;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                await _initializingSemaphore.WaitAsync();

                try
                {
                    var scssContent = System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "css", "initial_scss.txt"));
                    var newContent = scssContent.Replace("prmcolor", ModelInput.Primary);
                    newContent = newContent.Replace("seccolor", ModelInput.Secondary);
                    newContent = newContent.Replace("dancolor", ModelInput.Danger);
                    newContent = newContent.Replace("succolor", ModelInput.Success);
                    newContent = newContent.Replace("warcolor", ModelInput.Warning);
                    newContent = newContent.Replace("infcolor", ModelInput.Info);
                    System.IO.File.WriteAllText(Path.Combine(_env.WebRootPath, "css", "bootstrap.custom.scss"), newContent);
                    //TODO: Should add or update datasource.
                    _cache.Set(ThemeCacheKey, ModelInput);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception.Message);
                }
                finally
                {
                    _initializingSemaphore.Release();
                }
            }
            return RedirectToPage();
        }
    }
}
