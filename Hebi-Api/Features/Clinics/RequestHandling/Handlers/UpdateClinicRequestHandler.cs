﻿using Hebi_Api.Features.Clinics.RequestHandling.Requests;
using Hebi_Api.Features.Clinics.Services;
using Hebi_Api.Features.Core.Common.RequestHandling;
using MediatR;

namespace Hebi_Api.Features.Clinics.RequestHandling.Handlers;

public class UpdateClinicRequestHandler : IRequestHandler<UpdateClinicRequest, Response>
{
    private readonly IClinicsService _service;
    private readonly ILogger<UpdateClinicRequestHandler> _logger;

    public UpdateClinicRequestHandler(IClinicsService service, ILogger<UpdateClinicRequestHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Response> Handle(UpdateClinicRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateClinic(request.ClinicId, request.CreateClinicDto);
            return Response.Ok(request.Id, result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}
