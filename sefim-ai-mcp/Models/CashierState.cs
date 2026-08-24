using System;
using System.Collections.Generic;

namespace SefimMcp.Models;

public partial class CashierState
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string CashierType { get; set; } = null!;

    public DateTime Date { get; set; }

    public decimal Cash { get; set; }

    public decimal CreditCard { get; set; }

    public decimal Ticket { get; set; }

    public bool Aktarildi { get; set; }

    public bool? IsSynced { get; set; }

    public bool? IsUpdated { get; set; }
}
