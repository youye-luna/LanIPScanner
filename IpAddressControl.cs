using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DhcpScanner
{
    /// <summary>
    /// Windows 原生 IP 地址输入控件（SysIPAddress32），与系统网络属性对话框样式一致。
    /// 原生支持：点分四段、获得焦点全选、输满3位或按点号自动跳到下一段。
    /// </summary>
    public class IpAddressControl : Control
    {
        private const string ClassName = "SysIPAddress32";
        private const int ICC_INTERNET_CLASSES = 0x00000800;

        private const uint WS_CHILD = 0x40000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_TABSTOP = 0x00010000;

        private const int WM_SETFOCUS = 0x0007;
        private const int WM_SETFONT = 0x0030;
        private const int WM_USER = 0x0400;
        private const int IPM_SETADDRESS = WM_USER + 101;
        private const int IPM_GETADDRESS = WM_USER + 102;
        private const int IPM_SETRANGE = WM_USER + 103;
        private const int IPM_SETFOCUS_MSG = WM_USER + 105;

        [StructLayout(LayoutKind.Sequential)]
        private struct INITCOMMONCONTROLSEX
        {
            public int dwSize;
            public int dwICC;
        }

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern bool InitCommonControlsEx(ref INITCOMMONCONTROLSEX icce);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private IntPtr _hwndIp;
        private IntPtr _fontHandle;
        private string? _pendingAddress;

        static IpAddressControl()
        {
            var icce = new INITCOMMONCONTROLSEX
            {
                dwSize = Marshal.SizeOf<INITCOMMONCONTROLSEX>(),
                dwICC = ICC_INTERNET_CLASSES
            };
            InitCommonControlsEx(ref icce);
        }

        public IpAddressControl()
        {
            TabStop = true;
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_hwndIp == IntPtr.Zero)
            {
                _hwndIp = CreateWindowEx(0, ClassName, string.Empty, WS_CHILD | WS_VISIBLE | WS_TABSTOP,
                    0, 0, Width, Height, Handle, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);

                // 限制每段取值范围 0-255
                for (int field = 0; field < 4; field++)
                    SendMessage(_hwndIp, IPM_SETRANGE, (IntPtr)field, MakeRange(0, 255));

                ApplyNativeFont();

                if (_pendingAddress != null)
                {
                    SetAddress(_pendingAddress);
                    _pendingAddress = null;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_hwndIp != IntPtr.Zero)
                MoveWindow(_hwndIp, 0, 0, Width, Height, true);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ApplyNativeFont();
        }

        private void ApplyNativeFont()
        {
            if (_hwndIp == IntPtr.Zero)
                return;

            IntPtr oldFont = _fontHandle;
            _fontHandle = Font.ToHfont();
            SendMessage(_hwndIp, WM_SETFONT, _fontHandle, (IntPtr)1);
            if (oldFont != IntPtr.Zero)
                DeleteObject(oldFont);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // 外层控件通过 Tab 等方式获得焦点后，把焦点转交给内部原生控件
            if (m.Msg == WM_SETFOCUS && _hwndIp != IntPtr.Zero)
                SendMessage(_hwndIp, IPM_SETFOCUS_MSG, IntPtr.Zero, IntPtr.Zero);
        }

        private static IntPtr MakeRange(int low, int high) => (IntPtr)(((high & 0xFF) << 8) | (low & 0xFF));

        /// <summary>
        /// 获取当前输入的IP文本（如 "192.168.1.1"），空段按0处理
        /// </summary>
        public string GetAddressText()
        {
            if (_hwndIp == IntPtr.Zero)
                return _pendingAddress ?? "0.0.0.0";

            IntPtr lParam = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.WriteInt32(lParam, 0);
                int fieldCount = SendMessage(_hwndIp, IPM_GETADDRESS, IntPtr.Zero, lParam).ToInt32();
                if (fieldCount < 4)
                    return string.Empty;
                uint addr = (uint)Marshal.ReadInt32(lParam);
                return $"{(addr >> 24) & 0xFF}.{(addr >> 16) & 0xFF}.{(addr >> 8) & 0xFF}.{addr & 0xFF}";
            }
            finally
            {
                Marshal.FreeHGlobal(lParam);
            }
        }

        /// <summary>
        /// 设置IP地址
        /// </summary>
        public void SetAddress(string ip)
        {
            if (!IPAddress.TryParse(ip.Trim(), out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return;

            var parts = address.ToString().Split('.');
            byte a = byte.Parse(parts[0]);
            byte b = byte.Parse(parts[1]);
            byte c = byte.Parse(parts[2]);
            byte d = byte.Parse(parts[3]);
            string normalized = address.ToString();

            if (_hwndIp == IntPtr.Zero)
            {
                _pendingAddress = normalized;
                return;
            }

            uint addr = ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | d;
            SendMessage(_hwndIp, IPM_SETADDRESS, IntPtr.Zero, (IntPtr)addr);
        }

        protected override void Dispose(bool disposing)
        {
            if (_fontHandle != IntPtr.Zero)
            {
                DeleteObject(_fontHandle);
                _fontHandle = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }
    }
}
