using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using Hotcakes.Commerce.Accounts;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;
using Hotcakes.CommerceDTO.v1.Contacts;
using Hotcakes.CommerceDTO.v1.Orders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnimalView.Dnn.AnimalView_Modul.Services
{
    // 1. A WRAPPER INTERFÉSZ (Ezt fogjuk szimulálni a Moq-kal)
    public interface IHotcakesApiService
    {
        List<ProductDTO> GetProductsByCategory(string categoryId);
        int GetAvailableQuantity(string bvin);
        string GetCategoryBvinByName(string categoryName);
    }

    // 2. A VALÓS IMPLEMENTÁCIÓ (Éles környezetben ez hívja a weboldalt)
    public class HotcakesApiService : IHotcakesApiService
    {
        private Api _api;
        private string StoreUrl = "http://www.pikkelymania.hu/";
        private string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";

        public HotcakesApiService()
        {
            _api = new Api(StoreUrl, ApiKey);
        }

        public List<ProductDTO> GetProductsByCategory(string categoryId)
        {
            var response = _api.ProductsFindForCategory(categoryId, 1, 100);
            if (response.Content != null && response.Errors.Count == 0) return response.Content.Products;
            return new List<ProductDTO>();
        }

        public int GetAvailableQuantity(string bvin)
        {
            var response = _api.ProductInventoryFindForProduct(bvin);
            if (response.Content != null && response.Errors.Count == 0)
            {
                var inventory = response.Content.FirstOrDefault();
                // A fejlesztő új logikája: QuantityOnHand - QuantityReserved
                if (inventory != null) return inventory.QuantityOnHand - inventory.QuantityReserved;
            }
            return 0;
        }

        public string GetCategoryBvinByName(string categoryName)
        {
            var response = _api.CategoriesFindAll();
            if (response.Content != null && response.Errors.Count == 0)
            {
                var cat = response.Content.FirstOrDefault(c => c.Name == categoryName);
                if (cat != null) return cat.Bvin;
            }
            return "e197c105-c09e-47e0-af1a-918c43f3b74f";
        }
    }

    // 3. A FŐ SERVICE OSZTÁLY
    public class AnimalService
    {
        private List<Models.Animal> _Animals = new List<Models.Animal>();
        private readonly IHotcakesApiService _apiService;

        // Ezt csak az AddOrder használja, mert azt nem teszteljük
        private string StoreUrl = "http://www.pikkelymania.hu/";
        private string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";

        // Alap konstruktor éles használatra
        public AnimalService()
        {
            _apiService = new HotcakesApiService();
        }

        // Teszt konstruktor a Moq-hoz
        public AnimalService(IHotcakesApiService apiService)
        {
            _apiService = apiService;
        }

        public List<Models.Animal> GetAnimals(string CatId)
        {
            List<ProductDTO> rawAnimals = _apiService.GetProductsByCategory(CatId);

            if (rawAnimals != null && rawAnimals.Count > 0)
            {
                foreach (var i in rawAnimals)
                {
                    var animal = GetAnimalData(i);
                    // Az új fejlesztői logika a Wrapperen keresztül
                    if (0 < _apiService.GetAvailableQuantity(i.Bvin))
                    {
                        _Animals.Add(animal);
                    }
                }
            }

            if (_Animals.Count == 0)
            {
                Models.Animal a = new Models.Animal()
                {
                    AnimalId = "id",
                    BirthDate = DateTime.Now,
                    Gender = "nem",
                    Genetics = "genetika",
                    Image = "kép",
                    Name = "név",
                    Personality = "szem.",
                    Price = 0
                };
                _Animals.Add(a); // Append helyett Add, hogy tényleg bekerüljön a listába!
            }
            return _Animals;
        }

        public Models.Animal GetAnimalData(ProductDTO RawAnimal)
        {
            // Visszatettem a Null védelmeket, hogy a tesztjeink működjenek
            if (RawAnimal == null)
            {
                return new Models.Animal()
                {
                    AnimalId = "N/A",
                    Name = "Hiányzik a név",
                    Gender = "Hiányzik a nem",
                    Genetics = "Hiányzik a genetika",
                    Personality = "Hiányzik a személyiség",
                    BirthDate = DateTime.UtcNow,
                    Price = 0
                };
            }

            string LongDescription = RawAnimal.LongDescription ?? string.Empty;
            string AName; string AGender; string AGenetics; string APersonality;
            DateTime ABirthDate = DateTime.UtcNow;

            var matchNev = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Név</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchNev.Success) AName = matchNev.Groups[1].Value.Trim(); else AName = "Hiányzik a név";

            var matchNem = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Nem</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchNem.Success) AGender = matchNem.Groups[1].Value.Trim(); else AGender = "Hiányzik a nem";

            var matchGenetika = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Genetika</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchGenetika.Success) AGenetics = matchGenetika.Groups[1].Value.Trim(); else AGenetics = "Hiányzik a genetika";

            var matchSzemelyiseg = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Személyiség</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchSzemelyiseg.Success) APersonality = matchSzemelyiseg.Groups[1].Value.Trim(); else APersonality = "Hiányzik a személyiség";

            var matchSzuletett = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Született</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchSzuletett.Success)
            {
                string datumSzoveg = matchSzuletett.Groups[1].Value.Trim();
                if (DateTime.TryParse(datumSzoveg, out DateTime parsedDate)) { ABirthDate = parsedDate; }
            }

            return new Models.Animal()
            {
                AnimalId = RawAnimal.Bvin,
                Name = AName,
                Image = RawAnimal.ImageFileMedium,
                Price = RawAnimal.SitePrice,
                BirthDate = ABirthDate,
                Gender = AGender,
                Genetics = AGenetics,
                Personality = APersonality
            };
        }

        public string GetSpeciesBvin(string species)
        {
            return _apiService.GetCategoryBvinByName(species);
        }

        public void AddOrder(string AnimalBvin)
        {
            var _api = new Api(StoreUrl, ApiKey);
            OrderDTO NewOrder = new OrderDTO();
            UserInfo CurrentUser = PortalSettings.Current.UserInfo;

            if (CurrentUser.UserID > 0)
            {
                var orders = _api.OrdersFindAll().Content;
                NewOrder.OrderNumber = (int.Parse((from x in orders orderby x.OrderNumber descending select x.OrderNumber).FirstOrDefault()) + 1).ToString();
                NewOrder.BillingAddress = new AddressDTO
                {
                    AddressType = AddressTypesDTO.Billing,
                    City = "West Palm Beach",
                    CountryBvin = "BF7389A2-9B21-4D33-B276-23C9C18EA0C0",
                    FirstName = CurrentUser.FirstName,
                    LastName = CurrentUser.LastName,
                    Line1 = "319 N. Clematis Street",
                    Line2 = "Suite 500",
                    Phone = "561-228-5319",
                    PostalCode = "33401",
                    RegionBvin = "7EBE4F07-A844-47B8-BDA8-863DDDF5C778"
                };
                NewOrder.Items = new List<LineItemDTO>();
                var AnimalProduct = _api.ProductsFind(AnimalBvin);
                LineItemDTO Animal = new LineItemDTO()
                {
                    ProductId = AnimalProduct.Content.Bvin,
                    Quantity = 1,
                    ProductSku = AnimalProduct.Content.Sku,
                    StoreId = AnimalProduct.Content.StoreId,
                    ProductName = AnimalProduct.Content.ProductName,
                    AdjustedPricePerItem = AnimalProduct.Content.SitePrice,
                    BasePricePerItem = AnimalProduct.Content.SitePrice,
                    LineTotal = AnimalProduct.Content.SitePrice,
                    TaxPortion = decimal.Parse("0.27"),
                    QuantityReturned = 1
                };
                NewOrder.Items.Add(Animal);
                NewOrder.ShippingAddress = new AddressDTO();
                NewOrder.ShippingAddress = NewOrder.BillingAddress;
                NewOrder.ShippingAddress.AddressType = AddressTypesDTO.Shipping;
                NewOrder.UserEmail = CurrentUser.Email;
                NewOrder.UserID = CurrentUser.UserID.ToString();
                NewOrder.IsPlaced = true;
                NewOrder.TotalGrand = AnimalProduct.Content.SitePrice;
                NewOrder.PaymentStatus = new OrderPaymentStatusDTO();
                NewOrder.ShippingMethodId = "C107FBEF-C86D-46B9-A84F-3F9BC69E95AD";
                NewOrder.ShippingMethodDisplayName = "Bolti átvétel";
                NewOrder.StatusName = "Received";
                NewOrder.StatusCode = "F37EC405-1EC6-4a91-9AC4-6836215FBBBC";
                _api.OrdersCreate(NewOrder);

                ProductInventoryDTO AnimalInventory = _api.ProductInventoryFindForProduct(AnimalBvin).Content.FirstOrDefault();
                AnimalInventory.QuantityReserved = 1;
                _api.ProductInventoryUpdate(AnimalInventory);
            }
        }
    }
}