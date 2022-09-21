using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pipe_Stress_App_V2._5.Extensions
{
    public class PickupFolderDialog
    {
        /// <summary>
        /// 指定初始文件夹
        /// SelectedPath 未设置（or设定异常）等情况下使用
        /// </summary>
        public string InitialFolder { get; set; }
        /// <summary>
        /// set = 指定上次设置的路径
        /// get = 对话框中选择的路径（如果取消激活，则不设置）
        /// </summary>
        public string SelectedPath { get; set; }
        /// <summary>
        /// 对话框标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 添加文件夹路径
        /// </summary>
        /// <param name="value"></param>
        public void AddPlace(string value)
        {
            if (m_placeList == null)
            {
                m_placeList = new List<string>();
            }
            m_placeList.Add(value);
        }
        private List<string> m_placeList;

        /// <summary>
        ///	显示对话框
        /// </summary>
        /// <returns>
        /// true:选择某个位置
        /// false:取消或由于某种原因无法打开对话框
        /// </returns>
        public bool ShowDialog()
        {
            return ShowDialog(Application.Current.MainWindow);
        }
        public bool ShowDialog(Window ownerWindow)
        {
            var hwndSrc = System.Windows.Interop.HwndSource.FromVisual(ownerWindow) as System.Windows.Interop.HwndSource;
            return ShowDialog((hwndSrc != null) ? hwndSrc.Handle : IntPtr.Zero);
        }
        public bool ShowDialog(IntPtr ownerWindow)
        {
            //	将窗口切换为无
            ownerWindow = GetSafeOwnerWindow(ownerWindow);

            var dlg = new CoclassFileOpenDialog() as IFileOpenDialog;
            try
            {
                dlg.SetOptions(FOS.PICKFOLDERS | FOS.FORCEFILESYSTEM);
                bool setFolder = false;
                //	指定现有文件夹
                if (!string.IsNullOrWhiteSpace(SelectedPath))
                {
                    IShellItem item;
                    if (NativeMethods.SUCCEEDED(NativeMethods.SHCreateItemFromParsingName(SelectedPath, IntPtr.Zero, typeof(IShellItem).GUID, out item)))
                    {
                        dlg.SetFolder(item);
                        setFolder = true;
                    }
                }
                //	允许指定初始文件夹
                if (!setFolder && !string.IsNullOrWhiteSpace(InitialFolder))
                {
                    IShellItem item;
                    if (NativeMethods.SUCCEEDED(NativeMethods.SHCreateItemFromParsingName(InitialFolder, IntPtr.Zero, typeof(IShellItem).GUID, out item)))
                    {
                        dlg.SetFolder(item);
                    }
                }
                //	设置标题
                if (!string.IsNullOrWhiteSpace(Title))
                {
                    dlg.SetTitle(Title);
                }
                //	添加选定文件夹
                if (m_placeList != null)
                {
                    foreach (var folder in m_placeList)
                    {
                        IShellItem item;
                        if (NativeMethods.SUCCEEDED(NativeMethods.SHCreateItemFromParsingName(folder, IntPtr.Zero, typeof(IShellItem).GUID, out item)))
                        {
                            dlg.AddPlace(item, FDAP.TOP);
                        }
                    }
                }
                var hRes = dlg.Show(ownerWindow);
                if (NativeMethods.SUCCEEDED(hRes))
                {
                    IShellItem item;
                    dlg.GetResult(out item);
                    string resultPath;
                    item.GetDisplayName(SIGDN.FILESYSPATH, out resultPath);
                    SelectedPath = resultPath;
                    return true;
                }
            }
            catch (COMException exp)
            {
                MessageBox.Show(exp.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                Marshal.FinalReleaseComObject(dlg);
            }
            return false;
        }
        #region implements native methods
        /// <summary>
        /// 设置Win32窗口(确保进行模态处理的措施。)
        /// 注意，在执行调试时，nullptr可能会出现异常
        /// </summary>
        /// <param name="hwndOwner"></param>
        /// <returns></returns>
        private static IntPtr GetSafeOwnerWindow(IntPtr hwndOwner)
        {
            //	排除无效窗口
            if (hwndOwner != IntPtr.Zero && !NativeMethods.IsWindow(hwndOwner))
            {
                hwndOwner = IntPtr.Zero;
            }
            //	查找窗口的基本信息
            if (hwndOwner == IntPtr.Zero)
            {
                hwndOwner = NativeMethods.GetForegroundWindow();
            }
            //	查找顶层窗口
            IntPtr hwndParent = hwndOwner;
            while (hwndParent != IntPtr.Zero)
            {
                hwndOwner = hwndParent;
                hwndParent = NativeMethods.GetParent(hwndOwner);
            }
            //	获取属于顶层窗口的当前活动弹出窗口（包括其自身）
            if (hwndOwner != IntPtr.Zero)
            {
                hwndOwner = NativeMethods.GetLastActivePopup(hwndOwner);
            }
            return hwndOwner;
        }
        /// <summary>
        /// API包装
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool IsWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            internal static extern IntPtr GetForegroundWindow();
            [DllImport("user32.dll")]
            internal static extern IntPtr GetParent(IntPtr hwnd);
            [DllImport("user32.dll")]
            internal static extern IntPtr GetLastActivePopup(IntPtr hwnd);
            [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
            internal static extern int SHCreateItemFromParsingName(
                [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
                [In] IntPtr pbc,
                [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
                [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItem ppv);
            internal static bool SUCCEEDED(int result) => result >= 0;
            internal static bool FAILED(int result) => result < 0;
        }
        /// <summary>
        /// 简易安装版文件对话框接口
        /// </summary>
        [ComImport]
        [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class CoclassFileOpenDialog { }
        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig]
            int Show([In] IntPtr hwndParent);
            void SetFileTypes();     // not fully defined
            void SetFileTypeIndex();     // not fully defined
            void GetFileTypeIndex();     // not fully defined
            void Advise(); // not fully defined
            void Unadvise();
            void SetOptions([In] FOS fos);
            void GetOptions(); // not fully defined
            void SetDefaultFolder(); // not fully defined
            void SetFolder(IShellItem psi);
            void GetFolder(); // not fully defined
            void GetCurrentSelection(); // not fully defined
            void SetFileName();  // not fully defined
            void GetFileName();  // not fully defined
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel(); // not fully defined
            void SetFileNameLabel(); // not fully defined
            void GetResult(out IShellItem ppsi);
            void AddPlace([In] IShellItem item, [In] FDAP fdap);
            void SetDefaultExtension(); // not fully defined
            void Close(); // not fully defined
            void SetClientGuid();  // not fully defined
            void ClearClientData();
            void SetFilter(); // not fully defined
            void GetResults(); // not fully defined
            void GetSelectedItems(); // not fully defined
        }
        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(); // not fully defined
            void GetParent(); // not fully defined
            void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes();  // not fully defined
            void Compare();  // not fully defined
        }
        private enum SIGDN : uint // not fully defined
        {
            FILESYSPATH = 0x80058000,
        }
        [Flags]
        private enum FOS // not fully defined
        {
            FORCEFILESYSTEM = 0x40,
            PICKFOLDERS = 0x20,
        }
        private enum FDAP
        {
            BOTTOM = 0,
            TOP = 1
        }
        #endregion
    }
}
