using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RequestHandlerNETCore31.HelperClasses
{
    public class ValidationResultModel
    {
        public bool IsValid { get; set; }
        public IList<ValidationResult>? ValidationResults { get; set; }
    }
}
