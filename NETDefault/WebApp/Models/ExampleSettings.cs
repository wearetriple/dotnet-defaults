using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    // see Options pattern
    public class ExampleSettings
    {
        [Required]
        [MinLength(10)]
        public string Example { get; set; }
    }
}
