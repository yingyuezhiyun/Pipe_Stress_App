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
        /// 初期フォルダの指定
        /// SelectedPath がセットされていない(or設定がおかしい)等の場合に利用される
        /// </summary>
        public string InitialFolder { get; set; }
        /// <summary>
        /// set = 前回設定していたパスの指定
        /// get = ダイアログで選択したパス(Cancel終了の場合は設定されない)
        /// </summary>
        public string SelectedPath { get; set; }
        /// <summary>
        /// ダイアログのタイトル
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// プレースフォルダの追加
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
        ///	ダイアログの表示
        /// </summary>
        /// <returns>
        /// true:どこかを選択
        /// false:キャンセルか何らかの理由でダイアログが開けなかった
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
            //	オーナーウィンドウを問題のないやつに切り替え
            ownerWindow = GetSafeOwnerWindow(ownerWindow);

            var dlg = new CoclassFileOpenDialog() as IFileOpenDialog;
            try
            {
                dlg.SetOptions(FOS.PICKFOLDERS | FOS.FORCEFILESYSTEM);
                bool setFolder = false;
                //	既存フォルダを指定する
                if (!string.IsNullOrWhiteSpace(SelectedPath))
                {
                    IShellItem item;
                    if (NativeMethods.SUCCEEDED(NativeMethods.SHCreateItemFromParsingName(SelectedPath, IntPtr.Zero, typeof(IShellItem).GUID, out item)))
                    {
                        dlg.SetFolder(item);
                        setFolder = true;
                    }
                }
                //	初期フォルダを指定できるようにしておく
                if (!setFolder && !string.IsNullOrWhiteSpace(InitialFolder))
                {
                    IShellItem item;
                    if (NativeMethods.SUCCEEDED(NativeMethods.SHCreateItemFromParsingName(InitialFolder, IntPtr.Zero, typeof(IShellItem).GUID, out item)))
                    {
                        dlg.SetFolder(item);
                    }
                }
                //	タイトルを設定
                if (!string.IsNullOrWhiteSpace(Title))
                {
                    dlg.SetTitle(Title);
                }
                //	プレースメントフォルダを追加
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
                MessageBox.Show(exp.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                Marshal.FinalReleaseComObject(dlg);
            }
            return false;
        }
        #region implements native methods
        /// <summary>
        /// Win32レベルのオーナーウィンドウの設定(確実にモーダル処理を行うための措置。
        /// デバッグ実行していて nullptr の場合おかしくなることがあるので要注意
        /// </summary>
        /// <param name="hwndOwner"></param>
        /// <returns></returns>
        private static IntPtr GetSafeOwnerWindow(IntPtr hwndOwner)
        {
            //	無効なウィンドウを参照している場合の排除
            if (hwndOwner != IntPtr.Zero && !NativeMethods.IsWindow(hwndOwner))
            {
                hwndOwner = IntPtr.Zero;
            }
            //	オーナーウィンドウの基本を探す
            if (hwndOwner == IntPtr.Zero)
            {
                hwndOwner = NativeMethods.GetForegroundWindow();
            }
            //	トップレベルウィンドウを探す
            IntPtr hwndParent = hwndOwner;
            while (hwndParent != IntPtr.Zero)
            {
                hwndOwner = hwndParent;
                hwndParent = NativeMethods.GetParent(hwndOwner);
            }
            //	トップレベルウィンドウに所属する現在アクティブなポップアップ(自分も含む)を取得
            if (hwndOwner != IntPtr.Zero)
            {
                hwndOwner = NativeMethods.GetLastActivePopup(hwndOwner);
            }
            return hwndOwner;
        }
        /// <summary>
        /// APIラッパー
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
        /// 簡易実装版ファイルダイアログインターフェース
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
