using Exemplar_Mining.Models.DTOs;
using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class MineCreateDTOValidator : AbstractValidator<MineDTO>
    {
        public MineCreateDTOValidator()
        {
            RuleFor(x => x.MineId).NotNull();
            RuleFor(x => x.Name).NotNull().Length(2, 20);
            RuleFor(x => x.Type).NotNull().Length(2, 20);
            RuleFor(x => x.Name).NotNull().Length(2, 20);
            RuleFor(x => x.Lattitude).NotNull().InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).NotNull().InclusiveBetween(-180, 180);
            RuleFor(x => x.OverseerName).NotNull().Length(2, 20);
        }
    }
}

