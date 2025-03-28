﻿
namespace Hebi_Api.Features.Core.DataAccess.Models;

public class Disease : IBaseModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public bool IsDeleted { get; set; }

    #region Foreign keys
    public Guid? ClinicId { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? LastModifiedBy { get; set; }

    #endregion
}
