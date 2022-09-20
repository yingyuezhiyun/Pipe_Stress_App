using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models.Interfaces
{
    public interface IFileSaveInfo
    {
        /// <summary>
        /// 存储周期（一个文件存多长时间的数据）（分钟）
        /// </summary>
        public double Period { get; set; }
        /// <summary>
        /// 文件存储间隔时间（分钟）
        /// </summary>
        public double Interval { get; set; }

        /// <summary>
        /// 新建文件夹 间隔时间 （小时）
        /// </summary>
        public double NewFolderInterval { get; set; }

        /// <summary>
        /// 存储的根目录
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// 自动存储
        /// </summary>
        public bool AutoSave { get; set; }

        public DirectoryInfo FolderDirectory { get; set; }
    }
}
