using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Price_Checker.Services
{
    internal class FontManagerService
    {
        internal static readonly string FontFilePath = @"C:\ESTEBAN_JASMINE_PUPSMB\C#\Barcode-Scanner\PriceScannerV1\Price Checker\assets\Fonts\Schibsted_Grotesk\static\SchibstedGrotesk-Regular.ttf";

        private PrivateFontCollection privateFontCollection;
        private Font customFont;

        public FontManagerService()
        {
            privateFontCollection = new PrivateFontCollection();
            LoadFont();
        }

        private void LoadFont()
        {
            if (File.Exists(FontFilePath))
            {
                privateFontCollection.AddFontFile(FontFilePath);
                customFont = new Font(privateFontCollection.Families[0], 28f);
            }
        }

        public Font GetCustomFont()
        {
            return customFont;
        }

        internal void SetCustomFontForAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is TextBoxBase || control is Label || control is Button || control is CheckBox || control is RadioButton)
                {
                    control.Font = customFont;
                }

                if (control.Controls.Count > 0)
                {
                    SetCustomFontForAllControls(control);
                }
            }
        }
    }
}
