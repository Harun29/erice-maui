using System;  // Biblioteka koja pruža osnovne alate potrebne za izvršavanje programa, kao što su rad sa tekstom i matematičke operacije.
using System.IO;  // Biblioteka koja omogućava čitanje i pisanje podataka u datoteke i mape na računaru.
using System.Reflection;  // Biblioteka koja omogućava pregled informacija o samom programu, kao što su podaci o klasama i metodama.
using OfficeOpenXml;  // Biblioteka koja omogućava kreiranje i manipulaciju Excel dokumentima.

namespace erice
{
    class ExcelReader : IDisposable  // Ova klasa omogućava otvaranje i čitanje Excel dokumenata i osigurava da se resursi pravilno oslobode nakon upotrebe.
    {
        private readonly string path;  // Ovo čuva putanju do Excel dokumenta koji želimo da otvorimo.
        private ExcelPackage excelPackage;  // Ovaj objekat sadrži Excel dokument koji otvaramo i sa kojim radimo.
        private ExcelWorksheet worksheet;  // Ovo je pojedinačni list unutar Excel dokumenta na kojem se nalaze podaci.

        // Ovo je posebna metoda koja se poziva kada kreiramo novi objekat klase ExcelReader.
        public ExcelReader(string resourceName, int sheet)
        {
            try
            {
                this.path = resourceName;  // Ovde postavljamo putanju do Excel dokumenta.

                // Ovde postavljamo pravila korišćenja biblioteke OfficeOpenXml.
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Ovde dobavljamo informacije o samom programu.
                var assembly = Assembly.GetExecutingAssembly();

                // Ovde pokušavamo da učitamo Excel dokument iz programa.
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception("Resource stream is null.");  // Ako ne uspemo da učitamo dokument, program će prijaviti grešku.
                    }

                    // Ovde postavljamo početnu tačku za čitanje podataka iz dokumenta.
                    stream.Seek(0, SeekOrigin.Begin);

                    // Ovde kreiramo novi objekat koji će sadržati Excel dokument na osnovu podataka iz toka.
                    excelPackage = new ExcelPackage(stream);
                }

                // Ako objekat koji sadrži Excel dokument nije uspešno kreiran, program će prijaviti grešku.
                if (excelPackage == null)
                {
                    throw new Exception("ExcelPackage is null after initialization.");
                }

                // Ovde postavljamo koji list unutar Excel dokumenta želimo da koristimo.
                worksheet = excelPackage.Workbook.Worksheets[sheet];
            }
            catch (Exception ex)
            {
                // Ako dođe do greške prilikom otvaranja Excel dokumenta, ova poruka će se ispisati.
                Console.WriteLine("An error occurred while initializing Excel application: " + ex.Message);
                throw; // Ovo će prekinuti izvršavanje programa i prijaviti grešku.
            }
        }

        // Ova metoda vraća tekst iz određene ćelije u Excel dokumentu.
        public string ReadCell(int row, int column)
        {
            // Ako nismo uspešno postavili koji list koristimo, program će prijaviti grešku.
            if (worksheet == null)
            {
                throw new InvalidOperationException("Worksheet is not initialized.");
            }

            // Ovde proveravamo da li u ćeliji postoji neki tekst.
            if (worksheet.Cells[row, column].Value != null)
            {
                // Ako postoji, vraćamo taj tekst.
                return worksheet.Cells[row, column].Value.ToString();
            }
            else
            {
                // Ako ćelija ne sadrži tekst, vraćamo prazan string.
                return "";
            }
        }

        // Ova metoda oslobađa resurse koje Excel dokument koristi.
        public void Dispose()
        {
            Dispose(true);  // Pozivamo internu metodu za oslobađanje resursa.
            GC.SuppressFinalize(this);  // Ovo sprečava da se objekat ponovo finalizuje.
        }

        // Interna metoda za oslobađanje resursa.
        protected virtual void Dispose(bool disposing)
        {
            // Ako je objekat koji sadrži Excel dokument već kreiran, ovde ga oslobađamo.
            if (excelPackage != null)
            {
                excelPackage.Dispose();  // Ovo oslobađa sve resurse koje Excel dokument koristi.
            }
        }
    }
}


