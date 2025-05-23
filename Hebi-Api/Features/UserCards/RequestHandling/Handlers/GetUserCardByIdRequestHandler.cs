﻿using Hebi_Api.Features.Core.Common.RequestHandling;
using Hebi_Api.Features.UserCards.RequestHandling.Requests;
using Hebi_Api.Features.UserCards.Services;
using MediatR;

namespace Hebi_Api.Features.UserCards.RequestHandling.Handlers;

public class GetUserCardByIdRequestHandler : IRequestHandler<GetUserCardByIdRequest, Response>
{
    private readonly IUserCardsService _service;
    private readonly ILogger<GetUserCardByIdRequestHandler> _logger;

    public GetUserCardByIdRequestHandler(IUserCardsService service, ILogger<GetUserCardByIdRequestHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Response> Handle(GetUserCardByIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetUserCardAsync(request.UserCardId);
            return Response.Ok(request.Id, result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}
