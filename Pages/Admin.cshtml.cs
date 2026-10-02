using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace TooTheMoonWedding.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RsvpModel> Rsvps { get; set; } = new();
        public List<TableAssignmentModel> Assignments { get; set; } = new();
        public List<string> UnassignedGuests { get; set; } = new();

        [BindProperty]
        public string TableName { get; set; } = string.Empty;
        [BindProperty]
        public int Capacity { get; set; } = 8;

        public string LoggedInAdminName { get; set; } = string.Empty;

        private bool IsAdmin()
        {
            var isAdminLoggedIn = HttpContext.Session.GetString("IsAdmin") == "true";
            var adminName = HttpContext.Session.GetString("AdminName");

            if (!isAdminLoggedIn || string.IsNullOrEmpty(adminName))
                return false;

            var allowedAdmins = new[] { "Katja", "Andrea", "Heike", "Lulu" };
            return allowedAdmins.Any(name => name.Equals(adminName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdmin())
            {
                return RedirectToPage("/AdminLogin");
            }

            LoggedInAdminName = HttpContext.Session.GetString("AdminName") ?? "Admin";

            // Daten laden
            Rsvps = await _context.Rsvps.ToListAsync();
            Assignments = await _context.TableAssignments.ToListAsync();

            // Unzugewiesene Gäste ermitteln
            var confirmedRsvps = Rsvps.Where(r => r.IsAttending).ToList();
            var assignedNames = Assignments
                .Where(a => !string.IsNullOrEmpty(a.GuestName))
                .Select(a => a.GuestName)
                .ToHashSet();

            var unassignedList = new List<string>();
            foreach (var rsvp in confirmedRsvps)
            {
                if (!string.IsNullOrWhiteSpace(rsvp.Name) && !assignedNames.Contains(rsvp.Name))
                {
                    if (!unassignedList.Contains(rsvp.Name)) unassignedList.Add(rsvp.Name);
                }
                if (!string.IsNullOrWhiteSpace(rsvp.PartnerName) && !assignedNames.Contains(rsvp.PartnerName))
                {
                    if (!unassignedList.Contains(rsvp.PartnerName)) unassignedList.Add(rsvp.PartnerName);
                }
            }
            UnassignedGuests = unassignedList;

            return Page();
        }

        // --- RSVP Aktionen ---
        public async Task<IActionResult> OnPostDeleteRsvpAsync(int id)
        {
            if (!IsAdmin()) return Forbid();

            var rsvp = await _context.Rsvps.FindAsync(id);
            if (rsvp != null)
            {
                _context.Rsvps.Remove(rsvp);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // --- Sitzplan Aktionen ---
        public async Task<IActionResult> OnPostAddTableAsync()
        {
            if (!IsAdmin()) return Forbid();

            if (!string.IsNullOrWhiteSpace(TableName))
            {
                var existing = await _context.TableAssignments.AnyAsync(a => a.TableName == TableName);
                if (!existing)
                {
                    _context.TableAssignments.Add(new TableAssignmentModel { TableName = TableName, GuestName = "" });
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
                    _context.TableAssignments.Add(new TableAssignmentModel { TableName = targetTable, GuestName = guestName });
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
                var items = await _context.TableAssignments.Where(t => t.TableName == tableName).ToListAsync();
                if (items.Any())
                {
                    _context.TableAssignments.RemoveRange(items);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }

        // --- Logout ---
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/AdminLogin");
        }
    }
}