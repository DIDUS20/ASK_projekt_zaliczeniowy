using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASK_projekt_zaliczeniowy
{
    public class CPU
    {
        // indeksy
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


    }
}
