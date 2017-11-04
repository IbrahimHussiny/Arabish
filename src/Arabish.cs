using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Arabish
{
    public partial class Arabish : Form
    {


        //Our HotKey eX: CTRL+I
        private HotKey hotkey;
        //Ref o selected Window
        private IntPtr hWnd;
        // The path to the key where Windows looks for startup applications
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        //Dictionary to map each char 
        private Dictionary<char, char> myDictionary;
        //Dictionary to hold special arb to Eng chars ex:{}
        private Dictionary<char, char> specialArbToEngDic;
        //indicate if the dictionares were loaded or not
        private bool isLoaded;
        //indicates if user has disabled the application
        private bool isEnabled;
        //indicates if the user has enabled the Ctrl+Q mode
        private bool withoutSelectall;
        //indicates if the user has enabled the Ctrl+E mode
        private bool withSelectall;


        public Arabish()
        {
            InitializeComponent();
            //Don't show in task bar
            this.ShowInTaskbar = false;
            myDictionary = new Dictionary<char, char>();
            specialArbToEngDic = new Dictionary<char, char>();
            //Dectionaries are not yet loaded to decrease operations on Windows boot
            isLoaded = false;
            //Application is initially enabled with both modes

            //Run at start up
            rkApp.SetValue("Arabish", Application.ExecutablePath.ToString());
        }
        //ON Form Load register hotKey
        private void Form1_Load(object sender, EventArgs e)
        {
            hWnd = FindWindow(null, "Arabish");

            hotkey = new HotKey(hWnd);

            isEnabled = Properties.Settings.Default.isEnabled;
            Enable.Checked = Properties.Settings.Default.isEnabled;
            withoutSelectall = Properties.Settings.Default.withoutSelect;
            WithoutSelect.Checked = Properties.Settings.Default.withoutSelect;
            withSelectall = Properties.Settings.Default.withSelect;
            WithSelect.Checked = Properties.Settings.Default.withSelect;

            if (Enabled)
            {
                if (withSelectall)
                    hotkey.RegisterHotkeyById(2);
                if (withoutSelectall)
                    hotkey.RegisterHotkeyById(1);
            }
        }
        //to make main window unvisable
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Visible = false;
        }


        //Listen for HotKey trigger 
        protected override void WndProc(ref Message keyPressed)
        {
            //hotKey was triggred main code goes here
            if (keyPressed.Msg == 0x0312 && isEnabled)
            {

                //first time to run the programme 
                if (!isLoaded)
                {
                    setMyDictionaryData();
                    setSpecialDicData();
                    isLoaded = true;
                }
                IntPtr hotkeyID = keyPressed.WParam;

                if (hotkeyID.ToInt32() == 2 && !withSelectall || hotkeyID.ToInt32() == 1 && !withoutSelectall)
                    return;
                startOperations(hotkeyID.ToInt32());


            }
            base.WndProc(ref keyPressed);
        }

        #region ClipboardHandler

        //Set data to clipoard using thread
        private void startOperations(int select)
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate()
                {
                    try
                    {
                        //Clipboard.SetText(data);

                        Thread.Sleep(30);

                        if (select == 2)
                        {
                            Keyboard.SimulateKeyStroke('a', ctrl: true);
                            Thread.Sleep(30);
                        }
                        Keyboard.SimulateKeyStroke('c', ctrl: true);
                        Thread.Sleep(30);
                        String myContent = "";
                        if (Clipboard.ContainsText())
                        {
                            Thread.Sleep(10);
                            myContent = Clipboard.GetText();
                        }
                        Thread.Sleep(30);
                        //the user selected text
                            String returnedToUser = "";
                            //just to avoid run rime errors
                            if (!myContent.Equals(""))
                            {
                                if (isEnglish(myContent))
                                    returnedToUser = convert(myContent, false);
                                else
                                    returnedToUser = convert(myContent, true);


                                Thread.Sleep(30);
                                Clipboard.SetText(returnedToUser);
                                //setData(returnedToUser);
                                Thread.Sleep(30);
                                Keyboard.SimulateKeyStroke('v', ctrl: true);
                                Thread.Sleep(30);

                            }
                        }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                    try
                    {
                        Clipboard.Clear();
                        Thread.Sleep(30);
                    }
                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }

        #endregion

        #region dictionaryHandler
        //set MyDictionary data
        private void setMyDictionaryData()
        {
            //Eng to Arb Without Shift
            myDictionary.Add('`', 'ذ');
            myDictionary.Add('q', 'ض');
            myDictionary.Add('w', 'ص');
            myDictionary.Add('e', 'ث');
            myDictionary.Add('r', 'ق');
            myDictionary.Add('t', 'ف');
            myDictionary.Add('y', 'غ');
            myDictionary.Add('u', 'ع');
            myDictionary.Add('i', 'ه');
            myDictionary.Add('o', 'خ');
            myDictionary.Add('p', 'ح');
            myDictionary.Add('[', 'ج');
            myDictionary.Add(']', 'د');
            myDictionary.Add('a', 'ش');
            myDictionary.Add('s', 'س');
            myDictionary.Add('d', 'ي');
            myDictionary.Add('f', 'ب');
            myDictionary.Add('g', 'ل');
            myDictionary.Add('h', 'ا');
            myDictionary.Add('j', 'ت');
            myDictionary.Add('k', 'ن');
            myDictionary.Add('l', 'م');
            myDictionary.Add(';', 'ك');
            myDictionary.Add('\'', 'ط');
            myDictionary.Add('z', 'ئ');
            myDictionary.Add('x', 'ء');
            myDictionary.Add('c', 'ؤ');
            myDictionary.Add('v', 'ر');
            // myDictionary.Add('b', '\uFEFC');
            myDictionary.Add('n', 'ى');
            myDictionary.Add('m', 'ة');
            myDictionary.Add(',', 'و');
            myDictionary.Add('.', 'ز');
            myDictionary.Add('/', 'ظ');
            //With Shift
            myDictionary.Add('~', '\u0651');
            myDictionary.Add('Q', '\u064e');
            myDictionary.Add('W', '\u064b');
            myDictionary.Add('E', '\u064f');
            myDictionary.Add('R', '\u064c');
            //myDictionary.Add('T', '\uFEF9');
            myDictionary.Add('Y', 'إ');
            myDictionary.Add('U', '‘');
            myDictionary.Add('I', '÷');
            myDictionary.Add('O', '×');
            myDictionary.Add('P', '؛');
            myDictionary.Add('{', '<');
            myDictionary.Add('}', '>');
            myDictionary.Add('A', '\u0650');
            myDictionary.Add('S', '\u064d');
            myDictionary.Add('D', ']');
            myDictionary.Add('F', '[');
            //myDictionary.Add('G', '\uFEF7');
            myDictionary.Add('H', 'أ');
            myDictionary.Add('J', 'ـ');
            myDictionary.Add('K', '،');
            myDictionary.Add('L', '/');
            myDictionary.Add('Z', '~');
            myDictionary.Add('X', '\u0652');
            myDictionary.Add('C', '}');
            myDictionary.Add('V', '{');
            // myDictionary.Add('B', '\uFEF5');
            myDictionary.Add('N', 'آ');
            myDictionary.Add('M', '’');
            myDictionary.Add('<', ',');
            myDictionary.Add('>', '.');
            myDictionary.Add('?', '؟');

            //Without Shift Repeated
            myDictionary.Add('ذ', '`');
            myDictionary.Add('ض', 'q');
            myDictionary.Add('ص', 'w');
            myDictionary.Add('ث', 'e');
            myDictionary.Add('ق', 'r');
            myDictionary.Add('ف', 't');
            myDictionary.Add('غ', 'y');
            myDictionary.Add('ع', 'u');
            myDictionary.Add('ه', 'i');
            myDictionary.Add('خ', 'o');
            myDictionary.Add('ح', 'p');
            myDictionary.Add('ج', '[');
            myDictionary.Add('د', ']');
            myDictionary.Add('ش', 'a');
            myDictionary.Add('س', 's');
            myDictionary.Add('ي', 'd');
            myDictionary.Add('ب', 'f');
            myDictionary.Add('ل', 'g');
            myDictionary.Add('ا', 'h');
            myDictionary.Add('ت', 'j');
            myDictionary.Add('ن', 'k');
            myDictionary.Add('م', 'l');
            myDictionary.Add('ك', ';');
            myDictionary.Add('ط', '\'');
            myDictionary.Add('ئ', 'z');
            myDictionary.Add('ء', 'x');
            myDictionary.Add('ؤ', 'c');
            myDictionary.Add('ر', 'v');
            myDictionary.Add('\uFEFB', 'b');
            myDictionary.Add('ى', 'n');
            myDictionary.Add('ة', 'm');
            myDictionary.Add('و', ',');
            myDictionary.Add('ز', '.');
            myDictionary.Add('ظ', '/');
            //With Shift Repeated
            myDictionary.Add('\u0651', '~');
            myDictionary.Add(')', '(');
            myDictionary.Add('(', ')');
            myDictionary.Add('\u064e', 'Q');
            myDictionary.Add('\u064b', 'W');
            myDictionary.Add('\u064f', 'E');
            myDictionary.Add('\u064c', 'R');
            myDictionary.Add('\uFEF9', 'T');
            myDictionary.Add('إ', 'Y');
            myDictionary.Add('‘', 'U');
            myDictionary.Add('÷', 'I');
            myDictionary.Add('×', 'O');
            myDictionary.Add('؛', 'P');
            //myDictionary.Add('<', '{');
            //myDictionary.Add('>', '}');

            myDictionary.Add('\u0650', 'A');
            myDictionary.Add('\u064d', 'S');
            // myDictionary.Add(']', 'D');
            //myDictionary.Add('[', 'F');
            myDictionary.Add('\uFEF7', 'G');
            myDictionary.Add('أ', 'H');
            myDictionary.Add('ـ', 'J');
            myDictionary.Add('،', 'K');
            //myDictionary.Add('/', 'L');
            myDictionary.Add(':', ':');
            //myDictionary.Add('~', 'Z');
            myDictionary.Add('\u0652', 'X');
            //myDictionary.Add('}', 'C');
            myDictionary.Add('\uFEF5', 'B');
            // myDictionary.Add('{', 'V');
            myDictionary.Add('آ', 'N');
            myDictionary.Add('’', 'M');
            //myDictionary.Add(',', '<');
            //myDictionary.Add('.', '>');
            myDictionary.Add('؟', '?');
        }

        private void setSpecialDicData()
        {
            specialArbToEngDic.Add(']', 'D');
            specialArbToEngDic.Add('[', 'F');
            specialArbToEngDic.Add('/', 'L');
            specialArbToEngDic.Add('~', 'Z');
            specialArbToEngDic.Add('{', 'V');
            specialArbToEngDic.Add(',', '<');
            specialArbToEngDic.Add('.', '>');
            specialArbToEngDic.Add('<', '{');
            specialArbToEngDic.Add('>', '}');
            specialArbToEngDic.Add('}', 'C');
        }
        /*
         * converte the given string 
         * arb:bool value to manage special chars if conversion is from arb to Eng it is set as true
         */

        private String convert(String temp, bool arb)
        {
            String converted = "";

            for (int i = 0; i < temp.Length; i++)
            {
                if (myDictionary.ContainsKey(temp[i]))
                {
                    if (i < temp.Length - 1 && temp[i] == 'ل' && temp[i + 1] == 'ا')
                    {
                        converted += 'b';
                        i++;
                    }
                    else if (i < temp.Length - 1 && temp[i] == 'ل' && temp[i + 1] == '\u0622')
                    {
                        converted += "B";
                        i++;
                    }
                    else if (i < temp.Length - 1 && temp[i] == 'ل' && temp[i + 1] == 'أ')
                    {
                        converted += 'G';
                        i++;
                    }
                    else if (i < temp.Length - 1 && temp[i] == 'ل' && temp[i + 1] == 'إ')
                    {
                        converted += 'T';
                        i++;
                    }
                    else
                    {
                        if (arb && specialArbToEngDic.ContainsKey(temp[i]))
                        {
                            converted += specialArbToEngDic[temp[i]];
                        }
                        else
                            converted += myDictionary[temp[i]];
                    }

                }
                else
                {
                    if (temp[i] == 'b')
                    {
                        converted += "لا";
                    }
                    else if (temp[i] == 'B')
                    {
                        converted += "لآ";
                    }
                    else if (temp[i] == 'G')
                    {
                        converted += "لأ";
                    }
                    else if (temp[i] == 'T')
                    {
                        converted += "لإ";
                    }
                    else if (arb && specialArbToEngDic.ContainsKey(temp[i]))
                    {
                        converted += specialArbToEngDic[temp[i]];
                    }
                    else
                        converted += temp[i];
                }


            }

            return converted;
        }


        /*
         * detrmines if a given char is english or not
         */
        private bool isAlph(char a)
        {
            return (a >= 'a' && a <= 'z') || (a >= 'A' && a <= 'Z');
        }
        /*
         * an approximation that checks first 10 chars to manage special chars
         */
        private bool isEnglish(string a)
        {
            for (int i = 0; (a.Length <= 10) ? i < a.Length : i < 10; i++)
            {
                if (isAlph(a[i]))
                    return true;
            }
            return false;
        }
        #endregion


        #region WindowsApi

        [DllImport("user32.dll")]

        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string sClassName, string sAppName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        #endregion

        #region ClickRegion

        private void Enable_Click(object sender, EventArgs e)
        {
            isEnabled = !isEnabled;

            if (isEnabled && !withoutSelectall && !withSelectall)
            {
                withSelectall = Properties.Settings.Default.withSelect;
                withoutSelectall = Properties.Settings.Default.withoutSelect;
                WithoutSelect.Checked = withoutSelectall;
                WithSelect.Checked = withSelectall;

                if (withSelectall)
                    hotkey.RegisterHotkeyById(2);
                if (withoutSelectall)
                    hotkey.RegisterHotkeyById(1);
            }

            else if (!isEnabled)
            {
                Properties.Settings.Default.withoutSelect = withoutSelectall;
                Properties.Settings.Default.withSelect = withSelectall;
                withSelectall = false;
                withoutSelectall = false;
                WithoutSelect.Checked = false;
                WithSelect.Checked = false;
                hotkey.unRegisterHotKeys();

            }

        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.isEnabled = isEnabled;
            Properties.Settings.Default.withoutSelect = withoutSelectall;
            Properties.Settings.Default.withSelect = withSelectall;
            Properties.Settings.Default.Save();

            if (withSelectall)
                hotkey.unRegisterHotkeyById(2);

            if (withoutSelectall)
                hotkey.unRegisterHotkeyById(1);

            Close();
        }

        private void WithSelect_Click(object sender, EventArgs e)
        {
            withSelectall = !withSelectall;
            if (!withSelectall && !withoutSelectall)
            {
                isEnabled = false;
                Enable.Checked = false;
                hotkey.unRegisterHotKeys();
            }

            else if (!withSelectall)
                hotkey.unRegisterHotkeyById(2);

            else if (withSelectall && !isEnabled)
            {
                isEnabled = true;
                Enable.Checked = true;
                hotkey.RegisterHotkeyById(2);

                if (withoutSelectall)
                    hotkey.RegisterHotkeyById(1);
            }

            else if (withSelectall)
                hotkey.RegisterHotkeyById(2);
        }

        private void WithoutSelect_Click(object sender, EventArgs e)
        {
            withoutSelectall = !withoutSelectall;
            if (!withSelectall && !withoutSelectall)
            {
                isEnabled = false;
                Enable.Checked = false;
                hotkey.unRegisterHotKeys();
            }

            else if (!withoutSelectall)
                hotkey.unRegisterHotkeyById(1);

            else if (withoutSelectall && !isEnabled)
            {
                isEnabled = true;
                Enable.Checked = true;
                hotkey.RegisterHotkeyById(1);

                if (withSelectall)
                    hotkey.RegisterHotkeyById(2);
            }

            else if (withSelectall)
                hotkey.RegisterHotkeyById(1);
        }
        #endregion
    }

}
