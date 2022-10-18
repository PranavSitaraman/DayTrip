using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation.Results;
using LinqToDB;
using DayTrip.Backend.Hubs;
using DayTrip.Backend.Models;
using DayTrip.Shared;
using static DayTrip.Shared.MathExt;
using DayTrip.Shared.Models;
using DayTrip.Shared.Models.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UserManagement;


namespace DayTrip.Backend.Controllers
{
    public class PinController : Controller
    {
        public const double TILE_SIZE = 0.01;

        private readonly AppDataConnection _connection;
        private readonly PinValidator _pinValidator;
        private readonly PinKindValidator _pinKindValidator;
        private readonly IHubContext<PinHub, IPinClient> _pinHub;
        private readonly IHubContext<CommentHub, ICommentClient> _commentHub;

        public PinController(
            [FromServices] AppDataConnection connection,
            [FromServices] IHubContext<PinHub, IPinClient> pinHubCtx,
            [FromServices] IHubContext<CommentHub, ICommentClient> commentHubCtx
        )
        {
            _connection = connection;
            _pinHub = pinHubCtx;
            _commentHub = commentHubCtx;
            _pinValidator = new();
            _pinKindValidator = new();
        }

        [Route("Pin/Create")]
        [Authorize(Policy = "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> CreatePin([FromBody] Pin pin)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return BadRequest();

            pin.Id = Guid.NewGuid();
            pin.Author = userId;
            pin.Status = pin.Expires > DateTime.UtcNow ? PinStatus.Expired : PinStatus.Active;
            pin.Created = DateTime.UtcNow;

            ValidationResult result = await _pinValidator.ValidateAsync(pin);

            (DbPinPreview preview, DbPinBody body) = pin.Deconstruct();

            if (!result.IsValid) return BadRequest();
            try
            {
                await _connection.InsertAsync(preview);
                await _connection.InsertAsync(body);
            }
            catch (Exception e)
            {
                return Conflict();
            }

            string groupName = PinHub.GetGroupName(pin.Position.FloorRound());

            await _pinHub.Clients.Group(groupName).ReceivePin(preview);

            return Ok();
        }

        [Route("Pin/UploadImage")]
        [Authorize(Policy = "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> UploadImage()
        {
            var file = Request.Body;
            var folderName = Path.Combine("StaticFiles", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = $"IMG-{Guid.NewGuid()}.png";
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = "StaticFiles/Images/" + fileName;
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(dbPath);
        }

        [Route("Pin/CreateType")]
        [Authorize(Policy = "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> CreatePinType([FromBody] DbPinType pinType)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return BadRequest();

            pinType.Id = Guid.NewGuid();

            ValidationResult result = await _pinKindValidator.ValidateAsync(pinType);
            if (!result.IsValid) return BadRequest();
            try
            {
                await _connection.InsertAsync(pinType);
            }
            catch (Exception e)
            {
                return Conflict();
            }

            return Ok();
        }


        [Route("Pin/GetSurrounding")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetSurroundingPins([FromQuery] double lat, [FromQuery] double lon)
        {
            const int periphery = 1;
            double upperBoundLat = DecimalCeil(lat, 2) + periphery * TILE_SIZE;
            double lowerBoundLat = DecimalFloor(lat, 2) - periphery * TILE_SIZE;
            double upperBoundLon = DecimalCeil(lon, 2) + periphery * TILE_SIZE;
            double lowerBoundLon = DecimalFloor(lon, 2) - periphery * TILE_SIZE;
            List<PinPreview> pins = await _connection.Pins.Where(pin => pin.Expires > DateTime.UtcNow).DefaultIfEmpty().ToListAsync();

            return Json(pins);
        }

        [Route("Pin/GetBody/{id}")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetPinDetail(Guid id)
        {
            PinBody body = await _connection.PinBodies.FirstOrDefaultAsync((pinBody => pinBody.Id == id));
            return Json(body);
        }

        [Route("Pin/Comment")]
        [Authorize(Policy = "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> PostComment([FromBody] DbComment comment)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return BadRequest();
            comment.Created = DateTime.UtcNow;
            comment.Author = userId;
            comment.Id = Guid.NewGuid();
            await _connection.InsertAsync(comment);
            string groupName = CommentHub.GetGroupName(comment.Pin);

            await _commentHub.Clients.Group(groupName).ReceiveComment(comment);

            return Ok();
        }

        [Route("Pin/GetComments/{id}")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetComments(Guid id)
        {
            List<DbComment> comments = await _connection.Comments
                .Where(x => x.Pin == id)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
            foreach (Comment comment in comments)
            {
                Console.WriteLine(comment);
            }

            return Json(comments);
        }
    }
}