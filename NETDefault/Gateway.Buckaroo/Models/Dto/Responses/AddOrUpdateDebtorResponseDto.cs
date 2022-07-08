namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class AddOrUpdateDebtorResponseDto
{
    public string? Key { get; set; }
    public StatusResponseDto? Status { get; set; }
    public object? RequiredAction { get; set; }
    public ServiceDto[]? Services { get; set; }
    public object? CustomParameters { get; set; }
    public object? AdditionalParameters { get; set; }
    public object? RequestErrors { get; set; }
    public string? ServiceCode { get; set; }
    public bool? IsTest { get; set; }
    public object? ConsumerMessage { get; set; }
}
