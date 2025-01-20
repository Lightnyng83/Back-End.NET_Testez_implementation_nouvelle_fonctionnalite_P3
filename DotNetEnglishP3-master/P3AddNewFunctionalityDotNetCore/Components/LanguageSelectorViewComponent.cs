using Microsoft.AspNetCore.Mvc;
using P3Core.Models.Services;

namespace P3Core.Components
{
    public class LanguageSelectorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ILanguageService languageService)
        {
            return View(languageService);
        }
    }
}
