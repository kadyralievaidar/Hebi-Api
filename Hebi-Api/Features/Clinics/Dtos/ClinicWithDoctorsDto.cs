using Hebi_Api.Features.Users.Dtos;

namespace Hebi_Api.Features.Clinics.Dtos
{
    public class ClinicWithDoctorsDto
    {
        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; }
        public List<BasicInfoDto> Doctors { get; set; } = new();
    }
}
