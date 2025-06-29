using Hebi_Api.Features.Clinics.Dtos;
using Hebi_Api.Features.Clinics.RequestHandling.Requests;
using Hebi_Api.Features.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Hebi_Api.Features.Clinics.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ClinicController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicController(IMediator mediator) => _mediator = mediator;

    [HttpPost("create-clinic")]
    public async Task<IActionResult> Create([FromBody] CreateClinicDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateClinicRequest(dto), cancellationToken);
        return result.AsAspNetCoreResult();
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromQuery] Guid clinicId, [FromBody] CreateClinicDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateClinicRequest(clinicId, dto), cancellationToken);
        return result.AsAspNetCoreResult();
    }

    [HttpDelete("id")]
    public async Task<IActionResult> Delete(Guid clinicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteClinicRequest(clinicId),cancellationToken);
        return result.AsAspNetCoreResult();
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetById(Guid clinicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClinicByIdRequest(clinicId), cancellationToken);
        return result.AsAspNetCoreResult();
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAppointments([FromQuery] GetPagedListOfClinicDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPagedListClinicRequest(dto), cancellationToken);
        return result.AsAspNetCoreResult();
    }


    [HttpGet("{clinicId}/doctors")]
    public async Task<IActionResult> GetClinicWithDoctors(Guid clinicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClinicWithDoctorsRequest(clinicId), cancellationToken);
        return result.AsAspNetCoreResult();
    }

}
