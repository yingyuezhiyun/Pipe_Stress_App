using Pipe_Stress_App_V2._5.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers.Interface
{
    public interface IDataHandler
    {

        /// <summary>
        /// 数据处理是否完成
        /// </summary>
        public bool IsComplete { get; set; }


        void AutoMultiplex(bool run);

        /// <summary>
        /// 清空初始值
        /// </summary>
        public void ClearInitialValues();
        /// <summary>
        /// 应变仪 接收数据处理
        /// </summary>
        /// <param name="revDatas"></param>
        public void DeviceRevDataHandler(List<RevDataArray> revDatas);

        /// <summary>
        /// 获取当前通道初始值
        /// </summary>
        /// <returns></returns>
        public List<double> GetCurrentInitialValue();
        void GetMultiplexHoldingTime();

        /// <summary>
        /// 设定初始值
        /// </summary>
        public void SetInitialValues();
        public List<DetData> StainToValidStress(List<DetData> OriginData);

        public string _sourcenamespace { get; set; }
    }
}
