using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace erice
{
    class ExcelReader
    {
        string path = "";
        ExcelPackage excelPackage;
        ExcelWorksheet worksheet;

        public ExcelReader(string resourceName, int sheet)
        {
            try
            {
                this.path = resourceName;

                // Debugging: Print out all loaded resource names
                var assembly = Assembly.GetExecutingAssembly();
                var allResourceNames = assembly.GetManifestResourceNames();
                foreach (var name in allResourceNames)
                {
                    Console.WriteLine("Loaded resource: " + name);
                }

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception("Resource stream is null.");
                    }

                    Console.WriteLine("Stream length: " + stream.Length); // Debugging: Print stream length

                    // Rewind the stream position to the beginning
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        excelPackage = new ExcelPackage(memoryStream);
                    }

                    if (excelPackage == null)
                    {
                        throw new Exception("ExcelPackage is null after initialization.");
                    }
                }
                worksheet = excelPackage.Workbook.Worksheets[sheet];
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while initializing Excel application: " + ex.Message);
            }
        }




        public string ReadCell(int row, int column)
        {
            if (worksheet.Cells[row, column].Value != null)
            {
                return worksheet.Cells[row, column].Value.ToString();
            }
            else
            {
                return "";
            }
        }

        public void Close()
        {
            if (excelPackage != null)
            {
                excelPackage.Dispose();
            }
            else
            {
                Console.WriteLine("excelPackage is null. Cannot dispose.");
            }
        }
    }
}
