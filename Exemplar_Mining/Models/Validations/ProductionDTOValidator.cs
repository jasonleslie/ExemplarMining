using Exemplar_Mining.Models.DTOs;
using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class ProductionDTOValidator : AbstractValidator<ProductionDTO>
    {
        public ProductionDTOValidator()
        {

            RuleFor(x => x.MineId).NotNull();
            RuleFor(x => x.Amount).NotNull();
            //Custom Rule used for date validation            
            RuleFor(x => x.Datelogged).ValidDateTime().When(x => x != null);
        }
    }
}

