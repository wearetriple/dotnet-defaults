namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class ServiceDto
{
    public string? Name { get; set; }
    public object? Action { get; set; }
    public List<ParameterDto>? Parameters { get; set; }
}
