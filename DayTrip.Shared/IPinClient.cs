using System.Threading.Tasks;
using DayTrip.Shared.Models;

namespace DayTrip.Shared
{
    public interface IPinClient
    {
        Task ReceivePin(PinPreview pin);
    }
}