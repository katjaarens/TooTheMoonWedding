public class TableAssignmentModel
{
    public int Id { get; set; }
    public string GuestName { get; set; } = string.Empty; // Name des Gastes
    public string TableName { get; set; } = string.Empty; // z.B. "Brauttisch" oder "Tisch 3"
    public string? Notes { get; set; } // Optional: z.B. "Sonderwunsch"
}