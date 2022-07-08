namespace Gateway.Buckaroo.Models.Dto.Requests;

internal class CreateCombinedSubscriptionRequestDto
{
    public string Currency { get; set; } = null!;
    public string StartRecurrent { get; set; } = null!;
    public string ContinueOnIncomplete { get; set; } = null!;
    public decimal AmountDebit { get; set; }
    public decimal AmountCredit { get; set; }
    public string Invoice { get; set; } = null!;
    public object? Order { get; set; }
    public string? Description { get; set; }
    public string? ClientIP { get; set; }
    public string? ReturnURL { get; set; }
    public string? ReturnURLCancel { get; set; }
    public string? ReturnURLError { get; set; }
    public string? ReturnURLReject { get; set; }
    public string? OriginalTransactionKey { get; set; }
    public object? ServicesSelectableByClient { get; set; }
    public string? PushURL { get; set; }
    public string? PushURLFailure { get; set; }
    public string? ClientUserAgent { get; set; }
    public ServiceDto Services { get; set; } = null!;
    public ParameterCollectionDto? CustomParameters { get; set; }
    public ParameterDto[]? AdditionalParameters { get; set; }
}
