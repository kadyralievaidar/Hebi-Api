using Hebi_Api.Features.Clinics.Dtos;
using Hebi_Api.Features.Core.Common.Enums;
using Hebi_Api.Features.Core.Common.RequestHandling;
using Hebi_Api.Features.Core.DataAccess.Models;
using Hebi_Api.Features.Core.DataAccess.UOW;
using Hebi_Api.Features.Core.Extensions;
using Hebi_Api.Features.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Hebi_Api.Features.Clinics.Services;

public class ClinicsService : IClinicsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClinicsService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _contextAccessor = contextAccessor;
        _userManager = userManager;
    }

    public async Task<Guid> CreateClinicAsync(CreateClinicDto dto)
    {
        try
        {
            var userId = _contextAccessor.GetUserIdentifier();
            var user = await _unitOfWork.UsersRepository.FirstOrDefaultAsync(x => x.Id == userId);

            var clinic = new Clinic
            {
                Name = dto.Name,
                Location = dto.Location,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id,
                IsActive = true
            };
            await _unitOfWork.ClinicRepository.InsertAsync(clinic);

            var isSuperAdmin = await _userManager.IsInRoleAsync(user, UserRoles.SuperAdmin.ToString());

            if (!isSuperAdmin && !dto.DoctorIds.Contains(userId.Value))
                dto.DoctorIds.Add(userId.Value);

            var doctors = await _unitOfWork.UsersRepository
                .WhereAsync(x => dto.DoctorIds.Contains(x.Id) && !x.IsDeleted);

            if (doctors.Any())
            {
                foreach (var doctor in doctors)
                    doctor.ClinicId = clinic.Id;

                await _unitOfWork.UsersRepository.UpdateRangeAsync(doctors);
            }

            await _unitOfWork.SaveAsync();
            return clinic.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public async Task<Guid> CreateDefaultClinic()
    {
        var test = await _unitOfWork.ClinicRepository.CreateDefaultClinic();
        return await _unitOfWork.ClinicRepository.CreateDefaultClinic();
    }

    public async Task DeleteClinic(Guid id)
    {
        var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(id)
            ?? throw new NullReferenceException(nameof(Clinic));
        _unitOfWork.ClinicRepository.Delete(clinic);

        await _unitOfWork.SaveAsync();
    }

    public async Task<Clinic> GetClinicAsync(Guid clinicId)
    {
        var clinic = await _unitOfWork.ClinicRepository.GetClinicById(clinicId);

        return clinic;
    }

    public async Task<PagedResult<Clinic>> GetListOfClinicsAsync(GetPagedListOfClinicDto dto)
    {
        var query = _unitOfWork.ClinicRepository.AsQueryable();
        if (!string.IsNullOrEmpty(dto.SearchText))
            query = query.Where(x => x.Name.Contains(dto.SearchText) || x.Location.Contains(dto.SearchText));

        var totalCount = await query.CountAsync();
        var clinics = await query.OrderByDynamic(dto.SortBy, dto.SortDirection == ListSortDirection.Descending)
                .TrySkip(dto.PageSize * dto.PageIndex).TryTake(dto.PageSize).ToListAsync();

        return new PagedResult<Clinic>() 
        {
            Results = clinics,
            TotalCount = totalCount 
        };
    }

    public async Task<Clinic> UpdateClinicAsync(Guid id, CreateClinicDto dto)
    {
        var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(id);
        clinic.Location = dto.Location;
        clinic.Email = dto.Email;
        clinic.PhoneNumber = dto.PhoneNumber;
        clinic.Name = dto.Name;
        clinic.LastModifiedAt = DateTime.UtcNow;
        clinic.LastModifiedBy = _contextAccessor.GetUserIdentifier();

        _unitOfWork.ClinicRepository.Update(clinic);

        var doctors = await _unitOfWork.UsersRepository
                        .WhereAsync(user => dto.DoctorIds.Contains(user.Id) && !user.IsDeleted);

        foreach (var doctor in doctors)
        {
            doctor.ClinicId = clinic.Id;
        }

        await _unitOfWork.UsersRepository.UpdateRangeAsync(doctors);
        await _unitOfWork.SaveAsync();

        return clinic;
    }

    public async Task<ClinicWithDoctorsDto?> GetClinicWithDoctorsAsync(Guid clinicId)
    {
        var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(clinicId);
        if (clinic == null) return null;

        var allUsers = await _userManager.Users
            .Where(u => u.ClinicId == clinicId && !u.IsDeleted)
            .AsNoTracking()
            .ToListAsync();


        var doctors = new List<ApplicationUser>();

        foreach (var user in allUsers)
        {
            if (await _userManager.IsInRoleAsync(user, UserRoles.Doctor.ToString()))
            {
                doctors.Add(user);
            }
        }

        return new ClinicWithDoctorsDto
        {
            ClinicId = clinic.Id,
            ClinicName = clinic.Name,
            Doctors = doctors.Select(d => new BasicInfoDto
            {
                UserName = d.UserName,
                FirstName = d.FirstName,
                LastName = d.LastName,
                PhoneNumber = d.PhoneNumber,
            }).ToList()
        };
    }
}
