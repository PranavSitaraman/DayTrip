using System;
using System.ComponentModel.DataAnnotations;


namespace DayTrip.Shared.Models
{
    public record Pin
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
        
        public DateTime Created { get; set; }
        public PinKind Kind { get; set; }
        public Guid? KindId { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public DateTime? Expires { get; set; }
        public PinStatus Status { get; set; }
    }

    public enum PinKind
    {
        [Display(Name ="Warning")]
        Warning,
        [Display(Name="Blog")]
        Blog,
        [Display(Name="Lost Item")]
        LostItem,
        [Display(Name="Event")]
        Event,
    }

    public enum PinStatus
    {
        Active,
        Resolved,
        Expired
    }
}