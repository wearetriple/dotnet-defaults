namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class CreateCombinedSubscriptionResponseDto
{
    public string? Key { get; set; }
    public StatusResponseDto Status { get; set; } = null!;
    public RequiredActionDto RequiredAction { get; set; } = null!;
    public ServiceDto[]? Services { get; set; }
    public object? CustomParameters { get; set; }
    public object? AdditionalParameters { get; set; }
    public object? RequestErrors { get; set; }
    public string? Invoice { get; set; }
    public string? ServiceCode { get; set; }
    public bool IsTest { get; set; }
    public string? Currency { get; set; }
    public decimal AmountDebit { get; set; }
    public string? TransactionType { get; set; }
    public int MutationType { get; set; }
    public object? RelatedTransactions { get; set; }
    public object? ConsumerMessage { get; set; }
    public object? Order { get; set; }
    public object? IssuingCountry { get; set; }
    public bool StartRecurrent { get; set; }
    public bool Recurring { get; set; }
    public object? CustomerName { get; set; }
    public object? PayerHash { get; set; }
    public string? PaymentKey { get; set; }
}
