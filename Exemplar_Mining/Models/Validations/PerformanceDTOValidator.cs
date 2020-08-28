using Exemplar_Mining.Models.DTOs;
using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class PerformanceDTOValidator : AbstractValidator<PerformanceDTO>
    {
        public PerformanceDTOValidator()
        {
            RuleFor(x => x.EmployeeName).NotNull().Length(2, 20);
            RuleFor(x => x.PerformanceType).NotNull().Length(2, 20);
            RuleFor(x => x.Rating).NotNull().InclusiveBetween((short)0, (short)10);
        }
    }
}

