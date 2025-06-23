using Refit;
using TechChallange.Contact.Integration.Region.Dto;
using TechChallange.Contact.Integration.Response;

namespace TechChallange.Contact.Integration.Region
{
    public interface IRegionIntegration
    {
        [Get("/Region/get-by-id/{id}")]
        Task<IntegrationBaseResponseDto<RegionGetDto>> GetById(Guid id);

        [Get("/Region/get-by-ddd/{ddd}")]
        Task<IntegrationBaseResponseDto<RegionGetDto>> GetByDDD(string ddd);
    }
}
