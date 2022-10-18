using System;

namespace DayTrip.Shared.Models
{
    public record Comment
    {
        public Guid Id { get; set; }
        public Guid Author { get; set; }
        public Guid Pin { get; set; }
        public DateTime Created { get; set; }
        public string Text { get; set; }
    }
}