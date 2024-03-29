﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Triple.DataAnnotations
{
    /// <summary>
    /// This attribute will instruct the entity validator to also validate the properties of this object, instead of just the object itself.
    /// </summary>
    public class ValidateObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, validationContext, null);

            Validator.TryValidateObject(value, context, results, true);

            if (results.Count != 0)
            {
                var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed!", validationContext.MemberName ?? "Unknown member");
                compositeResults.Results.AddRange(results);

                return compositeResults;
            }

            return ValidationResult.Success;
        }
    }
}
