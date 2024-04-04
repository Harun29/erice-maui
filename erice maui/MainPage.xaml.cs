using erice; // Uključujemo alate definisane u 'erice' prostoru imena koji nam pomažu u radu sa Excel datotekama.
using System.Reflection; // Uključujemo alate za rad sa metapodacima programa, kao što su informacije o klasama i metodama.

namespace erice_maui
{
    public partial class MainPage : ContentPage
    {
        // Osnovna stranica aplikacije.
        public MainPage()
        {
            InitializeComponent(); // Inicijalizacija komponenti stranice.
        }

        // Metoda koja se poziva kada se klikne na dugme "Calculate".
        private void OnCalculateClicked(object sender, EventArgs e)
        {
            // Provjera da li su svi obavezni podaci popunjeni.
            if (string.IsNullOrWhiteSpace(GenderPicker.SelectedItem?.ToString()) ||
               string.IsNullOrWhiteSpace(CholesterolEntry.Text) ||
               string.IsNullOrWhiteSpace(AgeEntry.Text) ||
               string.IsNullOrWhiteSpace(SbpEntry.Text))
            {
                DisplayAlert("Error", "Please fill in all required fields.", "OK"); // Ako nisu, prikazujemo poruku o grešci.
                return; // Prekidamo dalje izvršavanje metode.
            }

            // Čuvanje informacija koje su unesene.
            string spol = GenderPicker.SelectedItem?.ToString();
            double chol = double.Parse(CholesterolEntry.Text);
            int age = int.Parse(AgeEntry.Text);
            double sbp = double.Parse(SbpEntry.Text);
            bool diabetic = DiabeticSwitch.IsToggled;
            bool smoker = SmokerSwitch.IsToggled;

            // Promenljive za navigaciju kroz Excel tabelu.
            int row = 0; //Red u tabeli.
            int col = 0; //Kolona u tabeli.
            int start = 4; // Početni red za čitanje.
            int finish = 24; // Završni red za čitanje.
            int reading = 1; // Trenutni red za čitanje.
            int sheet; // Broj lista (stranice) u Excel datoteci.

            // Određujemo koji list koristimo u zavisnosti od spola korisnika.
            if (spol == "Musko")
            {
                sheet = 1; // Za muškarce koristimo drugi list.
            }
            else
            {
                sheet = 0; // Za žene koristimo prvi list.
            }

            ExcelReader excel = null; // Kreiramo prazan objekat za čitanje Excel datoteka.
            var resourcePath = "erice_maui.Resources.Assets.Erice2.xlsx";  // Putanja do Excel datoteke.

            try
            {
                // Pokušavamo da kreiramo novi objekat za čitanje Excel datoteka sa datom putanjom.
                excel = new ExcelReader(resourcePath, sheet); // Kreiranje instance ExcelReader-a za čitanje podataka iz Excel datoteke.
                bool breakCheck = false; // Ova promenljiva nam pomaže da znamo kada da prekinemo petlju.

                // Petlja koja prolazi kroz Excel tabelu kako bi pronašla odgovarajuće podatke.
                while (!breakCheck)
                {
                    // Ako smo na početku tabele, počinjemo pretragu.
                    if (row == 0)
                    {
                        // Prolazimo kroz redove od početnog do završnog.
                        for (int i = start; i <= finish; i++)
                        {
                            // Čitamo vrednost iz trenutne ćelije.
                            string cellValue = excel.ReadCell(i, reading); // Čitanje podataka iz ćelije u Excel tablici.

                            // Pokušavamo da pretvorimo tekst iz ćelije u broj.
                            if (int.TryParse(cellValue, out int cellIntValue))
                            {
                                // Ako je uspešno, proveravamo da li je vrednost u granicama koje tražimo.
                                if ((age >= cellIntValue) && (cellIntValue != 0) && (reading == 1))
                                {
                                    // Ako jeste, postavljamo nove granice za pretragu.
                                    start = i - 3;
                                    finish = i;
                                    reading = 3;
                                    break;// Prekidamo trenutnu iteraciju petlje.
                                }
                                else if ((age < 49 && spol == "Zensko") && (reading == 1))
                                {
                                    // Ako je korisnik ženskog spola i mlađi od 49 godina, postavljamo granice za tu kategoriju.
                                    start = 20;
                                    finish = 23;
                                    reading = 3;
                                    break;
                                }
                                else if ((age < 39 && spol == "Musko") && (reading == 1))
                                {
                                    // Ako je korisnik muškog spola i mlađi od 39 godina, postavljamo granice za tu kategoriju.
                                    start = 20;
                                    finish = 23;
                                    reading = 3;
                                    break;
                                }
                                else if ((sbp >= cellIntValue) && reading == 3)
                                {
                                    row = i; // Postavljamo red na trenutnu poziciju.
                                    start = 4;
                                    finish = 17;
                                    reading = 1;
                                    break;
                                }
                            }
                            else
                            {
                                // Ako ne možemo da pretvorimo vrednost ćelije u broj, ispisujemo poruku o grešci.
                                Console.WriteLine("Failed to parse cell value to integer.");
                            }
                        }
                    }
                    else
                    {
                        // Ako nismo u prvom prolazu, pretražujemo dalje.
                        for (int i = start; i <= finish; i++)
                        {
                            // Čitamo vrednost iz ćelije.
                            string cellValue = excel.ReadCell(reading, i);

                            // Proveravamo da li je korisnik dijabetičar i usklađujemo pretragu sa tim.
                            if (diabetic && (cellValue == "Diabetics") && (reading == 1))
                            {
                                start = 4; // Početni red za dijabetičare.
                                finish = 11; // Završni red za dijabetičare.
                                reading = 2; // Prelazimo na drugu kolonu za čitanje.
                                break;
                            }
                            else if (!diabetic && (cellValue == "Non diabetics") && (reading == 1))
                            {
                                start = 12; // Početni red za one koji nisu dijabetičari.
                                finish = 19; // Završni red za one koji nisu dijabetičari.
                                reading = 2; // Prelazimo na drugu kolonu za čitanje.
                                break;
                            }
                            else if (smoker && (cellValue == "Smokers") && (reading == 2))
                            {
                                start += 4; // Pomeramo početni red za pušače.
                                reading = 3; // Prelazimo na treću kolonu za čitanje.
                                break;
                            }
                            else if (!smoker && (cellValue == "Non smokers") && (reading == 2))
                            {
                                finish -= 4; // Pomeramo završni red za one koji nisu pušači.
                                reading = 3; // Prelazimo na treću kolonu za čitanje.
                                break;
                            }

                            else if (double.TryParse(cellValue, out double cellIntValue))
                            {
                                // Ako uspemo da pretvorimo vrednost ćelije u decimalni broj, proveravamo nivo holesterola.
                                if ((chol < cellIntValue) && reading == 3)
                                {
                                    col = i; // Postavljamo kolonu na trenutnu poziciju.
                                    breakCheck = true;// Označavamo da smo pronašli potrebne podatke i prekidamo petlju.
                                    break;
                                }
                                else if ((chol > 7.8) && reading == 3)
                                {
                                    col = finish; // Ako je nivo holesterola veći od 7.8, postavljamo kolonu na završnu poziciju.
                                    breakCheck = true; // Označavamo da smo pronašli potrebne podatke i prekidamo petlju.
                                    break;
                                }
                            }
                        }
                    }
                }

                // Čitamo rezultat iz odgovarajuće ćelije u Excel tabeli.
                string result = excel.ReadCell(row, col);
                int.TryParse(result, out int intResult); // Pokušavamo da pretvorimo rezultat u ceo broj.
                string riskLevel = ""; // Promenljiva za nivo rizika.
                string chances = ""; // Promenljiva za opis šansi za srčani udar.

                // Logika za određivanje nivoa rizika na osnovu pronađenih podataka.
                if (intResult < 5)
                {
                    riskLevel = "Low";
                    chances = "Šanse za srčani udar su niske.";
                }
                else if (intResult >= 5 && intResult <= 9)
                {
                    riskLevel = "Mild";
                    chances = "Šanse za srčani udar su niske, ali trebate obratiti pažnju.";
                }
                else if (intResult >= 10 && intResult <= 14)
                {
                    riskLevel = "Moderate";
                    chances = "Postoji umjerena opasnost od srčanog udara.";
                }
                else if (intResult >= 15 && intResult <= 19)
                {
                    riskLevel = "Moderate-high";
                    chances = "Postoje prilično visoke šanse za srčani udar.";
                }
                else if (intResult >= 20 && intResult <= 29)
                {
                    riskLevel = "High";
                    chances = "Visoke šanse za srčani udar. Potrebno je hitno djelovanje.";
                }
                else
                {
                    riskLevel = "Very high";
                    chances = "Veoma visoke šanse za srčani udar. Odmah potražite medicinsku pomoć.";
                }

                // Prikazujemo rezultat i informacije korisniku.
                DisplayAlert("Rezultat", $"Rezultat: {result} - {riskLevel}\nSanse: {chances}", "OK");
            }

            // Hvatanje i obrada izuzetaka koji se mogu pojaviti prilikom čitanja Excel datoteke.
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message); // Ispisivanje poruke o grešci.
            }

            // Oslobađanje resursa nakon što završimo sa korišćenjem ExcelReader objekta.
            finally
            {
                if (excel != null)
                {
                    excel.Dispose(); // Oslobađanje ExcelReader objekta.
                }
            }

        }

    }

}
