using System;
using System.ComponentModel.DataAnnotations;

public class GuestbookEntry
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte gib deinen Namen ein.")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Bitte schreibe eine Nachricht.")]
    [StringLength(1500, ErrorMessage = "Die Nachricht ist zu lang.")]
    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Optional: Falls du eine Moderation (Freigabe) einbauen möchtest
    public bool IsApproved { get; set; } = true; // Oder standardmäßig auf false, wenn ihr es erst prüfen wollt
}