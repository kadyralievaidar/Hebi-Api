﻿using Hebi_Api.Features.Appointments.Dtos;
using Hebi_Api.Features.Appointments.RequestHandling.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hebi_Api.Features.Appointments.Controllers;
public class AppointmentController : ControllerBase
{
    private readonly IMediator _mediator;
    public AppointmentController(IMediator mediator) => _mediator = mediator;

    [HttpPost("create-appoitment")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto, CancellationToken cancellationToken)
    {
        var request = new CreateAppointmentRequest(dto);
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromQuery] Guid appointmentId, [FromBody] UpdateAppointmentDto dto, CancellationToken cancellationToken)
    {
        var request = new UpdateAppointmentRequest(appointmentId, dto);
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpDelete("id")]
    public async Task<IActionResult> Delete(Guid appointmentId, CancellationToken cancellationToken)
    {
        var request = new DeleteAppointmentRequest(appointmentId);
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetById(Guid appointmentId, CancellationToken cancellationToken)
    {
        var request = new GetAppoitmentByIdRequest(appointmentId);
        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments(GetPagedListOfAppointmentDto dto, CancellationToken cancellationToken)
    {
        var request = new GetPagedListOfAppointmentRequest(dto);
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}
