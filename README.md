# AnimalViewerModul - Állatmegjelenítő modul funkcióterv

## A modul feladata  
A modul slider szerűen meg tud jelenítenie állatokat. Az állatoknak kell, hogy legyen képe, egy kis táblázata a tulajdonságairól és egy gomb az állat lefoglalására. A megjeleníteni kívánt fajt az állat settingjében lehet beállítani, úgy, hogy beírjuk a Setting1-be a faj nevét. A lefoglaló gomb készít egy új rendelést a megfelelő állatra, a bejelentkezett felhasználó adataival és csökkenti az állat készletét, ha nincs senki bejelentkezve, akkor nem csinál semmit.  
## Architektúra  
A modul a DNN nevű cms rendszerhez (https://www.dnnsoftware.com) készült.  
A VS projektet Chris Hammond (https://www.christoc.com) sablonjával hoztam létre.
A modul asználja a Hotcakes modul (https://github.com/HotcakesCommerce) api-jait, ezért a projektfájlban hozzá kellett adni reference-eket a Hotcakes-es .dll-ekre.  
A lapozhatóság megvalósítása a slick pluginnel történik (https://github.com/kenwheeler/slick).  
Saját adatbázistábla létrehozására nem volt szükség.  
## Projektstruktúra  
AnimalViewer_Modul:
- Models  
  - Animal.cs  
  - Settings.cs
- Services
  - AnimalServices.cs
- Controllers  
  - AnimalController.cs
  - SettingsController.cs

- Views
  - Animal
      - Index.cshtml
  - Settings
    - Settings.cshtml
  - Shared
    - Layout.cshtml
  - _ViewStart.cshtml
- AnimalView_Modul.dnn
- module.css
- egyéb fájlok

## Models  
Adattároló osztályok  
### Animal  
Egy állat adatait tudja tárolni: Id, név, nem, születési idő, genetika, személyiség, ár. Mindegyik *string* kivéve a születési időt, ami *DateTime* és az árat, ami *decimal*.  
### Settings  
A modul beállításait tárolja. A **Setting1** a lényeges, ez egy *string* és ez tárolja, hogy milyen fajú állatokat kell megjelenítenie a modulnak.  
## Services / AnimalServices  
Az üzleti logikát megvalósító osztály.  
### GetAnimals függvény  
Bemenete: egy kategória bvin(*string*).  
Kimenete: egy *Animal*-okból álló lista  
Hotcakes api segítségével lekéri a bemenet kategóriához tartozó összes állatot. Rájuk egyesével meghívja a GetAnimalData függvényt és belerakja őket egy listába, ha raktáron vannak. Ezt a listát adja vissza kimenetként.  
### GetAnimalData függvény  
Bemenete: egy *ProductDTO*  
Kimenete: egy *Animal*  
Regex segítségével kiveszi a LongDescription-ből az állat adatait. Létrehoz egy *Animal* változót a kinyert adatokból és a bemenet **Bvin**, **ImageFileMedium**, **SitePrice** tulajdonságaiból. Végül ezt az *Animal*-t adja vissza.  
### GetSpeciesBvin függvény  
Bemenet: egy állatfaj neve - *string*  
Kimenet: az állatfaj 'bvin'-je - *string*  
Hotcakes api segítségével lekéri az összes kategóriát. Ezek közül linq lekérdezéssel megadja, hogy mi a 'bvin'-je annak, amelyiknek a neve a bemenet. Ha van ilyen, akkor visszaadja a kapott 'bvin'-t, ha nincs, akkor a vitorlásgekkók 'bvin'-jét adja vissza.  
### AddOrder függvény  
Bemenet: egy állat 'bvin'-je - *string*
Kimenet: nincs
Bejelentkezett felhasználó lekérése: PortalSettings.Current.UserInfo;  
Hozzáad egy új rendelést hotcakes api segítségével. Ehhez felhasználja az éppen bejelentkezett felhasználót és a bemenetet. Végül szintén apival csökkenti a bemenet állat elérhető készletét.  
## Controllers  
### AnimalController  
#### Index() action  
Lekérdezi modul **Setting1**-jét, ami egy faj neve. Ezt használva bemenetként meghívja a GetSpeciesBvin függvényt és a kapott kategória id-val pedig meghívja a GetAnimals-t. A visszakapott listával betölti az Index view-et.  
#### Create() action  
Bemenet: egy állat 'bvin'-je - *string*  
Egy try-catch blokkban meghívja az AddOrder függvényt, a saját bemenetét továbbadva bemenetnek és visszairányít az Index-re.  
## Views  
### Animal/Index.cshtml  
Örököl: egy *Animal*-okból álló listát  
Ez a view a weboldalon látható modul. Egy slidert jelenít meg, aminek minden eleme egy állat képét, alatta egy táblázatot az állat tulajdonságaival és végül egy lefoglaló linket tartalmaz.  
Ha az örökölt lista üres, akkor kiírja, hogy "Nincsenek állatok". Ha vannak elemek, akkor létrehoz egy slidert(div tag, slider classal) és bele foreach ciklussal betölti az állatokat: először a képet, majd a táblázatot és végül a linket. Annak a href tulajdonsága egy 'Url.Action', ami az aktuális állat **bvin**-jével meghívja a Create aactiont. A tartalmi rész után scriptben hivatkozik a jQuery-re és a slick plugin-re (https://github.com/kenwheeler/slick) és sima scriptként megadja a slider tulajdonságait.  
### Settings/Settings.cshtml  
Ez a modul beállításainak a megjelenése. Csak annyi változott meg a sablonhoz képest, hogy a **Setting1**-et textboxban lehet változtatni.  

## mudule.css  
A modul kinézetét konfiguráló css kódok.
