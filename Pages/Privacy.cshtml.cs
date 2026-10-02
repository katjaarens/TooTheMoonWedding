using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class SeatingPlanModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public SeatingPlanModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<TableAssignmentModel> Assignments { get; set; } = new();
    public List<RsvpModel> UnassignedGuests { get; set; } = new();

    [BindProperty]
    public string TableName { get; set; } = string.Empty;
    [BindProperty]
    public int Capacity { get; set; } = 8;

    public async Task OnGetAsync()
    {
        // Alle Tisch-Zuweisungen laden
        Assignments = await _context.TableAssignments.ToListAsync();

        // Alle zugesagten Gäste aus der RSVP-Tabelle laden
        var confirmedRsvps = await _context.Rsvps
            .Where(r => r.IsAttending)
            .ToListAsync();

        // Alle Namen, die bereits einem Tisch zugewiesen wurden
        var assignedNames = Assignments.Select(a => a.GuestName).ToHashSet();

        // Unzugewiesene Gäste filtern (wer noch auf keinem Tisch sitzt)
        UnassignedGuests = confirmedRsvps
            .Where(r => !assignedNames.Contains(r.Name))
            .ToList();
    }

    // Einen neuen Tisch (als leeren Platzhalter) im Raum anlegen
    public async Task<IActionResult> OnPostAddTableAsync()
    {
        if (!string.IsNullOrEmpty(TableName))
        {
            // Wir legen einen Dummy-Eintrag an, damit der Tisch im Raum sichtbar wird
            _context.TableAssignments.Add(new TableAssignmentModel
            {
                TableName = TableName,
                GuestName = "" // Leer, damit es nicht als echter Gast zählt
            });
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    // Gast einem Tisch zuweisen
    public async Task<IActionResult> OnPostAssignAsync(string guestName, string targetTable)
    {
        if (!string.IsNullOrEmpty(guestName) && !string.IsNullOrEmpty(targetTable))
        {
            // Falls für den Tisch bisher nur ein leerer Platzhalter existierte, können wir diesen nutzen oder einen neuen anlegen
            var placeholder = await _context.TableAssignments
                .FirstOrDefaultAsync(t => t.TableName == targetTable && string.IsNullOrEmpty(t.GuestName));

            if (placeholder != null)
            {
                placeholder.GuestName = guestName;
                _context.TableAssignments.Update(placeholder);
            }
            else
            {
                _context.TableAssignments.Add(new TableAssignmentModel
                {
                    TableName = targetTable,
                    GuestName = guestName
                });
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    // Gast von Tisch entfernen / Tisch löschen
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _context.TableAssignments.FindAsync(id);
        if (item != null)
        {
            _context.TableAssignments.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}