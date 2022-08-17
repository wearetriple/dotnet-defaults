using System.ComponentModel.DataAnnotations;

namespace Gateway.Buckaroo;

public class BuckarooConfiguration
{
    [Required]
    public string WebsiteKey { get; set; } = null!;

    [Required]
    public string PrivateKey { get; set; } = null!;

    [Required]
    public string BaseUrl { get; set; } = null!;

    [Required]
    public string ConfigurationCode { get; set; } = null!;
}
