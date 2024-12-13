using System.Configuration;
using System.Data;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Transactions;
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

        string ax = "0000";
        string bx = "0000";
        string cx = "0000";
        string dx = "0000";

        string si = "0000";
        string di = "0000";
        string bp = "0000";
        string disp = "0000";

        ushort[] memory = new ushort[65536]; // pamięć

        ushort[] stos = new ushort[65536];  // stos
        int wskaznikStosu = 0;

        /// dla combo boxów w MOV | XCHG
        string[] regs = {"AX", "BX", "CX", "DX"};

        public MainWindow()
        {
            InitializeComponent();

            // Init & Clear
            InitializeRegsInCombos();
            ClearInputs();
        }

        private void ClearInputs()
        {
            AXInput.Text = BXInput.Text = CXInput.Text = DXInput.Text = SIInput.Text = DIInput.Text = BPInput.Text = DISPInput.Text = "";
        }
        
        private void InitializeRegsInCombos()
        {
            // MOV ComboBox
            SrcMOV.ItemsSource = regs;
            DestMOV.ItemsSource = regs;

            // XCHG ComboBox
            SrcXCHG.ItemsSource = regs;
            DestXCHG.ItemsSource = regs;
        }

        private void UpdateRegsView()
        {
            AXView.Text = ax;
            BXView.Text = bx;
            CXView.Text = cx;
            DXView.Text = dx;
            SIView.Text = si;
            DIView.Text = di;
            BPView.Text = bp;
            DISPView.Text = disp;
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
                    value = StringHEX(AXInput.Text);
                    if (value != null)
                    {
                        ax = value;
                        done = true;
                    }
                    break;
                case "BX":
                    value = StringHEX(BXInput.Text);
                    if (value != null)
                    {
                        bx = value;
                        done = true;
                    }
                    break;
                case "CX":
                    value = StringHEX(CXInput.Text);
                    if (value != null)
                    {
                        cx = value;
                        done = true;
                    }
                    break;
                case "DX":
                    value = StringHEX(DXInput.Text);
                    if (value != null)
                    {
                        dx = value;
                        done = true;
                    }
                    break;
                case "SI":
                    value = StringHEX(SIInput.Text);
                    if (value != null)
                    {
                        si = value;
                        done = true;
                    }
                    break;
                case "DI":
                    value = StringHEX(DIInput.Text);
                    if (value != null)
                    {
                        di = value;
                        done = true;
                    }
                    break;
                case "BP":
                    value = StringHEX(BPInput.Text);
                    if (value != null)
                    {
                        bp = value;
                        done = true;
                    }
                    break;
                case "DISP":
                    value = StringHEX(DISPInput.Text);
                    if (value != null)
                    {
                        disp = value;
                        done = true;
                    }
                    break;
            }

            // Dodaj wpis do logu
            if (done) LogList.Items.Add($"MOV {tag} {value}");
            UpdateRegsView(); // Aktualizuj Widok
        }

        /// 2. Przycisk Reset
        private void ResetClick(object sender, RoutedEventArgs e)
        {
            // Czyszczenie wartości
            ax = bx = cx = dx = si = di = bp = disp = "0000";
            AXView.Text = BXView.Text = CXView.Text = DXView.Text = SIView.Text = DIView.Text = BPView.Text = DISPView.Text = "0000";
            LogList.Items.Clear();
            // Dodaj wpis do logu
            LogList.Items.Add("---Reset---");
        }

        /// 3. Przycisk MOV
        private void MOVClick(object sender, RoutedEventArgs e)
        {
            string? src = SrcMOV.Text;
            string? des = DestMOV.Text;

            if (src != null && des != null && src != "" && des != "" && des != src) //Control
            {
                switch (src)
                {
                    case "AX":
                        switch (des)
                        {
                            case "BX":
                                bx = ax;
                                break;
                            case "CX":
                                cx = ax;
                                break;
                            case "DX":
                                dx = ax;
                                break;
                        }
                        break;
                    case "BX":
                        switch (des)
                        {
                            case "AX":
                                ax = bx;
                                break;
                            case "CX":
                                cx = bx;
                                break;
                            case "DX":
                                dx = bx;
                                break;
                        }
                        break;
                    case "CX":
                        switch (des)
                        {
                            case "AX":
                                ax = cx;
                                break;
                            case "BX":
                                bx = cx;
                                break;
                            case "DX":
                                dx = cx;
                                break;
                        }
                        break;
                    case "DX":
                        switch (des)
                        {
                            case "AX":
                                ax = dx;
                                break;
                            case "BX":
                                bx = dx;
                                break;
                            case "CX":
                                cx = dx;
                                break;
                        }
                        break;
                }

                LogList.Items.Add($"MOV {des} {src}");
                UpdateRegsView();
            }
            
        }

        /// 4. Przycisk XCHG
        private void XCHGClick(object sender, RoutedEventArgs e)
        {
            string? src = SrcXCHG.Text;
            string? des = DestXCHG.Text;

            if (src != null && des != null && src != "" && des != "" && des != src) //Control
            {
                string temp = "";
                switch (src)
                {
                    case "AX":
                        switch (des)
                        {
                            case "BX":
                                temp = bx;
                                bx = ax;
                                break;
                            case "CX":
                                temp = cx;
                                cx = ax;
                                break;
                            case "DX":
                                temp = dx;
                                dx = ax;
                                break;
                        }
                        ax = temp;
                        break;
                    case "BX":
                        switch (des)
                        {
                            case "AX":
                                temp = ax;
                                ax = bx;
                                break;
                            case "CX":
                                temp = cx;
                                cx = bx;
                                break;
                            case "DX":
                                temp = dx;
                                dx = bx;
                                break;
                        }
                        bx = temp;
                        break;
                    case "CX":
                        switch (des)
                        {
                            case "AX":
                                temp = ax;
                                ax = cx;
                                break;
                            case "BX":
                                temp = bx;
                                bx = cx;
                                break;
                            case "DX":
                                temp = dx;
                                dx = cx;
                                break;
                        }
                        cx = temp;
                        break;
                    case "DX":
                        switch (des)
                        {
                            case "AX":
                                temp = ax;
                                ax = dx;
                                break;
                            case "BX":
                                temp = bx;
                                bx = dx;
                                break;
                            case "CX":
                                temp = cx;
                                cx = dx;
                                break;
                        }
                        dx = temp;
                        break;
                }

                LogList.Items.Add($"XCHG {des} {src}");
                UpdateRegsView();
            }

        }

        /// 5. Przycisk Random /
        private void RandomClick(object sender, RoutedEventArgs e)
        {
            Random dice = new Random();

            AXInput.Text = ((ushort)dice.Next(0, 65536)).ToString("X4");
            BXInput.Text = ((ushort)dice.Next(0, 65536)).ToString("X4");
            CXInput.Text = ((ushort)dice.Next(0, 65536)).ToString("X4");
            DXInput.Text = ((ushort)dice.Next(0, 65536)).ToString("X4");

            UpdateRegsView();
        }

        /// 6. Przycisk Wyczyść
        private void ClearClick(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }

        /// 7. Przycisk Wybrania kierunku wymiany danych w pamięci
        private void ChangeDirectionOfMemo(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem choice = (ComboBoxItem) DirMemo.SelectedItem;
            string? value = choice.Content.ToString();
            
            if (value == "Z rejestru do pamięci")
            {
                MemoRejLabel.Content = "Przenieś dane z rejestru ";
                MemoAdresLabel.Content = "do pamięci bazując na rejestrze ";
            }
            else if (value == "Z pamięci do rejestru")
            {
                MemoRejLabel.Content = "Przenieś dane z pamięci do rejestru ";
                MemoAdresLabel.Content = "bazując na rejestrze ";
            }
        }

        /// 8. Przycisk wybrania typu adresowania
        string typeOfAddresation = "";
        string[] BaseList = { "BX", "BP"};
        string[] IndexList = { "SI", "DI"};
        string[] IndexBaseList = { "SI+BX", "SI+BP", "DI+BX", "DI+BP" };
        private void ChangeTypeMemo(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var tag = button?.Tag.ToString();
            MemoAdres.Items.Clear();
            switch (tag)
            {
                case "bazowy":
                    // Bazowy typ adresacji
                    foreach( var item in BaseList) MemoAdres.Items.Add(item);
                    typeOfAddresation = tag;
                    break;
                case "indeksowy":
                    // Indeksowy typ adresacji
                    foreach (var item in IndexList) MemoAdres.Items.Add(item);
                    typeOfAddresation = tag;
                    break;
                case "indeksowo-bazowy":
                    // Indeksowo-bazowy typ adresacji
                    foreach (var item in IndexBaseList) MemoAdres.Items.Add(item);
                    typeOfAddresation = tag;
                    break;
                default:
                    typeOfAddresation = "";
                    break;
            }
        }

        /// 9. Przycisk MOV dla pamięci
        private void MOVMemory(object sender, RoutedEventArgs e)
        {   
            bool done = false;

            ComboBoxItem firstBox = (ComboBoxItem)MemoRej.SelectedItem;
            string? first = firstBox.Content.ToString();

            ComboBoxItem secondBox = (ComboBoxItem)MemoRej.SelectedItem;
            string? second = secondBox.Content.ToString();

            switch (typeOfAddresation)
            {
                case "bazowy":
                    switch (first) // Rejestr
                    {
                        case "AX":
                            switch (second) // Adresacja Rejestr 
                            {
                                case "BX":
                                    
                                    break;
                                case "BP":
                                    break;
                            }
                            break;
                        case "BX":
                            break;
                        case "CX":
                            break;
                        case "DX":
                            break;
                    }
                    break;
                case "indeksowy":
                    switch (first)
                    {
                        case "AX":
                            break;
                        case "BX":
                            break;
                        case "CX":
                            break;
                        case "DX":
                            break;
                    }
                    break;
                case "indeksowo-bazowy":
                    switch (first)
                    {
                        case "AX":
                            break;
                        case "BX":
                            break;
                        case "CX":
                            break;
                        case "DX":
                            break;
                    }
                    break;
            }
        }

        /// 10. Przycisk XCHG dla pamięci
        private void XCHGMemory(object sender, RoutedEventArgs e)
        {

        }

        private string? StringHEX(string s)
        {
            s = s.Trim();
            ushort rec;
            if (ushort.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out rec))
            {
                if (s.Length == 1) return "000" + s;
                else if (s.Length == 2) return "00" + s;
                else if (s.Length == 3) return "0" + s;
                else return s;
            }
            else return null;
            
        }

        private bool MOVRegToMem(string baseReg, string indexReg, string value)
        {
            try
            {
                ushort address = (ushort)(ushort.Parse(baseReg) + ushort.Parse(indexReg) + ushort.Parse(disp));
                memory[address] = ushort.Parse(value);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    
        private bool MOVMemToReg(string baseReg, string indexReg, string value)
        {
            try
            {
                ushort address = (ushort)(ushort.Parse(baseReg) + ushort.Parse(indexReg) + ushort.Parse(disp));
                memory[address] = ushort.Parse(value);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}