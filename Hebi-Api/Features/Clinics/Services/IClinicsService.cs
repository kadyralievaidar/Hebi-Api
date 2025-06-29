using Hebi_Api.Features.Clinics.Dtos;
using Hebi_Api.Features.Core.Common.RequestHandling;
using Hebi_Api.Features.Core.DataAccess.Models;

namespace Hebi_Api.Features.Clinics.Services;

public interface IClinicsService 
{
    /// <summary>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<Guid> CreateClinicAsync(CreateClinicDto dto);

    Task DeleteClinic(Guid id);

    Task<Clinic> UpdateClinicAsync(Guid id, CreateClinicDto dto);

    Task<PagedResult<Clinic>> GetListOfClinicsAsync(GetPagedListOfClinicDto dto);

    Task<Clinic> GetClinicAsync(Guid clinicId);
    Task<Guid> CreateDefaultClinic();
    Task<ClinicWithDoctorsDto?> GetClinicWithDoctorsAsync(Guid clinicId);
}
