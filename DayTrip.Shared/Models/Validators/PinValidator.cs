using FluentValidation;

namespace DayTrip.Shared.Models.Validators
{
    public class PinValidator : AbstractValidator<Pin>
    {
        public PinValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.Author).NotNull().NotEmpty();
            RuleFor(x => x.Lat).InclusiveBetween(-90, 90).NotNull();
            RuleFor(x => x.Lon).InclusiveBetween(-180, 180).NotNull();
            RuleFor(x => x.Title).MaximumLength(255).NotEmpty().NotNull();
            RuleFor(x => x.Kind).IsInEnum().NotNull();
            RuleFor(x => x.Image).MaximumLength(1023);
            RuleFor(x => x.Description).MaximumLength(8191);
            RuleFor(x => x.Status).IsInEnum().NotNull();
        }
    }
}