using System;
using System.Collections.Generic;

namespace Reservation.Models.ViewModels
{
    public class DayLayoutViewModel
    {
        public DateTime Date { get; set; }
        public List<TableAvailabilityDto> Tables { get; set; } = new();
        // SummaryBySeats: key = seat-count (2,4,6,8), value = (available, total)
        public Dictionary<int, (int available, int total)> SummaryBySeats { get; set; } = new();
    }
}
