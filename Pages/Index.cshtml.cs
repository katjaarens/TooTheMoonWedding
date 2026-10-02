using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public RsvpModel Rsvp { get; set; } = default!;

    [TempData]
    public string SuccessMessage { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // Wenn Validierungsfehler vorliegen, geben wir diese nun direkt als Nachricht aus, 
        // anstatt dass die Seite einfach stumm stehen bleibt.
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            TempData["SuccessMessage"] = "Validierungsfehler: " + (string.IsNullOrEmpty(errors) ? "Bitte alle Pflichtfelder ausfüllen." : errors);
            return RedirectToPage();
        }

        try
        {
            Rsvp.CreatedAt = DateTime.Now;
            _context.Rsvps.Add(Rsvp);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Vielen Dank! Deine Anmeldung wurde erfolgreich übermittelt.";
        }
        catch (Exception ex)
        {
            TempData["SuccessMessage"] = "Fehler beim Speichern: " + ex.Message;
        }

        return RedirectToPage();
    }
}