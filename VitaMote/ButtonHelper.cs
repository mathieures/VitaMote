using System.Collections;
using System.Collections.Generic;
using Android.Views;

namespace VitaMote
{
    class ButtonHelper
    {

        // Buttons
        // Type 1
        public const int btnL = 128;
        public const int btnD = 64;
        public const int btnR = 32;
        public const int btnU = 16;
        public const int btnSta = 8;
        public const int btnSel = 1;
        // Combos
        // Dpad Combos
        public const int btnLD = btnL + btnD;
        public const int btnLU = btnL + btnU;
        public const int btnDR = btnD + btnR;
        public const int btnRU = btnR + btnU;
        // Dpad + Sel or Sta
        public const int btnUSt = btnU + btnSta;
        public const int btnUSe = btnU + btnSel;
        public const int btnDSt = btnD + btnSta;
        public const int btnDSe = btnD + btnSel;
        public const int btnLSt = btnL + btnSta;
        public const int btnLSe = btnL + btnSel;
        public const int btnRSt = btnR + btnSta;
        public const int btnRSe = btnR + btnSel;
        public const int btnStSe = btnSta + btnSel;
        // Triple Combo (Dpad Combo + Sta)
        public const int btnLUSt = btnLU + btnSta;
        public const int btnLUSe = btnLU + btnSel;
        public const int btnLDSt = btnLD + btnSta;
        public const int btnLDSe = btnLD + btnSel;
        public const int btnRUSt = btnRU + btnSta;
        public const int btnRUSe = btnRU + btnSel;
        public const int btnDRSt = btnDR + btnSta;
        public const int btnDRSe = btnDR + btnSel;
        // Type 2
        public const int btnS = 128;
        public const int btnX = 64;
        public const int btnC = 32;
        public const int btnT = 16;
        public const int btnRt = 2;
        public const int btnLt = 1;
        // Double Combo
        public const int btnXS = btnX + btnS;
        public const int btnXC = btnX + btnC;
        public const int btnCT = btnC + btnT;
        public const int btnTS = btnT + btnS;
        public const int btnXLt = btnX + btnLt;
        public const int btnCLt = btnC + btnLt;
        public const int btnTLt = btnT + btnLt;
        public const int btnSLt = btnS + btnLt;
        public const int btnXRt = btnX + btnRt;
        public const int btnCRt = btnC + btnRt;
        public const int btnTRt = btnT + btnRt;
        public const int btnSRt = btnS + btnRt;
        public const int btnLtRt = btnLt + btnRt;
        // Triple Combo (L+R+SXCT)
        public const int btnLtRtC = btnLt + btnRt + btnC;
        public const int btnLtRtS = btnLt + btnRt + btnS;
        public const int btnLtRtT = btnLt + btnRt + btnT;
        public const int btnLtRtX = btnLt + btnRt + btnX;

        // Input to keycode conversions

        // DPAD
        public const Keycode bUp = Keycode.DpadUp;
        public const Keycode bDo = Keycode.DpadDown;
        public const Keycode bLe = Keycode.DpadLeft;
        public const Keycode bRi = Keycode.DpadRight;
        // STA-SEL
        public const Keycode bSt = Keycode.Back;
        public const Keycode bSe = Keycode.DpadCenter;
        // SXCT
        public const Keycode bS = Keycode.ButtonX;
        public const Keycode bX = Keycode.ButtonA;
        public const Keycode bC = Keycode.ButtonB;
        public const Keycode bT = Keycode.ButtonY;
        // L-R Triggers
        public const Keycode bLt = Keycode.ButtonL1;
        public const Keycode bRt = Keycode.ButtonR1;
        // Analog sticks
        // LEFT
        public const Keycode aLl = Keycode.A;
        public const Keycode aLr = Keycode.D;
        public const Keycode aLu = Keycode.W;
        public const Keycode aLd = Keycode.S;
        // RIGHT
        public const Keycode aRl = Keycode.J;
        public const Keycode aRr = Keycode.L;
        public const Keycode aRu = Keycode.I;
        public const Keycode aRd = Keycode.K;
    }
}