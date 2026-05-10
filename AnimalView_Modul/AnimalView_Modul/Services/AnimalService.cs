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
using Hotcakes.Commerce;
using Hotcakes.Commerce.Utilities;
using System.Net.Mail;
using DotNetNuke.Services.Mail;

namespace AnimalView.Dnn.AnimalView_Modul.Services
{
    public class AnimalService
    {
        private List<Models.Animal> _Animals = new List<Models.Animal>();
        //private List<Hotcakes.CommerceDTO.v1.Catalog.ProductDTO> _RawAnimals = new List<Hotcakes.CommerceDTO.v1.Catalog.ProductDTO>();

        private Hotcakes.CommerceDTO.v1.Client.Api _api;
        private string StoreUrl = "http://www.pikkelymania.hu/";
        private string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        
        public List<Models.Animal> GetAnimals(string CatId)
        {
            _api = new Hotcakes.CommerceDTO.v1.Client.Api(StoreUrl, ApiKey);
            var catResponse = _api.ProductsFindForCategory(CatId, 1, 100);
            if (catResponse.Content != null || catResponse.Errors.Count == 0)
            {
                List<Hotcakes.CommerceDTO.v1.Catalog.ProductDTO> _RawAnimals = catResponse.Content.Products;
                if(_RawAnimals.Count > 0)
                {
                    foreach (var i in _RawAnimals)
                    {
                        var animal = GetAnimalData(i);
                        if(0 < _api.ProductInventoryFindForProduct(i.Bvin).Content.FirstOrDefault().QuantityOnHand - _api.ProductInventoryFindForProduct(i.Bvin).Content.FirstOrDefault().QuantityReserved) _Animals.Add(animal);
                    }
                }
                
            }
            if(_Animals.Count() == 0)
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
                _Animals.Append(a);
            }
            return _Animals;
        }

        public Models.Animal GetAnimalData(ProductDTO RawAnimal)
        {
            string LongDescription = RawAnimal.LongDescription;
            string AName;
            string AGender;
            string AGenetics;
            string APersonality;
            DateTime ABirthDate = DateTime.UtcNow;

            var matchNev = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Név</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchNev.Success) AName = matchNev.Groups[1].Value.Trim();
            else AName = "Hiányzik a név";

            // Nem kinyerése
            var matchNem = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Nem</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchNem.Success) AGender = matchNem.Groups[1].Value.Trim();
            else AGender = "Hiányzik a nem";

            // Genetika kinyerése
            var matchGenetika = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Genetika</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchGenetika.Success) AGenetics = matchGenetika.Groups[1].Value.Trim();
            else AGenetics = "Hiányzik a genetika";

            // Személyiség kinyerése
            var matchSzemelyiseg = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Személyiség</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchSzemelyiseg.Success) APersonality = matchSzemelyiseg.Groups[1].Value.Trim();
            else APersonality = "Hiányzik a személyiség";

            // Dátum kinyerése
            var matchSzuletett = System.Text.RegularExpressions.Regex.Match(LongDescription, @"<strong>Született</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (matchSzuletett.Success)
            {
                string datumSzoveg = matchSzuletett.Groups[1].Value.Trim();
                // A sima TryParse a magyar gépeken megérti a "2025.10.18." és a "2025. 10. 18." formátumot is
                if (DateTime.TryParse(datumSzoveg, out DateTime parsedDate))
                {
                    ABirthDate = parsedDate;
                }
            }

            Models.Animal CurrentAnimal = new Models.Animal() { 
                AnimalId = RawAnimal.Bvin,
                Name = AName,
                Image = RawAnimal.ImageFileMedium,
                Price = (int) RawAnimal.SitePrice,
                BirthDate = ABirthDate,
                Gender = AGender,
                Genetics = AGenetics,
                Personality = APersonality};
            return CurrentAnimal;
        }

        public string GetSpeciesBvin(string species)
        {
            _api = new Hotcakes.CommerceDTO.v1.Client.Api(StoreUrl, ApiKey);
            var catResponse = _api.CategoriesFindAll();
            if (catResponse.Content != null || catResponse.Errors.Count == 0)
            {
                var catBvin = (from cat in catResponse.Content
                               where cat.Name == species
                               select cat.Bvin).FirstOrDefault();
                if(catBvin != null && catBvin != "" ) return catBvin;
            }

            return "e197c105-c09e-47e0-af1a-918c43f3b74f";
        }

        public void AddOrder(string AnimalBvin)
        {
            _api = new Api(StoreUrl, ApiKey);
            OrderDTO NewOrder = new OrderDTO();
            UserInfo CurrentUser = PortalSettings.Current.UserInfo;

            if(CurrentUser.UserID > 0)
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


                var hccApp = HotcakesApplication.Current;
                var store = hccApp.CurrentStore;

                // A Hotcakes Store-ban beállított e-mail cím használata feladóként
                string fromEmail = "pikkelymania@gmail.com";
                string toEmail = NewOrder.UserEmail;
                string subject = "Sikeres foglalás";
                string body = "Ön sikeresen lefoglalt egy állatot a Pikkelymánia.hu webhelyen. Az üzletben tudja majd átvenni és kifizetni. <br>Köszonjük foglalását!";

                Mail.SendEmail(fromEmail, toEmail, subject, body);
            }            
        }
    }
}