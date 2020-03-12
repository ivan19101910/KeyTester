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
//using System.Windows.Input;
using Hooks;
//доробити
namespace KeyTester
{
   
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern long SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point point);
        //[DllImport("user32.dll")]
        //public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("User32.dll")]
        static extern void mouse_event(MouseFlags dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        [Flags]
        enum MouseFlags
        {
            Move = 0x0001, LeftDown = 0x0002, LeftUp = 0x0004, RightDown = 0x0008,
            RightUp = 0x0010, Absolute = 0x8000
        };
        public Form1()
        {
            InitializeComponent();

            this.FormClosed += new FormClosedEventHandler(frmMain_FormClosed);
            //MouseHook.MouseDown += new MouseEventHandler(MouseHook_MouseDown);
            //MouseHook.MouseDown -= new MouseEventHandler(MouseHook_MouseDown);
            //MouseHook.MouseMove += new MouseEventHandler(MouseHook_MouseMove);
            //MouseHook.MouseUp += new MouseEventHandler(MouseHook_MouseUp);
            MouseHook.LocalHook = false;
            listBox1.Items.Clear();
            MouseHook.InstallHook();
            label1.Text = string.Format("Installed:{0}\r\nModule:{1}\r\nLocal{2}",
                MouseHook.IsHookInstalled, MouseHook.ModuleHandle, MouseHook.LocalHook);
        }


       

        const int STOP_HOTKEY_ID = 1;
        //Point []mouseMovement;
        MousePoint[] mouseMovement;
        Object[] actions;
        bool stopFlag = false;
        bool leftMouseDown = false;
        bool leftMouseUp = false;
        bool rightMouseDown = false;
        bool rightMouseUp = false;
        const double xScaling = (double)65535 / 1920;
        const double yScaling = (double)65535 / 1080;
        int i = 0;


        private void button1_Click(object sender, EventArgs e)
        {
            
            new Thread(() => recordMouseMovement(25)).Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, STOP_HOTKEY_ID, 0,(int)Keys.Q);
        }
        int x = (int)(500 * xScaling);
        int y = (int)(142 * yScaling);


        protected override void WndProc(ref Message m)//method for react on key pressing
        {

            if (m.Msg == 0x0312/*Constants.WM_HOTKEY_MSG_ID*/)
            {
                switch (GetKey(m.LParam))
                {
                    case Keys.Q:
                        // the hotkey key is A
                        MessageBox.Show("Orest pythonist");
                        break;

                }
            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == STOP_HOTKEY_ID)//react
            {
                //Point p = Cursor.Position;
                //MessageBox.Show($"X:{p.X} Y:{p.Y}");
                stopFlag = true;
                //mouse_event(MouseFlags.Absolute | MouseFlags.Move, x, y, 0, UIntPtr.Zero);
                //mouse_event(MouseFlags.Absolute | MouseFlags.LeftDown, x, y, 0, UIntPtr.Zero);
                //mouse_event(MouseFlags.Absolute | MouseFlags.LeftUp, x, y, 0, UIntPtr.Zero);
            }

           
                
            base.WndProc(ref m);


                //base.WndProc(ref m);
        }

        private Keys GetKey(IntPtr LParam)

        {

            return (Keys) ((LParam.ToInt32()) >> 16); // not all of the parenthesis are needed, I just found it easier to see what's happening

        }




    public void recordMouseMovement(int delay)
        {
            Thread.Sleep(100);
            MouseHook.MouseDown += new MouseEventHandler(MouseHook_MouseDown);
            MouseHook.MouseUp += new MouseEventHandler(MouseHook_MouseUp);
            //Point[] mouseMovement;
            for (; ;++i)
            {
                Array.Resize(ref mouseMovement, i+1);
                Array.Resize(ref actions, i + 1);
                //mouseMovement[i] = Cursor.Position;
                actions[i] = new MousePoint(Cursor.Position.X, Cursor.Position.Y);//try use object
                //actions[i].x = Cursor.Position.X;
                
                //actions[i].y = Cursor.Position.Y;
                mouseMovement[i] = new MousePoint();
                mouseMovement[i].x = Cursor.Position.X;
                mouseMovement[i].y = Cursor.Position.Y;
                Thread.Sleep(delay);
                if (stopFlag == true)
                {
                    stopFlag = false;
                    break;
                }
            }
            i = 0;
            MouseHook.MouseDown -= new MouseEventHandler(MouseHook_MouseDown);
            MouseHook.MouseUp -= new MouseEventHandler(MouseHook_MouseUp);

        }
        bool active = false;
        public void playbackMouseMovement()
        {

            Thread.Sleep(100);//playback delay
            for (int j = 0; j < mouseMovement.Length; ++j)
            {

                //mouseMovement[i] = Cursor.Position;

                //Thread.Sleep(delay);
                
                Thread.Sleep(25);
                SetCursorPos(mouseMovement[j].x, mouseMovement[j].y);
                leftMouseDown = mouseMovement[j].leftDown;
                leftMouseUp = mouseMovement[j].leftUp;

                int f = (actions[0] as MousePoint).x;//try use object

                if (leftMouseUp && active)
                {
                    mouse_event(MouseFlags.Absolute | MouseFlags.LeftUp, x, y, 0, UIntPtr.Zero);
                    active = false;
                }
                if (leftMouseDown && !active)
                {
                    mouse_event(MouseFlags.Absolute | MouseFlags.LeftDown, x, y, 0, UIntPtr.Zero);
                    active = true;
                }

                /*if(mouseMovement[j].leftDown == true)
                {
                    mouse_event(MouseFlags.Absolute | MouseFlags.LeftDown, x, y, 0, UIntPtr.Zero);
                }
                else if (mouseMovement[j].leftUp == true)
                {
                    mouse_event(MouseFlags.Absolute | MouseFlags.LeftUp, x, y, 0, UIntPtr.Zero);
                }
                else if (mouseMovement[j].rightDown == true)
                {

                }
                else if (mouseMovement[j].rightUp == true)
                {

                }*/

                if (stopFlag == true)
                {
                    stopFlag = false;
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            new Thread(() => playbackMouseMovement()).Start();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }





        //Hook function(for capture mouse clicks)
        void MouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            listBox1.Items.Add(e.Location);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        void MouseHook_MouseUp(object sender, MouseEventArgs e)
        {
            listBox1.Items.Add(e.Button);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            
            mouseMovement[i].leftUp = true;
        }

        void MouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            mouseMovement[i].leftDown = true;

            listBox1.Items.Add(e.Button);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            textBox1.Text = e.Button.ToString();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            MouseHook.UnInstallHook();
        }


    }

    public class MousePoint
    {
        public MousePoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public MousePoint() { }

        public int x;
        public int y;
        public bool leftDown = false;
        public bool leftUp = false;
        public bool rightDown = false;
        public bool rightUp = false;
    }

    public static class Constants

    {
        //modifiers
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;
        //windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;

    }

}
