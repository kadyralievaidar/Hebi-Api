﻿using Hebi_Api.Features.Core.Common.RequestHandling;
using Hebi_Api.Features.Shifts.RequestHandling.Requests;
using Hebi_Api.Features.Shifts.Services;
using MediatR;

namespace Hebi_Api.Features.Shifts.RequestHandling.Handlers;

public class CreateShiftRequestHandler : IRequestHandler<CreateShiftRequest, Response>
{
    private readonly IShiftsService _service;
    private readonly ILogger<CreateShiftRequestHandler> _logger;

    public CreateShiftRequestHandler(IShiftsService service, ILogger<CreateShiftRequestHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public Task<Response> Handle(CreateShiftRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
