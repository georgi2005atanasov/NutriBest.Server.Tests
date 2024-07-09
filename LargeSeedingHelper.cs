namespace NutriBest.Server.Tests
{
    using System.Net.Http.Json;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.UsersOrders.Models;
    using NutriBest.Server.Features.GuestsOrders.Models;
    using NutriBest.Server.Features.Orders.Models;
    using NutriBest.Server.Features.Identity.Models;

    public static class LargeSeedingHelper
    {
        // MUST BE USED FOR NOT MORE THAN 100 ORDERS!!!
        public static async Task SeedUserOrders(NutriBestDbContext db,
            ClientHelper clientHelper,
            int numberOfOrders,
            List<CartProductServiceModel> products,
            UserOrderServiceModel orderModel)
        {
            var admin = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedPromoCode(clientHelper,
                "20% OFF USERS",
                "100",
                "20");
            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                null,
                "50% OFF USERS",
                "Creatines",
                DateTime.Now,
                "50");

            for (int i = 0; i < numberOfOrders; i++)
            {
                var client = await clientHelper.GetOtherUserClientAsync();

                var oldOrderModelName = orderModel.Name;
                orderModel.Name += $"{i}";

                foreach (var product in products)
                {
                    await AddProductToCart(client, product);
                }

                if (i % 3 == 0)
                {
                    var promoCodeModel = new ApplyPromoCodeServiceModel
                    {
                        Code = db.PromoCodes
                        .ToList()[i].Code
                    };

                    var statusesModel = new UpdateOrderServiceModel
                    {
                        IsConfirmed = true,
                        IsPaid = false,
                        IsShipped = true,
                        IsFinished = false
                    };

                    var promoCodeResponse = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
                    var cookieHeader = promoCodeResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
                    if (cookieHeader != null)
                        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                    await client.PostAsJsonAsync("/UsersOrders", orderModel);
                    await admin.PutAsJsonAsync($"/Orders/ChangeStatus/{i}", statusesModel);
                }
                else if (i % 5 == 0)
                {
                    await admin.PutAsync("/Promotions/Status/1", null);

                    var statusesModel = new UpdateOrderServiceModel
                    {
                        IsConfirmed = true,
                        IsPaid = true,
                        IsShipped = true,
                        IsFinished = true
                    };

                    await client.PostAsJsonAsync("/UsersOrders", orderModel);
                    await admin.PutAsJsonAsync($"/Orders/ChangeStatus/{i}", statusesModel);
                }
                else
                {
                    await client.PostAsJsonAsync("/UsersOrders", orderModel);
                }

                orderModel.Name = oldOrderModelName;
            }
        }


        // MUST BE USED FOR NOT MORE THAN 100 ORDERS!!!
        public static async Task SeedGuestOrders(NutriBestDbContext db,
            ClientHelper clientHelper,
            int numberOfOrders,
            List<CartProductServiceModel> products,
            GuestOrderServiceModel orderModel)
        {
            var admin = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedPromoCode(clientHelper,
                "20% OFF GUESTS",
                "100",
                "20");
            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                null,
                "50% OFF GUESTS",
                "Creatines",
                DateTime.Now,
                "50");

            for (int i = 0; i < numberOfOrders; i++)
            {
                var client = await clientHelper.GetOtherUserClientAsync();

                foreach (var product in products)
                {
                    await AddProductToCart(client, product);
                }

                if (i % 3 == 0)
                {
                    var promoCodeModel = new ApplyPromoCodeServiceModel
                    {
                        Code = db.PromoCodes
                        .ToList()[i].Code
                    };

                    var promoCodeResponse = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
                    var cookieHeader = promoCodeResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
                    if (cookieHeader != null)
                        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                    await client.PostAsJsonAsync("/GuestsOrders", orderModel);
                }
                else if (i % 5 == 0)
                {
                    await admin.PutAsync("/Promotions/Status/1", null);
                    await client.PostAsJsonAsync("/GuestsOrders", orderModel);
                }
                else
                {
                    await client.PostAsJsonAsync("/GuestsOrders", orderModel);
                }
            }
        }

        private static async Task AddProductToCart(HttpClient client, CartProductServiceModel product)
        {
            var response = await client.PostAsJsonAsync("/Cart/Set", product);
            var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }
        }

        public static async Task SeedUsers(ClientHelper clientHelper,
            int numberOfUsers)
        {
            var client = clientHelper.GetAnonymousClient();

            for (int i = 0; i < numberOfUsers; i++)
            {
                // Arrange
                var registerModel = new RegisterServiceModel
                {
                    UserName = $"{i}_UNIQUE_USER",
                    Email = $"UNIQUE_USER_{i}@example.com",
                    Password = "Pesho12345",
                    ConfirmPassword = "Pesho12345"
                };

                await client.PostAsJsonAsync("/Identity/Register", registerModel);
            }
        }
    }
}
