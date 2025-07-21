﻿using Hebi_Api.Features.Clinics.Services;
using Hebi_Api.Features.Core.Common;
using Hebi_Api.Features.Core.DataAccess.Models;
using Hebi_Api.Features.Core.DataAccess.UOW;
using Hebi_Api.Features.UserCards.Dtos;
using Hebi_Api.Features.UserCards.Services;
using Hebi_Api.Features.Users.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Hebi_Api.Features.Users.Services;

public class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClinicsService _clinicService;
    private readonly IUserCardsService _userCardService;

    public UsersService(
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
        IOpenIddictApplicationManager openIddictApplicationManager, IHttpContextAccessor contextAccessor,
        IUnitOfWork unitOfWork, IClinicsService clinicService, IUserCardsService userCardService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _applicationManager = openIddictApplicationManager;
        _contextAccessor = contextAccessor;
        _unitOfWork = unitOfWork;
        _clinicService = clinicService;
        _userCardService = userCardService;
    }
    public async Task CreatePatient(CreatePatientDto dto)
    {
        var username = $"{dto.FirstName}_{dto.BirthDate:yyyyMMdd}";

        if (await _userManager.FindByNameAsync(username) != null)
            username = $"{username}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";

        var patient = new ApplicationUser
        {
            UserName = username,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            BirthDateTime = dto.BirthDate
        };

        var result = await _userManager.CreateAsync(patient);
        await _userCardService.CreateUserCard(new CreateUserCardDto()
        {
            UserId = patient.Id,
        });
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(patient, Consts.Patient);
        }
    }

    public async Task CreateUser(CreateUserDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.RegisterDto.UserName);
        if (user != null)
            await _signInManager.SignInAsync(user, true);

        var clinic = await _unitOfWork.ClinicRepository.GetClinicById(dto.ClinicId);
        var newUser = new ApplicationUser()
        {
            FirstName = dto.RegisterDto.FirstName,
            LastName = dto.RegisterDto.LastName,
            UserName = dto.RegisterDto.UserName,
            Email = dto.RegisterDto.Email,
            PhoneNumber = dto.RegisterDto.PhoneNumber,
            ClinicId = clinic!.ClinicId,
            Clinic = clinic
        };
        var result = await _userManager.CreateAsync(newUser, dto.RegisterDto.Password);
        if (result.Succeeded)
        {
            await _signInManager.CreateUserPrincipalAsync(newUser);
            await _userManager.AddToRoleAsync(newUser, Consts.Doctor);
        }
    }
    public async Task<BasicUserInfoDto> GetUserById(Guid userId)
    {
        var user = await _unitOfWork.UsersRepository.FirstOrDefaultAsync(x => x.Id == userId, new List<string>() { "Clinic"});
        var basicUserInfoDto = new BasicUserInfoDto() 
        {
            UserId = userId,
            Email = user.Email, 
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ClinicName = user.Clinic?.Name
        };
        return basicUserInfoDto;
    }
    public async Task Register(RegisterUserDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user != null)
            await _signInManager.SignInAsync(user, true);

        var newUser = new ApplicationUser()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
        };
        var result = await _userManager.CreateAsync(newUser, dto.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(newUser, true);
            if (dto.IsIndividual)
            {
                newUser.ClinicId = await _clinicService.CreateDefaultClinic();
                await _userManager.UpdateAsync(newUser);
                await _userManager.AddToRoleAsync(newUser, Consts.Individual);
            }
            else
                await _userManager.AddToRoleAsync(newUser, Consts.Admin);

            await _signInManager.CreateUserPrincipalAsync(newUser);
        }
    }

    public async Task<TokenResponse> Token(OpenIddictRequest request, CancellationToken cancellationToken)
    {
        if (request.IsPasswordGrantType())
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
                throw new InvalidOperationException("The application cannot be found.");

            var identity = await ConfigIdentity(request, application);

            var result = new TokenResponse() 
            {
                Principal = new ClaimsPrincipal(identity!), 
                AuthScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            };
            return result;
        }
        if(request.IsRefreshTokenGrantType()) 
        {
            var result = await _contextAccessor.HttpContext!.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("The refresh token is no longer valid.");
            }

            var principal = result.Principal;

            var response = new TokenResponse()
            {
                Principal = principal!,
                AuthScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            };
            return response;
        }
        return new TokenResponse();
    }
    private async Task<ClaimsIdentity?> ConfigIdentity(OpenIddictRequest request, object? application)
    {
        var user = await _unitOfWork.UsersRepository.FirstOrDefaultAsync(x => x.NormalizedUserName == request.Username.ToUpperInvariant());
        var roles = await _userManager.GetRolesAsync(user!);
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application!));
        identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application!));
        identity.SetClaim(Consts.UserId, user!.Id.ToString());
        identity.SetClaim(Consts.ClinicIdClaim, user.ClinicId?.ToString());
        identity.SetClaim(Consts.Role, roles.First());

        identity.SetScopes(request.GetScopes());
        identity.SetDestinations(static claim => claim.Type switch
        {
            Claims.Name when claim.Subject!.HasScope(Scopes.Profile)
                => [Destinations.AccessToken],
            ClaimTypes.NameIdentifier => new[] { Destinations.AccessToken},

            Consts.ClinicIdClaim => new[] { Destinations.AccessToken },

            _ => [Destinations.AccessToken]
        });
        return identity;
    }

    public async Task ChangeBasicInfo(BasicInfoDto dto)
    {
        var user = await _unitOfWork.UsersRepository.GetByIdAsync(dto.UserId);
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        _unitOfWork.UsersRepository.Update(user);
        await _unitOfWork.SaveAsync();
    }
}
