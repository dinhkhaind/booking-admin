namespace BookingAdmin.Web.Models;

public class BookingListViewModel
{
    public int? BoatId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? RoomTypeId { get; set; }
    public int? StatusId { get; set; }
    public string? Q { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public List<Booking> Bookings { get; set; } = new();

    // Metrics
    public string RevenueByCurrency { get; set; } = string.Empty;

    // Filter bar dropdowns
    public List<Boat> Boats { get; set; } = new();
    public List<RoomType> RoomTypes { get; set; } = new();
    public List<BookingStatus> Statuses { get; set; } = new();

    // Passed to _BookingModal partial (must be RoomScheduleViewModel)
    public RoomScheduleViewModel ModalData { get; set; } = new();
}
