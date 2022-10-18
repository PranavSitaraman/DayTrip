using System;
using LinqToDB.Mapping;
using DayTrip.Shared.Models;

namespace DayTrip.Backend.Models
{
    [Table(Name = "pins")]
    [Column("id", nameof(Id))]
    [Column("author", nameof(Author))]
    [Column("title", nameof(Title))]
    [Column("lat", nameof(Lat))]
    [Column("lon", nameof(Lon))]
    [Column("kind", nameof(Kind))]
    [Column("kind_id", nameof(KindId))]
    [Column("expires", nameof(Expires))]
    [Column("status", nameof(Status))]
    [Column("created", nameof(Created))]
    
    public record DbPinPreview : PinPreview
    {
    }
}