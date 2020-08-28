using Exemplar_Mining.Models.DTOs;
using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class ResourceDTOValidator : AbstractValidator<ResourceDTO>
    {
        public ResourceDTOValidator()
        {

            RuleFor(x => x.Type).NotNull().Length(2, 20);
            RuleFor(x => x.Value).NotNull();
            RuleFor(x => x.Metric).NotNull().Length(2, 20);
        }
    }
}

