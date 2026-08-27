namespace SefimMcp.Models;

public class ReservationList
{
    [PrimaryKey (IsIdentity = true)]
    public int Id { get; set; }

    public string? ReservationName { get; set; }

    public string? ReservationPhone { get; set; }

    public string? ReservationName2 { get; set; }

    public string? ReservationPhone2 { get; set; }

    public int? NumberOfPersons { get; set; }

    public int? Man { get; set; }

    public int? Woman { get; set; }

    public int? Child { get; set; }

    public DateTime? Date { get; set; }

    public string? StartTime { get; set; }

    public string? EndTime { get; set; }

    public int? TableGroupId { get; set; }

    public string? TableNumber { get; set; }

    public string? Note { get; set; }

    public bool Completed { get; set; }

    public bool IsSee { get; set; }

    public bool Confirm { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }

    public DateTime? Time { get; set; }

    public int? HeaderId { get; set; }

    public bool? RezervasyonInserted { get; set; }

    public bool? RezervasyonUpdated { get; set; }
}
