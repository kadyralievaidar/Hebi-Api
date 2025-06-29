using Hebi_Api.Features.Clinics.Dtos;
using Hebi_Api.Features.Clinics.RequestHandling.Requests;
using Hebi_Api.Features.Clinics.Services;
using Hebi_Api.Features.Core.Common.RequestHandling;
using MediatR;

namespace Hebi_Api.Features.Clinics.RequestHandling.Handlers
{
    public class GetClinicWithDoctorsRequestHandler : IRequestHandler<GetClinicWithDoctorsRequest, Response>
    {
        private readonly IClinicsService _service;
        private readonly ILogger<GetClinicWithDoctorsRequestHandler> _logger;

        public GetClinicWithDoctorsRequestHandler(IClinicsService service, ILogger<GetClinicWithDoctorsRequestHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Response> Handle(GetClinicWithDoctorsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetClinicWithDoctorsAsync(request.ClinicId);
                return Response.Ok(request.Id, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Response.InternalServerError(request.Id, e);
            }
        }
    }
}
