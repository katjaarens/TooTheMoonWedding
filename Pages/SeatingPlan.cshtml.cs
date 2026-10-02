using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace TooTheMoonWedding.Pages
{
    public class SeatingPlanModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SeatingPlanModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TableAssignmentModel> Assignments { get; set; } = new();
        public List<string> UnassignedGuests { get; set; } = new();

        [BindProperty]
        public string TableName { get; set; } = string.Empty;
        [BindProperty]
        public int Capacity { get; set; } = 8;

        // Hilfsmethode, um zu prüfen, ob der eingeloggte User zu den berechtigten Admins gehört
        private bool IsAdmin()
        {
            // Wir prüfen, ob überhaupt ein Admin eingeloggt ist ("IsAdmin" == "true")
            // UND ob der Name des Admins exakt zu unseren 4 Personen gehört.
            var isAdminLoggedIn = HttpContext.Session.GetString("IsAdmin") == "true";
            var adminName = HttpContext.Session.GetString("AdminName"); // Oder z.B. Session.GetString("AdminEmail")

            if (!isAdminLoggedIn || string.IsNullOrEmpty(adminName))
            {
                return false;
            }

            // Liste der erlaubten Admin-Namen (Groß-/Kleinschreibung egal dank StringComparison)
            var allowedAdmins = new[] { "Katja", "Andrea", "Heike", "Lulu" };

            return allowedAdmins.Any(name => name.Equals(adminName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task OnGetAsync()
        {
            Assignments = await _context.TableAssignments.ToListAsync();

            var confirmedRsvps = await _context.Rsvps
                .Where(r => r.IsAttending)
                .ToListAsync();

            var assignedNames = Assignments
                .Where(a => !string.IsNullOrEmpty(a.GuestName))
                .Select(a => a.GuestName)
                .ToHashSet();

            var unassignedList = new List<string>();

            foreach (var rsvp in confirmedRsvps)
            {
                if (!string.IsNullOrWhiteSpace(rsvp.Name) && !assignedNames.Contains(rsvp.Name))
                {
                    if (!unassignedList.Contains(rsvp.Name))
                        unassignedList.Add(rsvp.Name);
                }

                if (!string.IsNullOrWhiteSpace(rsvp.PartnerName) && !assignedNames.Contains(rsvp.PartnerName))
                {
                    if (!unassignedList.Contains(rsvp.PartnerName))
                        unassignedList.Add(rsvp.PartnerName);
                }
            }

            UnassignedGuests = unassignedList;
        }

        public async Task<IActionResult> OnPostAddTableAsync()
        {
            if (!IsAdmin()) return Forbid();

            if (!string.IsNullOrWhiteSpace(TableName))
            {
                bool tableExists = Assignments.Any(a => a.TableName == TableName);
                if (!tableExists)
                {
                    _context.TableAssignments.Add(new TableAssignmentModel
                    {
                        TableName = TableName,
                        GuestName = ""
                    });
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignAsync(string guestName, string targetTable)
        {
            if (!IsAdmin()) return Forbid();

            if (!string.IsNullOrWhiteSpace(guestName) && !string.IsNullOrWhiteSpace(targetTable))
            {
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

        public async Task<IActionResult> OnPostRemoveGuestAsync(int id)
        {
            if (!IsAdmin()) return Forbid();

            var item = await _context.TableAssignments.FindAsync(id);
            if (item != null)
            {
                item.GuestName = "";
                _context.TableAssignments.Update(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTableAsync(string tableName)
        {
            if (!IsAdmin()) return Forbid();

            if (!string.IsNullOrEmpty(tableName))
            {
                var tableItems = await _context.TableAssignments
                    .Where(t => t.TableName == tableName)
                    .ToListAsync();

                if (tableItems.Any())
                {
                    _context.TableAssignments.RemoveRange(tableItems);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }
    }
}