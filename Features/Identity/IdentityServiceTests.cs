namespace NutriBest.Server.Tests.Features.Identity
{
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Tests.Fixtures;
    using System.Reflection;

    public class IdentityServiceTests : IClassFixture<IdentityTestsFixture>
    {
        private readonly IdentityTestsFixture fixture;

        public IdentityServiceTests(IdentityTestsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllRoles_ShouldReturnTheCurrentRoles()
        {
            // Act
            var result = await fixture.IdentityService.AllRoles();

            // Assert
            Assert.NotNull(result);
            var roles = Assert.IsType<List<string>>(result);
            Assert.Contains("Employee", roles);
            Assert.Contains("User", roles);
            Assert.DoesNotContain("Administrator", roles);
        }

        [Fact]
        public async Task CheckUserPassword_ShoulReturnTrueIfCredentialsAreValid()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique30",
                Email = "pesho@abv.bg",
                Password = "12345Pesho",
                ConfirmPassword = "12345Pesho",
            };

            // Act
            await fixture.IdentityController.Register(registerModel);

            var user = await fixture.UserManager.FindByEmailAsync("pesho@abv.bg");

            var result = await fixture.IdentityService.CheckUserPassword(user, "12345Pesho");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckUserPassword_ShoulReturnFalseIfCredentialsAreInvalid()
        {
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique30",
                Email = "pesho@abv.bg",
                Password = "12345Pesho",
                ConfirmPassword = "12345Pesho",
            };

            await fixture.IdentityController.Register(registerModel);

            var user = await fixture.UserManager.FindByEmailAsync("pesho@abv.bg");

            var result = await fixture.IdentityService.CheckUserPassword(user, "12345Pesho9");

            Assert.False(result);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnSucceededResult_WithValidDataPassed()
        {
            // Act
            var result = await fixture.IdentityService.CreateUser("unique31",
                "unique31@abv.bg",
                "12345Pesho");

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task CreateUser_ShouldThrowInvalidOperation_WithInvalidDataPassed()
        {
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>
                (async () => await fixture.IdentityService.CreateUser("unique32",
                "unique32@abv.bg",
                "12345P"));

            await Assert.ThrowsAsync<InvalidOperationException>
                (async () => await fixture.IdentityService.CreateUser("uniq ue33",
                "unique33@abv.bg",
                "12345Pooooo"));

            await Assert.ThrowsAsync<InvalidOperationException>
                (async () => await fixture.IdentityService.CreateUser("uniq ue33",
                "unique33@abv.bg",
                "12345"));

            await Assert.ThrowsAsync<InvalidOperationException>
                (async () => await fixture.IdentityService.CreateUser("uniq ue33",
                "unique33abv.bg",
                "12345"));

            await Assert.ThrowsAsync<InvalidOperationException>
                (async () => await fixture.IdentityService.CreateUser("unique33",
                "unique33abv.bg",
                "12345Pooooo"));
        }

        [Fact]
        public async Task CreateUser_ShouldReturnErrors_ForAlreadyExistingUser()
        {
            // Act
            var result = await fixture.IdentityService.CreateUser("unique350",
                "unique350@abv.bg",
                "12345Pesho");

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>
                (async () => await fixture.IdentityService.CreateUser("unique350",
                "unique350@abv.bg",
                "12345Pesho"));
        }

        [Fact]
        public async Task FindUserById_ShouldReturnModel_WhenUserExists()
        {
            // Arrange
            await fixture.IdentityService.CreateUser("unique36",
                "unique35@abv.bg",
                "12345Pesho");

            var user = await fixture.UserManager.FindByEmailAsync("unique35@abv.bg");

            // Act
            var profileModel = await fixture.IdentityService.FindUserById(user.Id);

            var props = profileModel!.GetType()
                .GetRuntimeProperties();

            // Assert
            Assert.NotNull(profileModel);
            Assert.Equal(7, props.Count());
            Assert.Equal("unique35@abv.bg", profileModel.Email);
            Assert.Equal("unique36", profileModel.UserName);
            Assert.True(profileModel.CreatedOn <= DateTime.Now);
            Assert.True(profileModel.ModifiedOn <= DateTime.Now);
        }

        [Fact]
        public async Task FindUserById_ShouldReturnNull_WhenUserDoesNotExists()
        {
            // Act
            var userModel = await fixture.IdentityService.FindUserById("invalidId");

            // Assert
            Assert.Null(userModel);
        }

        [Fact]
        public async Task FindUserByUserName_ShouldReturnModel_WhenUserExists()
        {
            // Arrange
            await fixture.IdentityService.CreateUser("unique37",
                "unique38@abv.bg",
                "12345Pesho");

            // Act
            var profileModel = await fixture.IdentityService.FindUserByUserName("unique36");

            // Check profileModel
            Assert.NotNull(profileModel);
        }

        [Fact]
        public async Task FindUserByUserName_ShouldReturnNull_WhenUserDoesNotExists()
        {
            // Act
            var userModel = await fixture.IdentityService.FindUserByUserName("invalidUserName");

            // Assert
            Assert.Null(userModel);
        }
    }
}
