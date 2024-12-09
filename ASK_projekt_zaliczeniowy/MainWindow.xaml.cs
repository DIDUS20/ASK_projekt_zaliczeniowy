using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASK_projekt_zaliczeniowy
{
    public partial class MainWindow : Window
    {
        
        string ax, bx, cx, dx;
        string si, di, bp, disp;

        string XCHG;
        string[] TABLICA = new string[65536]; // pamięć
        string starszyAdres, mlodszyAdres;

        int siDec, diDec, dispDec, sumDec;

        int bxDec, bpDec;

        string sumHex, pobierz, odczyt, zamiana;

        string[] STOS = new string[65536];
        int wskaznikStosu = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// 1. Przycisk Przypisz
        private void SaveRegister_Click(object sender, RoutedEventArgs e)
        {
            // Obsługa zapisu rejestru
            var button = sender as FrameworkElement;
            var tag = button?.Tag.ToString(); // Pobranie tagu przycisku (nazwa zmiennej)
            string? value = "";
            bool done = false;

            // Pobierz wartość z odpowiedniego pola tekstowego wpisz do zmiennej i wyświetl w rejestrze
            switch (tag)
            {
                case "AX":
                    value = StringHEX(AXInput.Text, ax);
                    if (value != null)
                    {
                        done = true;
                        AXView.Text = value;
                    }
                    break;
                case "BX":
                    value = StringHEX(BXInput.Text, bx);
                    if (value != null)
                    {
                        done = true;
                        BXView.Text = value;
                    }
                    break;
                case "CX":
                    value = StringHEX(CXInput.Text, cx);
                    if (value != null)
                    {
                        done = true;
                        CXView.Text = value;
                    }
                    break;
                case "DX":
                    value = StringHEX(DXInput.Text, dx);
                    if (value != null)
                    {
                        done = true;
                        DXView.Text = value;
                    }
                    break;
                case "SI":
                    value = StringHEX(SIInput.Text, si);
                    if (value != null)
                    {
                        done = true;
                        SIView.Text = value;
                    }
                    break;
                case "DI":
                    value = StringHEX(SIInput.Text, di);
                    if (value != null)
                    {
                        done = true;
                        DIView.Text = value;
                    }
                    break;
                case "BP":
                    value = StringHEX(BPInput.Text, bp);
                    if (value != null)
                    {
                        done = true;
                        BPView.Text = value;
                    }
                    break;
                case "DISP":
                    value = StringHEX(DISPInput.Text, disp);
                    if (value != null)
                    {
                        done = true;
                        DISPView.Text = value;
                    }
                    break;

            }

            // Dodaj wpis do logu
            if (done) LogList.Items.Add($"MOV {tag} {value}");
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            // Reset wartości rejestrów i wyświetleń
            ax = bx = cx = dx = si = di = bp = disp = "0";
            AXView.Text = BXView.Text = CXView.Text = DXView.Text = SIView.Text = DIView.Text = BPView.Text = DISPView.Text = "0000";

            // Dodaj wpis do logu
            LogList.Items.Add("---Reset---");
        }
        
        private string? StringHEX(string s, string saver)
        {
            ushort rec;
            if (ushort.TryParse(s, System.Globalization.NumberStyles.HexNumber, null,out rec)) return rec.ToString("X4");
            else return null;
        }
    }
}