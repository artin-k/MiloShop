using avatCo.Areas.Profile.Controllers;
using avatCo.Areas.Profile.ViewModels;
using avatCo.Models;
using avatCo.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace avatCo.Tests.Controllers;

public class DashboardControllerTests
{
    [Fact]
    public async Task Index_WhenUserIsMissing_RedirectsToLogin()
    {
        await using var db = TestDbContextFactory.Create();
        var userManager = UserManagerMockFactory.Create();
        userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        var sut = new DashboardController(userManager.Object, db);

        var result = await sut.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
    }

    [Fact]
    public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsSameViewModel()
    {
        await using var db = TestDbContextFactory.Create();
        var userManager = UserManagerMockFactory.Create();
        var sut = new DashboardController(userManager.Object, db);
        sut.ModelState.AddModelError("UserName", "required");

        var vm = new UpdateProfileVM();
        var result = await sut.Edit(vm);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().Be(vm);
    }
}
