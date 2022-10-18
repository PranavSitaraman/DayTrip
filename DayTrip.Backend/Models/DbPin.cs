using LinqToDB.Mapping;
using DayTrip.Shared.Models;
using LinqToDB.Reflection;
using RestSharp;
using System.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace DayTrip.Backend.Models
{
    public static class PinExt
    {
        public static (DbPinPreview, DbPinBody) Deconstruct(this Pin pin)
        {
            DbPinPreview preview = new()
            {
                Author = pin.Author,
                Expires = pin.Expires,
                Id = pin.Id,
                Kind = pin.Kind,
                KindId = pin.KindId,
                Lat = pin.Lat,
                Lon = pin.Lon,
                Status = pin.Status,
                Title = pin.Title,
                Created = pin.Created
            };
            var image = "";
            var desc = "";
            try
            {
                var apikey = "fsq3iS4rbtf4aBcsbcFtH8MRwn5tlR0cD1d1LS4LgwWAxvQ=";
                var url1 = "https://api.foursquare.com/v3/places/match?name=" + Uri.EscapeDataString(pin.Title).ToString() + "&ll=" + pin.Lat.ToString() + "%2C" + pin.Lon.ToString();
                var client1 = new RestClient(url1);
                var request1 = new RestRequest(Method.GET);
                request1.AddHeader("accept", "application/json");
                request1.AddHeader("Authorization",apikey);
                IRestResponse response1 = client1.Execute(request1);
                var value1 = JObject.Parse(response1.Content);
                desc = value1["place"]["location"]["formatted_address"].ToString();
                var url2 = "https://api.foursquare.com/v3/places/" + value1["place"]["fsq_id"].ToString() + "/photos?limit=1";
                var client2 = new RestClient(url2);
                var request2 = new RestRequest(Method.GET);
                request2.AddHeader("accept", "application/json");
                request2.AddHeader("Authorization", apikey);
                IRestResponse response2 = client2.Execute(request2);
                JArray jsonArray = JArray.Parse(response2.Content);
                var value2 = JObject.Parse(jsonArray[0].ToString());
                image = value2["prefix"].ToString() + "original" + value2["suffix"].ToString();
                Console.Write(image);
            }
            catch (Exception e)
            {
                Console.Write(e);
                image = pin.Image;
                desc = pin.Description;
            }
            DbPinBody body = new()
            {
                Id = pin.Id,
                Description = pin.Description == null ? desc : pin.Description,
                Image = (pin.Image == null) ? image : pin.Image
            };
            return (preview, body);
        }
    }
}