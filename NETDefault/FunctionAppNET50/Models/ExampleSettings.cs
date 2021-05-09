using System.ComponentModel.DataAnnotations;

namespace FunctionAppNET50.Models
{
    public class ExampleSettings
    {
        [Required]
        [MinLength(10)]
        public string Example { get; set; }
    }
}
