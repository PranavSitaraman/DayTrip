using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DayTrip.Shared;
using Microsoft.AspNetCore.SignalR;
using static DayTrip.Shared.MathExt;

namespace DayTrip.Backend.Hubs
{
    public class PinHub : Hub<IPinClient>
    {
        public static Position PosTileSize = new(0.01, 0.01);

        public async Task JoinSurroundingTiles(Position position)
        {
            var centerTile = position.FloorRound();


            foreach ((int, int) n in Neighbors)
            {
                string groupName = GetGroupName(centerTile + PosTileSize * n);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }

        }

        public async Task JoinTile(Position position)
        {
            string groupName = GetGroupName(position.FloorRound());
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveTile(Position position)
        {
            string groupName = GetGroupName(position.FloorRound());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }


        public static string GetGroupName(Position position) => GetGroupName(position.Lat, position.Lon, Guid.Empty);
        public static string GetGroupName(Position position, Guid ns) => GetGroupName(position.Lat, position.Lon, ns);

        public static string GetGroupName(double lat, double lon) =>
            GetGroupName(lat, lon, Guid.Empty);

        public static string GetGroupName(double lat, double lon, Guid ns)
        {
            return $"SRG|{ns.ToString()}|{lat}|{lon}";
        }
    }
}