using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RequestHandlerNETCore31.HelperClasses
{
    public class Utilities
    {
        public static ValidationResultModel ModelValidator(object? obj)
        {
            if (obj == null)
            {
                return new ValidationResultModel
                {
                    IsValid = false,
                    ValidationResults = new List<ValidationResult>() {
                        new ValidationResult("Validator received null object")
                    }
                };
            }

            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(obj, context, validationResults, true);

            return new ValidationResultModel
            {
                IsValid = isValid,
                ValidationResults = validationResults
            };
        }
    }
