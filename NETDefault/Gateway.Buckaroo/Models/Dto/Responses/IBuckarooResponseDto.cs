namespace Gateway.Buckaroo.Models.Dto.Responses;

internal interface IBuckarooResponseDto
{
    public List<ServiceDto> Services { get; set; }
}
