using System;

namespace DayTrip.Shared.Models
{
    public record PinType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
    
}