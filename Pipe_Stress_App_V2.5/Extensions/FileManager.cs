using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Extensions
{
    public class FileManager
    {
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">根目录</param>
        /// <returns>文件夹信息</returns>
        public static DirectoryInfo CreateNewFolder(string path)
        {
            string foldName = DateTime.Now.ToString("yyyy年MM月dd日HH时");//yyyyMMddHHmmssffff

            DirectoryInfo newFold = new DirectoryInfo(Path.Combine(path, foldName));
            if (!newFold.Exists)
            {
                newFold.Create();
            }
            return newFold;
        }

        /// <summary>
        /// true表示正在使用,false没有使用
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch
            {
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return inUse;//true表示正在使用,false没有使用
        }

    }
}
