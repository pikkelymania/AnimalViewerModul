# AnimalViewerModul - Állatmegjelenítő modul

## A modul feladata  
A modul slider szerűen meg tud jelenítenie állatokat. Az állatoknak kell, hogy legyen képe, egy kis táblázata a tulajdonságairól és egy gomb az állat lefoglalására. A megjeleníteni kívánt fajt az állat settingjében lehet beállítani, úgy, hogy beírjuk a Setting1-be a faj nevét. A lefoglaló gomb készít egy új rendelést a megfelelő állatra, a bejelentkezett felhasználó adataival és csökkenti az állat készletét, ha nincs senki bejelentkezve, akkor nem csinál semmit.  

## Funkcióterv  
### Projektstruktúra  
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

### Models  
Adattároló osztályok  
#### Animal  
Egy állat adatait tudja tárolni: Id, név, nem, születési idő, genetika, személyiség, ár. Mindegyik *string* kivéve a születési időt, ami *DateTime* és az árat, ami *decimal*.  
#### Settings  
A modul beállításait tárolja. A **Setting1** a lényeges, ez egy *string* és ez tárolja, hogy milyen fajú állatokat kell megjelenítenie a modulnak.  
### Services / AnimalServices  
Az üzleti logikát megvalósító osztály.  
#### GetAnimals függvény  
Bemenete: egy kategória bvin(*string*).  
Kimenete: egy *Animal*-okból álló lista  
Hotcakes api segítségével lekéri a bemenet kategóriához tartozó összes állatot. Rájuk egyesével meghívja a GetAnimalData függvényt és belerakja őket egy listába, ha raktáron vannak. Ezt a listát adja vissza kimenetként.  
#### GetAnimalData függvény  
Bemenete: egy *ProductDTO*  
Kimenete: egy *Animal*  
Regex segítségével kiveszi a LongDescription-ből az állat adatait. Létrehoz egy *Animal* változót a kinyert adatokból és a bemenet **Bvin**, **ImageFileMedium**, **SitePrice** tulajdonságaiból. Végül ezt az *Animal*-t adja vissza.  
#### GetSpeciesBvin függvény  
Bemenet: egy állatfaj neve - *string*  
Kimenet: az állatfaj 'bvin'-je - *string*  
Hotcakes api segítségével lekéri az összes kategóriát. Ezek közül linq lekérdezéssel megadja, hogy mi a 'bvin'-je annak, amelyiknek a neve a bemenet. Ha van ilyen, akkor visszaadja a kapott 'bvin'-t, ha nincs, akkor a vitorlásgekkók 'bvin'-jét adja vissza.  
#### AddOrder függvény  
Bemenet: egy állat 'bvin'-je - *string*
Kimenet: nincs
Bejelentkezett felhasználó lekérése: PortalSettings.Current.UserInfo;  
Hozzáad egy új rendelést hotcakes api segítségével. Ehhez felhasználja az éppen bejelentkezett felhasználót és a bemenetet. Végül szintén apival csökkenti a bemenet állat elérhető készletét.  
### Controllers  
#### AnimalController  
