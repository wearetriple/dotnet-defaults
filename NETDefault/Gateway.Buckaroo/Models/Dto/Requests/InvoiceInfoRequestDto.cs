namespace Gateway.Buckaroo.Models.Dto.Requests;

internal class InvoiceInfoRequestDto
{
    public string? Invoice { get; set; }
    public ServiceDto? Services { get; set; }
}
