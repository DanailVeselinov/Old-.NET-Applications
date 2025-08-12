using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using BFDL;

namespace BetFairBot
{
    public class IsActiveTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var parInt = int.Parse(parameter.ToString());
            if (parInt == 0)
            {
                return value;
            }
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class PercentPerBudjetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrWhiteSpace(value.ToString()))
            {
                double budjet = 200;
                double newValue = 0;
                try
                {

                    newValue = double.Parse(value.ToString());
                    double.TryParse((App.Current.MainWindow.FindName("_Funds") as Label).Content.ToString(), out budjet);
                }
                catch (Exception)
                {
                }
                newValue = Math.Round((newValue * MainWindow.ReduceBallance(budjet)) / 100, 2);
                return newValue > 0 ? newValue.ToString() : "0";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrWhiteSpace(value.ToString()))
            {
                double budjet = 200;
                double newValue = 0;
                try
                {

                    newValue = double.Parse(value.ToString());
                    double.TryParse((App.Current.MainWindow.FindName("_Funds") as Label).Content.ToString(), out budjet);
                }
                catch (Exception)
                {
                }
                newValue = Math.Round((newValue / MainWindow.ReduceBallance(budjet)) * 100, 2);
                return newValue > 0 ? newValue.ToString() : "0";
            }
            return value;
        }
    }
    public class RGBConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int betlineId = (int)value;
                var colour = RGBConverterForeground.GetMeColour(betlineId);
                var brush = new System.Windows.Media.SolidColorBrush(colour);
                return brush;
            }
            catch (Exception)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RGBConverterForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int betlineId = (int)value;
                var colour = GetMeColour(betlineId);
                if ((colour.G + colour.B + colour.R) > 300)
                {
                    colour = System.Windows.Media.Colors.Black;
                }
                else
                {
                    colour = System.Windows.Media.Colors.White;
                }
                var brush = new System.Windows.Media.SolidColorBrush(colour);
                return brush;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static System.Windows.Media.Color GetMeColour(int betlineId)
        {
            var colour = new System.Windows.Media.Color();
            int betlineIdInt = betlineId % 64;
            int step = 8;
            int colourNumber = step * ((betlineIdInt % step) - 1) + (betlineIdInt / step);

            colour.R = (byte)((colourNumber / 16) * 85);
            colour.G = (byte)((colourNumber / 4) * 85);
            colour.B = (byte)((colourNumber % 4) * 85);
            colour.A = 255;
            return colour;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
    public class BetlineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isChecked = (bool)(App.Current.MainWindow.FindName("_showAllCB") as CheckBox).IsChecked;
            if (isChecked)
            {
                var context = new BFBDBEntities();
                var view = context.PlacedBets.ToList().OrderByDescending(b => b.datePlaced);
                return view;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    

    public class SelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = (value as BFDL.PlacedBets);
            if (item != null)
            {
                switch (item.marketName)
                {
                    case "Correct Score":
                        switch (item.Selection)
                        {
                            case "1":
                                return "0:0";
                            case "2":
                                return "0:1";
                            case "3":
                                return "0:2";
                            case "4":
                                return "0:3";
                            case "5":
                                return "1:0";
                            case "6":
                                return "1:1";
                            case "7":
                                return "1:2";
                            case "8":
                                return "1:3";
                            case "9":
                                return "2:0";
                            case "10":
                                return "2:1";
                            case "11":
                                return "2:2";
                            case "12":
                                return "2:3";
                            case "13":
                                return "3:0";
                            case "14":
                                return "3:1";
                            case "15":
                                return "3:2";
                            case "16":
                                return "3:3";
                            case "17":
                                return "Any Unquoted";
                            default:
                                return "Any unquoted";
                        }
                    case "Match Odds":
                        switch (item.Selection)
                        {
                            case "1":
                                return item.marketMenuPath.Split('\\').ToList().Last().Split(new[] { " v " }, StringSplitOptions.None).First();
                            case "2":
                                return item.marketMenuPath.Split('\\').ToList().Last().Split(new[] { " v " }, StringSplitOptions.None).Last();
                            case "3":
                                return "X";
                            default:
                                return item.Selection;
                        }
                    default:
                        return "Any Unquoted";
                }
            }
            return "Any Unquoted";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
