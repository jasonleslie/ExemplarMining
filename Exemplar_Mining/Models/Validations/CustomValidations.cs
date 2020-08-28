using FluentValidation;

namespace Exemplar_Mining.Models.Validations
{
    public static class CustomValidations
    {


        /*
        * Method used to validate the various dates pertaining to object creation such as an employee's enrollment date.
        * Date is valid only if occurred sometime in the past year.
        */
        public static IRuleBuilderOptions<T, DateTime> ValidDateTime<T, DateTime>(this IRuleBuilder<T, DateTime> ruleBuilder)
        {
            var DateUpperBound = System.DateTime.Now;
            var DateLowerBound = DateUpperBound.AddYears(-1);

            var DateErrorMessage = "Dates must be some datetime value that occurred in the past year.";

            return ruleBuilder.Must(dt => DateLowerBound.CompareTo(dt) <= 0 && DateUpperBound.CompareTo(dt) >= 0).WithMessage(DateErrorMessage);
        }
    }
}
