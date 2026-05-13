namespace BookingAdmin.Web.Models;

public class RoomScheduleViewModel
{
    public int SelectedBoatId { get; set; }
    public int SelectedMonth { get; set; }
    public int SelectedYear { get; set; }
    public int? SelectedRoomTypeId { get; set; }

    public double OccupancyRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveBookingCount { get; set; }
    public int LastMinuteBookingCount { get; set; }
    public string RevenueByCurrency { get; set; } = string.Empty;

    public List<RoomScheduleRow> Rows { get; set; } = new();
    public int DaysInMonth { get; set; }

    public List<Boat> Boats { get; set; } = new();
    public List<RoomType> RoomTypes { get; set; } = new();
    public List<Channel> Channels { get; set; } = new();
    public List<Currency> Currencies { get; set; } = new();
    public List<Package> Packages { get; set; } = new();
    public List<BookingStatus> Statuses { get; set; } = new();
}

public class RoomScheduleRow
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<DayCell> Days { get; set; } = new();
}

public class DayCell
{
    public int Day { get; set; }
    public bool IsEmpty => Booking == null && Block == null;
    public bool IsStart { get; set; }
    public int ColSpan { get; set; }
    public BookingChipVm? Booking { get; set; }
    public BlockChipVm? Block { get; set; }
}

public class BookingChipVm
{
    public int BookingId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string PackageCode { get; set; } = string.Empty;
    public int PackageAddedDate { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}

public class BlockChipVm
{
    public int BlockId { get; set; }
    public string Partner { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class BookingCreateRequest
{
    public int ChannelId { get; set; }
    public int BoatId { get; set; }
    public int RoomId { get; set; }
    public int PackageId { get; set; }
    public int CurrencyId { get; set; }
    public string? AgencyBookingCode { get; set; }
    public string? CustomerName { get; set; }
    public int AdultCount { get; set; }
    public int ChildCount { get; set; }
    public int InfantCount { get; set; }
    public DateOnly CheckIn { get; set; }
    public decimal TotalPrice { get; set; }
    public bool HasTransferService { get; set; }
    public string? PickupPoint { get; set; }
    public string? DropoffPoint { get; set; }
    public string? Note { get; set; }
    public int? EmployeeId { get; set; }
    public int? EnteredByUserId { get; set; }
    public int StatusId { get; set; } = 1;
}
