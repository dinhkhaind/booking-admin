namespace BookingAdmin.Web.Models;

public class TransferBoatRequest
{
    public int BookingId { get; set; }
    public int NewBoatId { get; set; }
    public int NewRoomId { get; set; }
}
