using LinqToDB.Mapping;
using DayTrip.Shared.Models;

namespace DayTrip.Backend.Models
{
    [Table("comments")]
    [Column("id", nameof(Id))]
    [Column("author", nameof(Author))]
    [Column("pin", nameof(Pin))]
    [Column("created", nameof(Created))]
    [Column("text", nameof(Text))]
    public record DbComment : Comment
    {
    }
}