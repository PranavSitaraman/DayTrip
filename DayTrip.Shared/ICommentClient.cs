using System.Threading.Tasks;
using DayTrip.Shared.Models;

namespace DayTrip.Shared
{
    public interface ICommentClient
    {
        Task ReceiveComment(Comment comment);
    }
}