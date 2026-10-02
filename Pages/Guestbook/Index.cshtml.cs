using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TooTheMoonWedding.Pages.Guestbook
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<GuestbookEntry> Entries { get; set; } = new List<GuestbookEntry>();

        [BindProperty]
        public GuestbookEntry NewEntry { get; set; } = new();

        // Prüft, ob der User als Admin eingeloggt ist (über das Cookie-System)
        public bool IsAdmin => User.Identity != null && User.Identity.IsAuthenticated;

        [BindProperty]
        public string AdminPassword { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            Entries = await _context.GuestbookEntries
                .Where(e => e.IsApproved)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        // Passwort-Überprüfung beim Login
        public async Task<IActionResult> OnPostLoginAsync()
        {
            // HIER DEIN PASSWORT ÄNDERN:
            string geheimPasswort = "TooTheMoon2028!"; 

            if (AdminPassword == geheimPasswort)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Admin") };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            }

            return RedirectToPage();
        }

        // Admin-Modus wieder ausschalten (Logout)
        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage();
        }

        // Methode zum Löschen eines Eintrags
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAdmin) return RedirectToPage();

            var entry = await _context.GuestbookEntries.FindAsync(id);
            
            if (entry != null)
            {
                _context.GuestbookEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Wird beim Absenden des Gästebuch-Formulars aufgerufen
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Entries = await _context.GuestbookEntries
                    .Where(e => e.IsApproved)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
                return Page();
            }

            NewEntry.CreatedAt = DateTime.Now;
            NewEntry.IsApproved = true; 
            
            _context.GuestbookEntries.Add(NewEntry);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}