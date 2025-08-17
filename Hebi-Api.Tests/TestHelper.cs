using Hebi_Api.Features.Core.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Hebi_Api.Tests;
public static class TestHelper
{
    public static Guid ClinicId = Guid.NewGuid();
    public static Guid UserId = Guid.NewGuid();
    public static Guid DoctorId = Guid.NewGuid();
    public static Mock<IHttpContextAccessor> CreateHttpContext(Guid? adminId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(Consts.UserId, adminId == null ? UserId.ToString() : adminId.ToString()),
            new(Consts.ClinicIdClaim, ClinicId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(context);

        return mock;
    }
}
