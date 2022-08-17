namespace Gateway.Buckaroo.Models.Dto.Requests;

public class ServiceListDto
{
    public string? Name { get; set; }
    public string? Action { get; set; }
    public List<ParameterDto>? Parameters { get; set; }
}
