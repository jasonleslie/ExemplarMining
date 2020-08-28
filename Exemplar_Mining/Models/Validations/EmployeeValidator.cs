using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public class EmployeeValidator : AbstractValidator<Employee>
    {
        public EmployeeValidator()
        {
            RuleFor(x => x.FirstName).NotNull().Length(2, 20);
            RuleFor(x => x.LastName).NotNull().Length(2, 20);
            RuleFor(x => x.Position).NotNull().Length(2, 20);
            RuleFor(x => x.DepId).NotNull();
            //Custom Rule used for date validation
            RuleFor(x => x.EnrollmentDate).ValidDateTime().When(x => x != null);
            RuleFor(x => x.Salary).NotNull().GreaterThanOrEqualTo(0);
        }

    }
}

