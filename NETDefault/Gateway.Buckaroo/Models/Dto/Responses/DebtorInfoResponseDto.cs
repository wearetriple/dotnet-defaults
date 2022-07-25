namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class DebtorInfoResponseDto : IBuckarooResponseDto
{
    public string? Key { get; set; }
    public StatusResponseDto Status { get; set; } = null!;
    public object? RequiredAction { get; set; }
    public List<ServiceDto>? Services { get; set; }
    public List<ParameterDto>? CustomParameters { get; set; }
    public List<ParameterDto>? AdditionalParameters { get; set; }
    public object? RequestErrors { get; set; }
    public string? ServiceCode { get; set; }
    public bool? IsTest { get; set; }
    public object? ConsumerMessage { get; set; }
}
