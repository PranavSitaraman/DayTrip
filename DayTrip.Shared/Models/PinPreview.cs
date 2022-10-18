using System;

namespace DayTrip.Shared.Models
{
    public record PinPreview
    {
        public Guid Id { get; set; }
        public Guid Author { get; set; }
        public string Title { get; set; }

        public double Lat { get; set; }
        public double Lon { get; set; }
        
        public Position Position
        {
            get => new Position(Lat, Lon);
            set
            {
                Lat = value.Lat;
                Lon = value.Lon;
            }
        }

        public PinKind Kind { get; set; }
        public Guid? KindId { get; set; }
        
        public DateTime Created { get; set; }
        public DateTime? Expires { get; set; }
        public PinStatus Status { get; set; }
    }
}