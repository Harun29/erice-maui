using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace erice
{
    class ExcelReader : IDisposable
    {
        private readonly string path;
        private ExcelPackage excelPackage;
        private ExcelWorksheet worksheet;

        public ExcelReader(string resourceName, int sheet)
        {
            try
            {
                this.path = resourceName;

                // Set license context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Load the Excel file from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception("Resource stream is null.");
                    }

                    // Rewind the stream position to the beginning
                    stream.Seek(0, SeekOrigin.Begin);

                    excelPackage = new ExcelPackage(stream);
                }

                if (excelPackage == null)
                {
                    throw new Exception("ExcelPackage is null after initialization.");
                }

                // Ensure the sheet index is valid
                if (sheet < 0 || sheet >= excelPackage.Workbook.Worksheets.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(sheet), "Invalid sheet index.");
                }

                worksheet = excelPackage.Workbook.Worksheets[sheet];
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while initializing Excel application: " + ex.Message);
                throw; // Rethrow the exception to indicate initialization failure
            }
        }

        public string ReadCell(int row, int column)
        {
            if (worksheet == null)
            {
                throw new InvalidOperationException("Worksheet is not initialized.");
            }

            if (worksheet.Cells[row, column].Value != null)
            {
                return worksheet.Cells[row, column].Value.ToString();
            }
            else
            {
                return "";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (excelPackage != null)
                {
                    excelPackage.Dispose();
                }
            }
        }
    }
}
