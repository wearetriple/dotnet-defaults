namespace Gateway.Buckaroo.Models.Dto.Responses;

internal class StatusResponseDto
{
    public CodeDto Code { get; set; } = null!;
    public SubCodeDto SubCode { get; set; } = null!;
    public DateTime? DateTime { get; set; }
}
