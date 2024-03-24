using erice;
using System.Reflection;

namespace erice_maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCalculateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GenderPicker.SelectedItem?.ToString()) ||
               string.IsNullOrWhiteSpace(CholesterolEntry.Text) ||
               string.IsNullOrWhiteSpace(AgeEntry.Text) ||
               string.IsNullOrWhiteSpace(SbpEntry.Text))
            {
                DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }

            string spol = GenderPicker.SelectedItem?.ToString();
            double chol = double.Parse(CholesterolEntry.Text);
            int age = int.Parse(AgeEntry.Text);
            double sbp = double.Parse(SbpEntry.Text);
            bool diabetic = DiabeticSwitch.IsToggled;
            bool smoker = SmokerSwitch.IsToggled;


            int row = 0;
            int col = 0;
            int start = 4;
            int finish = 24;
            int reading = 1;
            int sheet;

            if (spol == "Musko")
            {
                sheet = 2;
            }
            else
            {
                sheet = 1;
            }
            ExcelReader excel = null;

            var resourcePath = "erice_maui.Resources.Assets.Erice2.xlsx";

            try
            {
                excel = new ExcelReader(resourcePath, sheet);
                bool breakCheck = false;
                while (!breakCheck)
                {
                    if (row == 0)
                    {
                        for (int i = start; i <= finish; i++)
                        {
                            string cellValue = excel.ReadCell(i, reading);

                            if (int.TryParse(cellValue, out int cellIntValue))
                            {
                                if ((age >= cellIntValue) && (cellIntValue != 0) && (reading == 1))
                                {
                                    start = i - 3;
                                    finish = i;
                                    reading = 3;
                                    break;
                                }
                                else if ((age < 49 && spol == "Z") && (reading == 1))
                                {
                                    start = 20;
                                    finish = 23;
                                    reading = 3;
                                    break;
                                }
                                else if ((age < 39 && spol == "M") && (reading == 1))
                                {
                                    start = 20;
                                    finish = 23;
                                    reading = 3;
                                    break;
                                }
                                else if ((sbp >= cellIntValue) && reading == 3)
                                {
                                    row = i;
                                    start = 4;
                                    finish = 17;
                                    reading = 1;
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to parse cell value to integer.");
                            }
                        }
                    }
                    else
                    {
                        for (int i = start; i <= finish; i++)
                        {
                            string cellValue = excel.ReadCell(reading, i);

                            if (diabetic && (cellValue == "Diabetics") && (reading == 1))
                            {
                                start = 4;
                                finish = 11;
                                reading = 2;
                                break;
                            }
                            else if (!diabetic && (cellValue == "Non diabetics") && (reading == 1))
                            {
                                start = 12;
                                finish = 19;
                                reading = 2;
                                break;
                            }
                            else if (smoker && (cellValue == "Smokers") && (reading == 2))
                            {
                                start += 4;
                                reading = 3;
                                break;
                            }
                            else if (!smoker && (cellValue == "Non smokers") && (reading == 2))
                            {
                                finish -= 4;
                                reading = 3;
                                break;
                            }

                            else if (double.TryParse(cellValue, out double cellIntValue))
                            {
                                if ((chol < cellIntValue) && reading == 3)
                                {
                                    col = i;
                                    breakCheck = true;
                                    break;
                                }
                                else if ((chol > 7.8) && reading == 3)
                                {
                                    col = finish;
                                    breakCheck = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                string result = excel.ReadCell(row, col);
                int.TryParse(result, out int intResult);
                string riskLevel = "";
                string chances = "";

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
                DisplayAlert("Rezultat", $"Rezultat: {result} - {riskLevel}\nSanse: {chances}", "OK");
            }

            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                if (excel != null)
                {
                    excel.Close();
                }
            }


        }

    }

}
