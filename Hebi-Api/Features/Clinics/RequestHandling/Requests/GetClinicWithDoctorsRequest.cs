using Hebi_Api.Features.Core.Common.RequestHandling;

namespace Hebi_Api.Features.Clinics.RequestHandling.Requests
{
    public class GetClinicWithDoctorsRequest: Request<Response>
    {
        public Guid ClinicId { get; }

        public GetClinicWithDoctorsRequest(Guid clinicId)
        {
            ClinicId = clinicId;
        }
    }
}
