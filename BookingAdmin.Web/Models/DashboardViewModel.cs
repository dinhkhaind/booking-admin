namespace BookingAdmin.Web.Models;

public class DashboardViewModel
{
    public int BoatId { get; set; }
    public string? BoatName { get; set; }
    public string Period { get; set; } = "week";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<string> PeriodLabels { get; set; } = new();
    public string RevenueByCurrency { get; set; } = string.Empty;

    public List<EmployeeSalesRow> EmployeeSalesData { get; set; } = new();
    public List<EmployeeSalesRow> EmployeeCancellationData { get; set; } = new();
    public List<ChannelSummaryRow> ChannelSummaryData { get; set; } = new();
    public List<EmployeeLastminRow> EmployeeLastminData { get; set; } = new();
    public List<ChannelLastminRow> ChannelLastminData { get; set; } = new();
}

public class EmployeeSalesRow
{
    public string EmployeeName { get; set; } = string.Empty;
    // PeriodSales[period_label] = {currency_code: amount}
    public Dictionary<string, Dictionary<string, decimal>> PeriodSales { get; set; } = new();
    public Dictionary<string, decimal> TotalByCurrency { get; set; } = new();
    public decimal Total { get; set; }
}

public class ChannelSummaryRow
{
    public string ChannelName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public Dictionary<string, decimal> TotalSalesByCurrency { get; set; } = new();
    public decimal TotalSales { get; set; }
    public int TotalCustomers { get; set; }
    public int CancelledRooms { get; set; }
    public Dictionary<string, decimal> CancelledSalesByCurrency { get; set; } = new();
    public decimal CancelledSales { get; set; }
    public int CancelledCustomers { get; set; }
}

public class EmployeeLastminRow
{
    public string EmployeeName { get; set; } = string.Empty;
    public int Rooms1Day { get; set; }
    public Dictionary<string, decimal> Sales1DayByCurrency { get; set; } = new();
    public decimal Sales1Day { get; set; }
    public int Rooms3Days { get; set; }
    public Dictionary<string, decimal> Sales3DaysByCurrency { get; set; } = new();
    public decimal Sales3Days { get; set; }
}

public class ChannelLastminRow
{
    public string ChannelName { get; set; } = string.Empty;
    public int Rooms1Day { get; set; }
    public Dictionary<string, decimal> Sales1DayByCurrency { get; set; } = new();
    public decimal Sales1Day { get; set; }
    public int Rooms3Days { get; set; }
    public Dictionary<string, decimal> Sales3DaysByCurrency { get; set; } = new();
    public decimal Sales3Days { get; set; }
}
