using ElectricalPOS.Utilities;
using ElectricalPOS.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ElectricalPOS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register converters for XAML
            RegisterConverters();

            // Set default culture for currency formatting
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("ar-SY");
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("ar-SY");
        }

        private void RegisterConverters()
        {
            // Register custom converters (only if not already defined in App.xaml)
            if (!Resources.Contains("FilterPeriodToBooleanConverter"))
            {
                var converter = new FilterPeriodToBooleanConverter();
                Resources.Add("FilterPeriodToBooleanConverter", converter);
            }
            if (!Resources.Contains("DecimalToCurrencyConverter"))
            {
                var decimalConverter = new DecimalToCurrencyConverter();
                Resources.Add("DecimalToCurrencyConverter", decimalConverter);
            }
            if (!Resources.Contains("IntToStringConverter"))
            {
                var intConverter = new IntToStringConverter();
                Resources.Add("IntToStringConverter", intConverter);
            }
            if (!Resources.Contains("StringToVisibilityConverter"))
            {
                var stringConverter = new StringToVisibilityConverter();
                Resources.Add("StringToVisibilityConverter", stringConverter);
            }
            if (!Resources.Contains("BooleanToVisibilityConverter"))
            {
                var boolConverter = new BooleanToVisibilityConverter();
                Resources.Add("BooleanToVisibilityConverter", boolConverter);
            }
        }
    }
}