using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace Arabish
{
    class HotKey
    {
        //enum of values of KEYS
        private enum fsModfiers
        {
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Window = 0x0008,
        }
        //ref to selected window
        private IntPtr myWindow;

        //Constructor
        public HotKey(IntPtr Wptr)
        {
            myWindow = Wptr;
        }

        //Register hot KEY for the programme 
        public void registerHotKeys()
        {
            //Register CTRL+E with ID=1
            RegisterHotKey(myWindow, 1, (uint)fsModfiers.Control, (uint)Keys.E);
            RegisterHotKey(myWindow, 2, (uint)fsModfiers.Control, (uint)Keys.Q);
        }
        //un register hot key
        public void unRegisterHotKeys()
        {
            UnregisterHotKey(myWindow, 1);
            UnregisterHotKey(myWindow, 2);
        }

        public void unRegisterHotkeyById(int ID)
        {
            UnregisterHotKey(myWindow, ID);
        }

        public void RegisterHotkeyById(int ID)
        {
            if (ID==1)
                RegisterHotKey(myWindow, 1, (uint)fsModfiers.Control, (uint)Keys.E);
            else if (ID==2)
                RegisterHotKey(myWindow, 2, (uint)fsModfiers.Control, (uint)Keys.Q);

        }
        //WINDOWS API fucnctions
        #region windowsAPI
            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vK);

            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hwnd, int id);
        #endregion
    }
}
