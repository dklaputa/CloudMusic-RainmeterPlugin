using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PluginCloudMusic
{
    public static class CloudMusicUtils
    {
        private const int WM_GETTEXT = 0x00D;
        private const int WM_GETTEXTLENGTH = 0x00E;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int INPUT_KEYBOARD = 1;
        
        private const short VK_CONTROL = 0x11;
        private const short VK_MENU = 0x12;
        private const short VK_P = 0x50;
        private const short VK_LEFT = 0x25;
        private const short VK_RIGHT = 0x27;

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public int X;
            public int Y;
            public int Data;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public short VirtualKey;
            public short ScanCode;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Input
        {
            public int Type;
            public InputUnion ui;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MouseInput Mouse;
            [FieldOffset(0)] public KeyboardInput Keyboard;
            [FieldOffset(0)] public HardwareInput Hardware;
        }

        [DllImport("user32", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpszClass, string lpszWindow);

        [DllImport("user32", SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, StringBuilder lParam);

        [DllImport("user32", SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32", SetLastError = true)]
        private static extern void SendInput(int nInputs, Input[] pInputs, int cbsize);

        public static string GetNowMusic()
        {
            var cloudMusicWindow = FindWindow("OrpheusBrowserHost", null);
            if (cloudMusicWindow == IntPtr.Zero)
            {
                return null;
            }

            var length = SendMessage(cloudMusicWindow, WM_GETTEXTLENGTH, 0, 0);
            if (length == 0)
            {
                return null;
            }

            var text = new StringBuilder(length + 1);
            SendMessage(cloudMusicWindow, WM_GETTEXT, text.Capacity, text);
            return text.ToString();
        }

        private static void HotKey(short vk)
        {
            Input[] inputs = new Input[6];
            inputs[0] = new Input {Type = INPUT_KEYBOARD};
            inputs[0].ui.Keyboard.VirtualKey = VK_CONTROL;
            inputs[1] = new Input {Type = INPUT_KEYBOARD};
            inputs[1].ui.Keyboard.VirtualKey = VK_MENU;
            inputs[2] = new Input {Type = INPUT_KEYBOARD};
            inputs[2].ui.Keyboard.VirtualKey = vk;

            inputs[3] = new Input {Type = INPUT_KEYBOARD};
            inputs[3].ui.Keyboard.VirtualKey = vk;
            inputs[3].ui.Keyboard.Flags = KEYEVENTF_KEYUP;
            inputs[4] = new Input {Type = INPUT_KEYBOARD};
            inputs[4].ui.Keyboard.VirtualKey = VK_MENU;
            inputs[4].ui.Keyboard.Flags = KEYEVENTF_KEYUP;
            inputs[5] = new Input {Type = INPUT_KEYBOARD};
            inputs[5].ui.Keyboard.VirtualKey = VK_CONTROL;
            inputs[5].ui.Keyboard.Flags = KEYEVENTF_KEYUP;

            SendInput(inputs.Length, inputs, Marshal.SizeOf(inputs[0]));
        }

        public static void PlayOrPause()
        {
            HotKey(VK_P);
        }
        
        public static void PlayNext()
        {
            HotKey(VK_RIGHT);
        }
        
        public static void PlayPrevious()
        {
            HotKey(VK_LEFT);
        }
    }
}