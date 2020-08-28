using Exemplar_Mining.Models.DTOs;
using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class LeaveDTOValidator : AbstractValidator<LeaveDTO>
    {
        public LeaveDTOValidator()
        {
            RuleFor(x => x.EmployeeName).NotNull().Length(2, 20);
            RuleFor(x => x.LeaveType).NotNull().Length(2, 20);
            RuleFor(x => x.Amount).NotNull().InclusiveBetween((short)-180, (short)180);
        }
    }
}

