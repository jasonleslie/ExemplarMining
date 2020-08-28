using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class DepartmentDTOValidator : AbstractValidator<DepartmentDTO>
    {
        public DepartmentDTOValidator()
        {

            RuleFor(x => x.DepartmentName).NotNull().Length(2, 20);
            //Custom Rule used for date validation            
            RuleFor(x => x.DateEstablished).ValidDateTime().When(x => x != null);
        }
    }
}

