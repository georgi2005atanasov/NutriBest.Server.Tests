using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Profile
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Profile.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.ProfileController;

    [Collection("Profile Controller Tests")]
    public class UpdateProfileIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public UpdateProfileIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task UpdateProfile_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 21,
                Email = "administrator@example.com",
                Gender = "Male",
                UserName = "theBestAdministratorMillionaire",
                Name = "Georgi Atanasov"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);

            var profile = await db!.Profiles
                .FirstAsync(x => x.Name == "Georgi Atanasov");

            var user = await db!.Users
                .FirstAsync(x => x.UserName == "theBestAdministratorMillionaire");

            Assert.Equal("administrator@example.com", user.Email);
            Assert.Equal("Male", profile.Gender.ToString());
            Assert.Equal(21, profile.Age);
        }

        [Fact]
        public async Task UpdateProfile_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 21,
                Email = "employee123@example.com",
                Gender = "Female",
                UserName = "employee123",
                Name = "Mariq"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);

            var profile = await db!.Profiles
                .FirstAsync(x => x.Name == "Mariq");

            var user = await db!.Users
                .FirstAsync(x => x.UserName == "employee123");

            Assert.Equal("employee123@example.com", user.Email);
            Assert.Equal("Female", profile.Gender.ToString());
            Assert.Equal(21, profile.Age);
        }

        [Fact]
        public async Task UpdateProfile_ShouldBeExecuted_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 21,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "user123",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);

            var profile = await db!.Profiles
                .FirstAsync(x => x.Name == "Aleks");

            var user = await db!.Users
                .FirstAsync(x => x.UserName == "user123");

            Assert.Equal("user123@example.com", user.Email);
            Assert.Equal("Unspecified", profile.Gender.ToString());
            Assert.Equal(21, profile.Age);
        }

        [Fact]
        public async Task UpdateProfile_ShouldBeExecuted_AndShouldAlsoUpdateNotificationsEntity()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var formDataNewsletter = new MultipartFormDataContent
            {
                { new StringContent("user@example.com"), "email" },
            };

            Assert.Empty(db!.Newsletter);

            // Act
            await client.PostAsync("/Newsletter", formDataNewsletter);

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 21,
                Email = "administrator@example.com",
                Gender = "Male",
                UserName = "theBestAdministratorMillionaire",
                Name = "Georgi Atanasov"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);

            var profile = await db!.Profiles
                .FirstAsync(x => x.Name == "Georgi Atanasov");

            var user = await db!.Users
                .FirstAsync(x => x.UserName == "theBestAdministratorMillionaire");

            Assert.Equal("administrator@example.com", user.Email);
            Assert.Equal("Male", profile.Gender.ToString());
            Assert.Equal(21, profile.Age);

            Assert.False(db.Newsletter.Any(x => x.Email == "admin@example.com"));
            Assert.True(db.Newsletter.Any(x => x.Email == "administrator@example.com"));

            Assert.False(db.Newsletter.Any(x => x.Name == null));
            Assert.True(db.Newsletter.Any(x => x.Name == "Georgi Atanasov"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenAgeIsInvalid(int age)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = age,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "user123",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidAge, error);
        }

        [Theory]
        [InlineData("user")]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenUserNameIsTheSame(string userName)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = userName,
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(UserNameCannotBeTheSame, error);
        }

        [Theory]
        [InlineData("admin")]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenUserNameIsTaken(string userName)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = userName,
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(UserNameAlreadyTaken, error);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenNameIsTheSame()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "pesho",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            await client.PutAsync("/Profile", formData);

            updateModel = new UpdateProfileServiceModel
            {
                Name = "Aleks"
            };

            formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Name), "Name" }
            };

            var response = await client.PutAsync("/Profile", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(NameCannotBeTheSame, error);
        }


        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenEmailIsTheSame()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "pesho",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            await client.PutAsync("/Profile", formData);

            updateModel = new UpdateProfileServiceModel
            {
                Email = "user123@example.com"
            };

            formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Email), "Email" }
            };

            var response = await client.PutAsync("/Profile", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(EmailCannotBeTheSame, error);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenAgeIsTheSame()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "pesho",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            await client.PutAsync("/Profile", formData);

            updateModel = new UpdateProfileServiceModel
            {
                Age = 19
            };

            formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" }
            };

            var response = await client.PutAsync("/Profile", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(AgeCannotBeTheSame, error);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenGenderIsTheSame()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "pesho",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            await client.PutAsync("/Profile", formData);

            updateModel = new UpdateProfileServiceModel
            {
                Gender = "Unspecified"
            };

            formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Gender}"), "Gender" }
            };

            var response = await client.PutAsync("/Profile", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(GenderCannotBeTheSame, error);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenEmailIsTaken()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "admin@example.com",
                Gender = "Unspecified",
                UserName = "pesho",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Email 'admin@example.com' is already taken!", error);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenGenderIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 19,
                Email = "user123@example.com",
                Gender = "InvalidGender",
                UserName = "pesho",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            var error = root.GetProperty("message").GetString();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("InvalidGender is invalid Gender!", error);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var updateModel = new UpdateProfileServiceModel
            {
                Age = 21,
                Email = "user123@example.com",
                Gender = "Unspecified",
                UserName = "user123",
                Name = "Aleks"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent($"{updateModel.Age}"), "Age" },
                { new StringContent(updateModel.Email), "Email" },
                { new StringContent(updateModel.Gender), "Gender" },
                { new StringContent(updateModel.UserName), "UserName" },
                { new StringContent(updateModel.Name), "Name" }
            };

            // Act
            var response = await client.PutAsync("/Profile", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedFlavours();
            db.SeedBrands();
            db.SeedCategories();
            db.SeedPackages();
            db.SeedBgCities();
            db.SeedDeCities();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
