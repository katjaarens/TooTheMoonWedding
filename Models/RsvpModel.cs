public class RsvpModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsAttending { get; set; }

    // Als string definieren, damit "yes" / "no" problemlos angenommen wird
    public string? HasPartner { get; set; }
    public string? PartnerName { get; set; }
    public int? Kids { get; set; }

    // Als string definieren, damit Zahlen wie "2", "0" etc. nicht fehlschlagen
    public string? CateringOmnivore { get; set; }
    public string? CateringVegetarian { get; set; }
    public string? CateringVegan { get; set; }

    public string? Allergies { get; set; }
    public string? Song { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}