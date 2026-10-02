using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TooTheMoonWedding.Pages
{
    public class AdminLoginModel : PageModel
    {
        [BindProperty]
        public string AdminName { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public IActionResult OnPost()
        {
            var nameToCheck = AdminName?.Trim().ToLower() ?? string.Empty;

            bool isValid = (nameToCheck, Password) switch
            {
                ("andrea", "Test123") => true,
                ("katja", "Test123") => true,
                ("heike", "Test123") => true,
                ("lulu", "Test123") => true,
                _ => false
            };

            if (isValid)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminName", AdminName.Trim()); 
                
                // Hier angepasst auf die neue kombinierte Admin-Seite:
                return RedirectToPage("/Admin");
            }

            ErrorMessage = "Ungültiger Name oder falsches Passwort!";
            return Page();
        }
    }
}