using System.ComponentModel.DataAnnotations;

namespace FunctionAppNETCore31.Models
{
    // see Options pattern
    public class ExampleSettings
    {
        [Required]
        [MinLength(10)]
        public string Example { get; set; }
    }
}
