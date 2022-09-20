using Pipe_Stress_App_V2._5.Extensions;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pipe_Stress_App_V2._5.Views
{
    /// <summary>
    /// SerialPortView.xaml 的交互逻辑
    /// </summary>
    public partial class SerialPortView : UserControl
    {
        public SerialPortView(IEventAggregator aggregator)
        {
            InitializeComponent();
            aggregator.ResgiterMessage(arg =>
            {
                Snackbar.MessageQueue.Enqueue(arg.Message);
            }, "SerialPort");
        }
    }
}
