﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Triple.DataAnnotations
{
    public class CompositeValidationResult : ValidationResult
    {
        public string? MemberName { get; private set; }
        public List<ValidationResult> Results { get; private set; } = new List<ValidationResult>();

        public CompositeValidationResult(string errorMessage, string? memberName = default) : base(errorMessage) { MemberName = memberName; }
    }
}
