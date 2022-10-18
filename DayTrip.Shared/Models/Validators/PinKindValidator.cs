using FluentValidation;

namespace DayTrip.Shared.Models.Validators
{
    public class PinKindValidator : AbstractValidator<PinType>
    {
        public PinKindValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.Color).NotNull().MaximumLength(63);
            RuleFor(x => x.Icon).NotNull().MaximumLength(1023);
            RuleFor(x => x.Name).NotNull().MaximumLength(255);
        }
    }
}