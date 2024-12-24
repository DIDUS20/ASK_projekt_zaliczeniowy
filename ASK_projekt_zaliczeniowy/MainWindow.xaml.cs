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
        string disp = "0000"; // offset

        string?[] memory = new string[65536]; // pamięć
        ushort addresation = 0;

        string[] stos = new string[65536];  // stos
        int sp = 0; //wskaznikStosu

        /// dla combo boxów w MOV | XCHG
        string[] regs = {"AX", "BX", "CX", "DX"};

        public MainWindow()
        { 
            InitializeComponent();
            StackPointer.Text = sp.ToString();
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
            SrcMOV.SelectedItem = null;
            DestMOV.SelectedItem = null;
            SrcXCHG.SelectedItem = null;
            DestXCHG.SelectedItem = null;
            StackReg.SelectedItem = null;
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
            memory = new string[65536];
            stos = new string[65536];

            StackList.Items.Clear();
            LogList.Items.Clear();
            MemoList.Items.Clear();
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

        /// 5. Przycisk Random 
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
                                            if (MOVMemo(bx, "0", "AX", 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", "AX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", "BX", 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", "BX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", "CX", 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", "CX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (MOVMemo(bx, "0", "DX", 1)) done = true;
                                            break;
                                        case "BP":
                                            if (MOVMemo(bp, "0", "DX", 1)) done = true;
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
                                            if (MOVMemo("0", si, "AX", 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, "AX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, "BX", 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, "BX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, "CX", 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, "CX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (MOVMemo("0", si, "DX", 1)) done = true;
                                            break;
                                        case "DI":
                                            if (MOVMemo("0", di, "DX", 1)) done = true;
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
                                            if (MOVMemo(bx, si, "AX", 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, "AX", 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, "AX", 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, "AX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, "BX", 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, "BX", 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, "BX", 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, "BX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, "CX", 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, "CX", 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, "CX", 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, "CX", 1)) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (MOVMemo(bx, si, "DX", 1)) done = true;
                                            break;
                                        case "SI+BP":
                                            if (MOVMemo(bp, si, "DX", 1)) done = true;
                                            break;
                                        case "DI+BX":
                                            if (MOVMemo(bx, di, "DX", 1)) done = true;
                                            break;
                                        case "DI+BP":
                                            if (MOVMemo(bp, di, "DX", 1)) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                }

                // MessageBox.Show($"MOV {first} {second} {direction} {typeOfAddresation}");

                if (done && first != null && second != null)
                {
                    UpdateMemoViewMOV(first, second);
                    UpdateRegsView();
                }
            }
        }

        // Aktualizacja Rejestru operacji i pamięci
        private void UpdateMemoViewMOV(string reg, string based)
        {
            MemoList.Items.Clear();

            if (direction == "Z rejestru do pamięci") LogList.Items.Insert(0,$"MOV [{based+"+"+disp.ToString()}] {reg}");
            else if (direction == "Z pamięci do rejestru") LogList.Items.Insert(0,$"MOV {reg} [{based + "+" + disp.ToString()}]");

            for (int i = 0; i < memory.Length; i++)
            {
                if (memory[i] != null)
                {
                    MemoList.Items.Add($"{i.ToString("X")}: {memory[i]}");
                }
            }
        }

        private void UpdateMemoViewXCHG(string reg, string based)
        {
            MemoList.Items.Clear();

            if (direction == "Z rejestru do pamięci") LogList.Items.Insert(0, $"XChG [{based + "+" + disp.ToString()}] {reg}");
            else if (direction == "Z pamięci do rejestru") LogList.Items.Insert(0, $"XCHG {reg} [{based + "+" + disp.ToString()}]");

            for (int i = 0; i < memory.Length; i++)
            {
                if (memory[i] != null)
                {
                    MemoList.Items.Add($"{i.ToString("X")}: {memory[i]}");
                }
            }
        }

        /// 10. Przycisk XCHG dla pamięci
        private void XCHGMemory(object sender, RoutedEventArgs e)
        {
            bool done = false;

            ComboBoxItem firstBox = (ComboBoxItem)MemoRej.SelectedItem;

            if (firstBox != null && MemoAdres.SelectedItem != null)
            {
                string? first = firstBox.Content.ToString();
                string? second = MemoAdres.SelectedItem.ToString();

                    switch (typeOfAddresation)
                    {
                        case "bazowy":
                            switch (first) // Rejestr na którym jest operacja
                            {
                                case "AX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (XCHGMemo(bx, "0", "AX")) done = true;
                                            break;
                                        case "BP":
                                            if (XCHGMemo(bp, "0", "AX")) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (XCHGMemo(bx, "0", "BX")) done = true;
                                            break;
                                        case "BP":
                                            if (XCHGMemo(bp, "0", "BX")) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (XCHGMemo(bx, "0", "CX")) done = true;
                                            break;
                                        case "BP":
                                            if (XCHGMemo(bp, "0", "CX")) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "BX":
                                            if (XCHGMemo(bx, "0", "DX")) done = true;
                                            break;
                                        case "BP":
                                            if (XCHGMemo(bp, "0", "DX")) done = true;
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
                                            if (XCHGMemo("0", si, "AX")) done = true;
                                            break;
                                        case "DI":
                                            if (XCHGMemo("0", di, "AX")) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (XCHGMemo("0", si, "BX")) done = true;
                                            break;
                                        case "DI":
                                            if (XCHGMemo("0", di, "BX")) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (XCHGMemo("0", si, "CX")) done = true;
                                            break;
                                        case "DI":
                                            if (XCHGMemo("0", di, "CX")) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI":
                                            if (XCHGMemo("0", si, "DX")) done = true;
                                            break;
                                        case "DI":
                                            if (XCHGMemo("0", di, "DX")) done = true;
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
                                            if (XCHGMemo(bx, si, "AX")) done = true;
                                            break;
                                        case "SI+BP":
                                            if (XCHGMemo(bp, si, "AX")) done = true;
                                            break;
                                        case "DI+BX":
                                            if (XCHGMemo(bx, di, "AX")) done = true;
                                            break;
                                        case "DI+BP":
                                            if (XCHGMemo(bp, di, "AX")) done = true;
                                            break;
                                    }
                                    break;
                                case "BX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (XCHGMemo(bx, si, "BX")) done = true;
                                            break;
                                        case "SI+BP":
                                            if (XCHGMemo(bp, si, "BX")) done = true;
                                            break;
                                        case "DI+BX":
                                            if (XCHGMemo(bx, di, "BX")) done = true;
                                            break;
                                        case "DI+BP":
                                            if (XCHGMemo(bp, di, "BX")) done = true;
                                            break;
                                    }
                                    break;
                                case "CX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (XCHGMemo(bx, si, "CX")) done = true;
                                            break;
                                        case "SI+BP":
                                            if (XCHGMemo(bp, si, "CX")) done = true;
                                            break;
                                        case "DI+BX":
                                            if (XCHGMemo(bx, di, "CX")) done = true;
                                            break;
                                        case "DI+BP":
                                            if (XCHGMemo(bp, di, "CX")) done = true;
                                            break;
                                    }
                                    break;
                                case "DX":
                                    switch (second) // Adresacja Rejestr 
                                    {
                                        case "SI+BX":
                                            if (XCHGMemo(bx, si, "DX")) done = true;
                                            break;
                                        case "SI+BP":
                                            if (XCHGMemo(bp, si, "DX")) done = true;
                                            break;
                                        case "DI+BX":
                                            if (XCHGMemo(bx, di, "DX")) done = true;
                                            break;
                                        case "DI+BP":
                                            if (XCHGMemo(bp, di, "DX")) done = true;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                
                if (done && first != null && second != null)
                {
                    UpdateRegsView();
                    UpdateMemoViewXCHG(first, second);
                }
            }
        }

        /// 11. Przycisk PUSH stos
        string selected = "";           // Rejestr
        string selected_value = "";     // Wartość rejestru
        private void StackPush(object sender, RoutedEventArgs e)
        {
            StackRegData();
            if (selected != null && selected != "")
            {
                stos[sp] = selected_value.Substring(0,2);
                stos[sp+1] = selected_value.Substring(2, 2);
                sp+=2;

                // Update view
                StackPointer.Text = sp.ToString();
                LogList.Items.Insert(0,$"PUSH {selected}");
                UpdateStckView();
            }
        }

        /// 12. Przycisk POP stos
        private void StackPop(object sender, RoutedEventArgs e)
        {
            StackRegData();
            if (selected != null && selected != "" && sp > 0)
            {
                string temp = stos[sp-2] + stos[sp-1];
                stos[sp - 1] = "";
                stos[sp - 2] = "";
                switch (selected)
                {
                    case "AX":
                        ax = temp;
                        break;
                    case "BX":
                        bx = temp;
                        break;
                    case "CX":
                        cx = temp;
                        break;
                    case "DX":
                        dx = temp;
                        break;
                }
                sp -= 2;

                // Update view
                StackPointer.Text = sp.ToString();
                LogList.Items.Insert(0, $"POP {selected}");
                UpdateRegsView();
                UpdateStckView();
            }
        }

        private void UpdateStckView()
        {
            StackList.Items.Clear();
            for (int i = 0; i < stos.Length; i++)
            {
                if(stos[i] != "" && stos[i] != null)
                {
                    StackList.Items.Insert(0,$"{i}: {stos[i]}");
                }
            }
        }
        // Wybór rejestru w operacjach na stosie
        private void StackRegData()
        {
            ComboBoxItem combo = (ComboBoxItem)StackReg.SelectedItem;
            if (combo != null)
            {
                string? s = combo.Content.ToString();
                switch (s)
                {
                    case "AX":
                        selected = "AX";
                        selected_value = ax;
                        break;
                    case "BX":
                        selected = "BX";
                        selected_value = bx;
                        break;
                    case "CX":
                        selected = "CX";
                        selected_value = cx;
                        break;
                    case "DX":
                        selected = "DX";
                        selected_value = dx;
                        break;
                }
            }
        }

        // Sprawdź input string 
        private string? StringHEX(string s)
        {
            s = s.Trim().ToUpper();
            char[] alphabet = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            if (s.Length == 4 && s != "")
            {
                foreach (char c in s)
                {
                    if (!alphabet.Contains(c))
                        return null;
                }

                return s;
            }
            else return null;
            
        }

        // Polecenie MOV Pamięć
        private bool MOVMemo(string baseReg , string indexReg , string reg , int kierunek = 0)
        {
            int first = Convert.ToInt32(baseReg,16);
            int second = Convert.ToInt32(indexReg,16);
            int third = Convert.ToInt32(disp,16);
            int address_one = CheckAddress(first+second+third);
            int address_two = CheckAddress(first+second+third+1);

            if(kierunek == 0) // Z rejestru do pamięci
            {
                try
                {
                    
                    memory[address_one] = reg.Substring(2,2);
                    memory[address_two] = reg.Substring(0, 2);
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else if(kierunek == 1) // Z pamięci do rejestru
            {
                try
                {
                        string temp = $"{memory[address_two]}{memory[address_one]}";
                        string? stemp = StringHEX(temp);

                        if (stemp != null)
                        {
                            switch (reg)
                            {
                                case "AX":
                                    ax = stemp;
                                    break;
                                case "BX":
                                    bx = stemp;
                                    break;
                                case "CX":
                                    cx = stemp;
                                    break;
                                case "DX":
                                    dx = stemp;
                                    break;
                            }
                        }

                        memory[address_one] = null;
                        memory[address_two] = null;
                        return true;
                    
                }
                catch
                {
                    return false;
                }
            }
            return false;
            
        }

        // Polecenie XCHG Pamięć
        private bool XCHGMemo(string baseReg, string indexReg, string reg, int kierunek = 0)
        {
            int first = Convert.ToInt32(baseReg, 16);
            int second = Convert.ToInt32(indexReg, 16);
            int third = Convert.ToInt32(disp, 16);
            int address_one = CheckAddress(first + second + third);
            int address_two = CheckAddress(first + second + third+1);
            try
            {
                if (memory[address_one] != null && memory[address_two] != null)
                {
                    string temp = $"{memory[address_two]}{memory[address_one]}";
                    string? stemp = StringHEX(temp);
                    if (stemp != null)
                    {
                        switch (reg)
                        {
                            case "AX":
                                memory[address_one] = ax.Substring(2, 2);
                                memory[address_two] = ax.Substring(0, 2);
                                ax = stemp;
                                break;
                            case "BX":
                                memory[address_one] = bx.Substring(2, 2);
                                memory[address_two] = bx.Substring(0, 2);
                                bx = stemp;
                                break;
                            case "CX":
                                memory[address_one] = cx.Substring(2, 2);
                                memory[address_two] = cx.Substring(0, 2);
                                cx = stemp;
                                break;
                            case "DX":
                                memory[address_one] = dx.Substring(2, 2);
                                memory[address_two] = dx.Substring(0, 2);
                                dx = stemp;
                                break;

                        }
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Sprawdzanie poprawności adresu
        private int CheckAddress(int address)
        {
            addresation = Convert.ToUInt16(address);
            if (address > 65536)       // Przepełnienie adresu
                return (address%65536);
            else
                return address;
        }


    }
}