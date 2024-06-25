//namespace NutriBest.Server.Tests.Features.Identity
//{
//    using NutriBest.Server.Features.Identity.Models;
//    using NutriBest.Server.Tests.Fixtures;
//    using System.Reflection;

//    public class IdentityServiceTests : IClassFixture<IdentityTestsFixture>
//    {
//        private readonly IdentityTestsFixture fixture;

//        public IdentityServiceTests(IdentityTestsFixture fixture)
//            => this.fixture = fixture;

//        [Fact]
//        public async Task AllRoles_ShouldReturnTheCurrentRoles()
//        {
//            // Act
//            var result = await fixture.IdentityService.AllRoles();

//            // Assert
//            Assert.NotNull(result);
//            var roles = Assert.IsType<List<string>>(result);
//            Assert.Contains("Employee", roles);
//            Assert.Contains("User", roles);
//            Assert.DoesNotContain("Administrator", roles);
//        }

//        [Fact]
//        public async Task CheckUserPassword_ShoulReturnTrueIfCredentialsAreValid()
//        {
//            // Arrange
//            var registerModel = new RegisterServiceModel
//            {
//                UserName = "unique30",
//                Email = "pesho@abv.bg",
//                Password = "12345Pesho",
//                ConfirmPassword = "12345Pesho",
//            };

//            // Act
//            await fixture.IdentityController.Register(registerModel);

//            var user = await fixture.UserManager.FindByEmailAsync("pesho@abv.bg");

//            var result = await fixture.IdentityService.CheckUserPassword(user, "12345Pesho");

//            // Assert
//            Assert.True(result);
//        }

//        [Theory]
//        [InlineData("unique30", "pesho@abv.bg", "12345Pesho", "12345Pesho")]
//        public async Task CheckUserPassword_ShoulReturnFalseIfCredentialsAreInvalid(string userName,
//            string email, string password, string confirmPassword)
//        {
//            var registerModel = new RegisterServiceModel
//            {
//                UserName = userName,
//                Email = email,
//                Password = password,
//                ConfirmPassword = confirmPassword,
//            };

//            await fixture.IdentityController.Register(registerModel);

//            var user = await fixture.UserManager.FindByEmailAsync(email);

//            var result = await fixture.IdentityService.CheckUserPassword(user, "12345Pesho9");

//            Assert.False(result);
//        }

//        [Theory]
//        [InlineData("unique31", "unique31@abv.bg", "12345Pesho")]
//        public async Task CreateUser_ShouldReturnSucceededResult_WithValidDataPassed(string userName,
//            string email, string password)
//        {
//            // Act
//            var result = await fixture.IdentityService.CreateUser(userName,
//                email,
//                password);

//            // Assert
//            Assert.True(result.Succeeded);
//        }

//        [Theory]
//        [MemberData(nameof(GetInvalidUserData))]
//        public async Task CreateUser_ShouldThrowInvalidOperation_WithInvalidDataPassed(string username, string email, string password)
//        {
//            // Assert
//            await Assert.ThrowsAsync<InvalidOperationException>(
//                async () => await fixture.IdentityService.CreateUser(username, email, password));
//        }

//        [Theory]
//        [InlineData("unique350", "unique350@abv.bg", "12345Pesho")]
//        public async Task CreateUser_ShouldReturnErrors_ForAlreadyExistingUser(string userName,
//            string email, string password)
//        {
//            // Act
//            var result = await fixture.IdentityService.CreateUser(userName,
//                email,
//                password);

//            // Assert
//            await Assert.ThrowsAsync<InvalidOperationException>
//                (async () => await fixture.IdentityService.CreateUser(userName,
//                email,
//                password));
//        }

//        [Theory]
//        [InlineData("unique35", "unique35@abv.bg", "12345Pesho")]
//        public async Task FindUserById_ShouldReturnModel_WhenUserExists(string userName,
//            string email, string password)
//        {
//            // Arrange
//            await fixture.IdentityService.CreateUser(userName,
//                email,
//                password);

//            var user = await fixture.UserManager.FindByEmailAsync(email);

//            // Act
//            var profileModel = await fixture.IdentityService.FindUserById(user.Id);

//            var props = profileModel!.GetType()
//                .GetRuntimeProperties();

//            // Assert
//            Assert.NotNull(profileModel);
//            Assert.Equal(7, props.Count());
//            Assert.Equal("unique35@abv.bg", profileModel.Email);
//            Assert.Equal("unique35", profileModel.UserName);
//            Assert.True(profileModel.CreatedOn <= DateTime.Now);
//            Assert.True(profileModel.ModifiedOn <= DateTime.Now);
//        }

//        [Fact]
//        public async Task FindUserById_ShouldReturnNull_WhenUserDoesNotExists()
//        {
//            // Act
//            var userModel = await fixture.IdentityService.FindUserById("invalidId");

//            // Assert
//            Assert.Null(userModel);
//        }

//        [Theory]
//        [InlineData("unique38", "unique38@abv.bg", "12345Pesho")]
//        public async Task FindUserByUserName_ShouldReturnModel_WhenUserExists(string userName,
//            string email, string password)
//        {
//            // Arrange
//            await fixture.IdentityService.CreateUser(userName,
//                email,
//                password);

//            // Act
//            var profileModel = await fixture.IdentityService.FindUserByUserName(userName);

//            // Check profileModel
//            Assert.NotNull(profileModel);
//        }

//        [Fact]
//        public async Task FindUserByUserName_ShouldReturnNull_WhenUserDoesNotExists()
//        {
//            // Act
//            var userModel = await fixture.IdentityService.FindUserByUserName("invalidUserName");

//            // Assert
//            Assert.Null(userModel);
//        }

//        public static IEnumerable<object[]> GetInvalidUserData()
//        {
//            yield return new object[] { "unique32", "unique32@abv.bg", "12345P" };
//            yield return new object[] { "uniq ue33", "unique33@abv.bg", "12345Pooooo" };
//            yield return new object[] { "uniq ue33", "unique33@abv.bg", "12345" };
//            yield return new object[] { "uniq ue33", "unique33abv.bg", "12345" };
//            yield return new object[] { "unique33", "unique33abv.bg", "12345Pooooo" };
//        }
//    }
//}
