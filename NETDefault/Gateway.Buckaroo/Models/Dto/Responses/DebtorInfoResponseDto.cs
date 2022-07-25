namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class DebtorInfoResponseDto
{
    public string? Key { get; set; }
    public StatusResponseDto Status { get; set; } = null!;
    public object? RequiredAction { get; set; }
    public ServiceDto[]? Services { get; set; }
    public ParameterDto[]? CustomParameters { get; set; }
    public ParameterDto[]? AdditionalParameters { get; set; }
    public object? RequestErrors { get; set; }
    public string? ServiceCode { get; set; }
    public bool? IsTest { get; set; }
    public object? ConsumerMessage { get; set; }
}
