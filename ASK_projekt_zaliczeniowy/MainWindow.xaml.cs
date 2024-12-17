using System.Configuration;
using System.Data;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

        ushort address = 0;

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
            MemoAdres.Text = null;
            MemoRej.Text = null;
            DirMemo.SelectedItem = null;
            group0.IsChecked = false;
            group1.IsChecked = false;
            group2.IsChecked = false;
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
            if (done) LogList.Items.Insert(0,$"MOV {tag} {value}");
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
            LogList.Items.Insert(0,"---Reset---");
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

                LogList.Items.Insert(0,$"MOV {des} {src}");
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

                LogList.Items.Insert(0,$"XCHG {des} {src}");
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
        string direction = "";
        private void ChangeDirectionOfMemo(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem choice = (ComboBoxItem) DirMemo.SelectedItem;
            if (choice != null)
            {
                string? value = choice.Content.ToString();
                if (value == "Z rejestru do pamięci")
                {
                    MemoRejLabel.Content = "Przenieś dane z rejestru ";
                    MemoAdresLabel.Content = "do pamięci bazując na rejestrze ";
                    direction = value.ToString();
                }
                else if (value == "Z pamięci do rejestru")
                {
                    MemoRejLabel.Content = "Przenieś dane z pamięci do rejestru ";
                    MemoAdresLabel.Content = "bazując na rejestrze ";
                    direction = value.ToString();
                }
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

            if (firstBox != null && MemoAdres.SelectedItem != null)
            {
                string? first = firstBox.Content.ToString();
                string? second = MemoAdres.SelectedItem.ToString();

                if (direction == "Z rejestru do pamięci") // Z REJESTRU DO PAMIĘCI
                {
                    switch (typeOfAddresation)
                    {
                        case "bazowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", ax)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", ax)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", bx)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", bx)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", cx)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", cx)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", dx)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", dx)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "indeksowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, ax)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, ax)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, bx)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, bx)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, cx)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, cx)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, dx)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, dx)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "indeksowo-bazowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, ax)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, ax)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, ax)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, ax)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, bx)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, bx)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, bx)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, bx)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, cx)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, cx)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, cx)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, cx)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, dx)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, dx)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, dx)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, dx)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                }
                else if (direction == "Z pamięci do rejestru") // Z PAMIĘCI DO REJESTRU
                {
                    switch (typeOfAddresation)
                    {
                        case "bazowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", ax, 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", ax, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", bx, 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", bx, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", cx, 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", cx, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", dx, 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", dx, 1)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "indeksowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, ax, 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, ax, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, bx, 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, bx, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, cx, 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, cx, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, dx, 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, dx, 1)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "indeksowo-bazowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, ax, 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, ax, 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, ax, 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, ax, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, bx, 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, bx, 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, bx, 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, bx, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, cx, 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, cx, 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, cx, 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, cx, 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, dx, 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, dx, 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, dx, 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, dx, 1)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                }

                // MessageBox.Show($"MOV {first} {second} {direction} {typeOfAddresation}");

                if (done)
                {
                    UpdateRegsView();
                    UpdateMemoView(first, second);
                }
            }
        }

        // Aktualizacja Rejestru operacj i pamięci
        void UpdateMemoView(string reg, string based)
        {
            if (direction == "Z rejestru do pamięci") LogList.Items.Insert(0,$"MOV [{based+ "+" +disp.ToString()}] {reg}");
            else if (direction == "Z pamięci do rejestru") LogList.Items.Insert(0,$"MOV {reg} [{based + "+" + disp.ToString()}]");

            for (int i = 0; i < memory.Length; i++)
            {
                if (memory[i] != 0)
                {
                    MemoList.Items.Insert(0,$"{i}: {memory[i]}");
                }
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

        // Polecenie MOV Pamięć
        private bool MOVMemo(string baseReg , string indexReg , string reg , int kierunek = 0)
        {
            if(kierunek == 0) // Z rejestru do pamięci
            {
                try
                {
                    address = (ushort)(ushort.Parse(baseReg) + ushort.Parse(indexReg) + ushort.Parse(disp));
                    memory[address] = ushort.Parse(reg.Substring(0,2));
                    memory[address+1] = ushort.Parse(reg.Substring(2, 2));
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else  // Z pamięci do rejestru
            {
                try
                {
                    address = (ushort)(ushort.Parse(baseReg) + ushort.Parse(indexReg) + ushort.Parse(disp));
                    switch (reg)
                    {
                        case "AX":
                            ax = memory[address].ToString();
                            break;
                        case "BX":
                            bx = memory[address].ToString();
                            break;
                        case "CX":
                            cx = memory[address].ToString();
                            break;
                        case "DX":
                            dx = memory[address].ToString();
                            break;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
        }

        private void MemoAdres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}