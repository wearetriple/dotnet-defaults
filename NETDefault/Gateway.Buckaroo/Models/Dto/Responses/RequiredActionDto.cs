namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class RequiredActionDto
{
    public string RedirectURL { get; set; } = "";
    public object? RequestedInformation { get; set; }
    public object? PayRemainderDetails { get; set; }
    public string? Name { get; set; }
    public int TypeDeprecated { get; set; }
}
