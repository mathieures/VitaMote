using System.Collections;
using System.Collections.Generic;
using Android.Views;

namespace VitaMote
{
    public static class ButtonHelper
    {
        // Official enum from vitasdk (https://github.com/vitasdk/vita-headers/blob/master/include/psp2/ctrl.h)
        public static int SCE_CTRL_SELECT   = 0x00000001;        //!< Select button.
	    //public static int SCE_CTRL_L3       = 0x00000002;        //!< L3 button.
	    //public static int SCE_CTRL_R3       = 0x00000004;        //!< R3 button.
	    public static int SCE_CTRL_START    = 0x00000008;        //!< Start button.
	    public static int SCE_CTRL_UP       = 0x00000010;        //!< Up D-Pad button.
	    public static int SCE_CTRL_RIGHT    = 0x00000020;        //!< Right D-Pad button.
	    public static int SCE_CTRL_DOWN     = 0x00000040;        //!< Down D-Pad button.
	    public static int SCE_CTRL_LEFT     = 0x00000080;        //!< Left D-Pad button.
	    public static int SCE_CTRL_LTRIGGER = 0x00000100;        //!< Left trigger.
	    //public static int SCE_CTRL_L2       = SCE_CTRL_LTRIGGER; //!< L2 button.
	    public static int SCE_CTRL_RTRIGGER = 0x00000200;        //!< Right trigger.
	    //public static int SCE_CTRL_R2       = SCE_CTRL_RTRIGGER; //!< R2 button.
	    //public static int SCE_CTRL_L1       = 0x00000400;        //!< L1 button.
	    //public static int SCE_CTRL_R1       = 0x00000800;        //!< R1 button.
	    public static int SCE_CTRL_TRIANGLE = 0x00001000;        //!< Triangle button.
        public static int SCE_CTRL_CIRCLE = 0x00002000;        //!< Circle button.
        public static int SCE_CTRL_CROSS    = 0x00004000;        //!< Cross button.
        public static int SCE_CTRL_SQUARE   = 0x00008000;        //!< Square button.

        //// Official enum from VitaPad (https://github.com/Rinnegatamante/VitaPad/blob/master/Server/source/main.c)
        //public static int LEFT_CLICK  = 0x08;
        //public static int RIGHT_CLICK = 0x10;

        // Maps every button to the corresponding keycode to send to the application
        public static Dictionary<int, Keycode> buttonToKeycode = new Dictionary<int, Keycode>
        {
            // SEL-STA
            { SCE_CTRL_SELECT, Keycode.DpadCenter },
            { SCE_CTRL_START, Keycode.Back },
            // DPAD
            { SCE_CTRL_UP, Keycode.DpadUp },
            { SCE_CTRL_RIGHT, Keycode.DpadRight },
            { SCE_CTRL_DOWN, Keycode.DpadDown },
            { SCE_CTRL_LEFT, Keycode.DpadLeft },
            // L-R Triggers
            { SCE_CTRL_LTRIGGER, Keycode.ButtonL1 },
            { SCE_CTRL_RTRIGGER, Keycode.ButtonR1 },
            // TCXS
            { SCE_CTRL_TRIANGLE, Keycode.ButtonY },
            { SCE_CTRL_CIRCLE, Keycode.ButtonB },
            { SCE_CTRL_CROSS, Keycode.ButtonA },
            { SCE_CTRL_SQUARE, Keycode.ButtonX },
        };
    }
}