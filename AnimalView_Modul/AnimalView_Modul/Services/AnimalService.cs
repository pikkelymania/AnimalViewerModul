using Hotcakes.Commerce.Accounts;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;
using Hotcakes.CommerceDTO.v1.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnimalView.Dnn.AnimalView_Modul.Services
{
    public class AnimalService
    {
        private IEnumerable<Models.Animal> _Animals;
        private List<Hotcakes.CommerceDTO.v1.Catalog.ProductDTO> _RawAnimals;

        private Hotcakes.CommerceDTO.v1.Client.Api _api;
        private string StoreUrl = "http://www.pikkelymania.hu/";
        private string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        
        public IEnumerable<Models.Animal> GetAnimals(string CatId)
        {
            _api = new Api(StoreUrl, ApiKey);
            var catResponse = _api.ProductsFindForCategory(CatId, 1, 100);
            if (catResponse.Errors != null || catResponse.Errors.Count == 0)
            {
                _RawAnimals = catResponse.Content.Products;
                foreach (var i in _RawAnimals)
                {
                    _Animals.Append(GetAnimalData(i));
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
                Name = RawAnimal.ProductName,
                Image = RawAnimal.ImageFileMedium,
                Price = RawAnimal.SitePrice,
                BirthDate = ABirthDate,
                Gender = AGender,
                Genetics = AGenetics,
                Personality = APersonality};
            return CurrentAnimal;
        }

        public string GetSpeciesBvin()
        {
            Models.Settings settings = new Models.Settings();
            // bvin kiválasztás setting alapján
            string animal = settings.Setting1;

            return "e197c105-c09e-47e0-af1a-918c43f3b74f";
        }

        public void AddOrder()
        {
            OrderDTO NewOrder = new OrderDTO();
            _api = new Api(StoreUrl, ApiKey);
            LineItemDTO OrderedAnimal = new LineItemDTO();
            
            NewOrder.Items.Add(OrderedAnimal);
            
            _api.OrdersCreate(NewOrder);
        }
    }
}