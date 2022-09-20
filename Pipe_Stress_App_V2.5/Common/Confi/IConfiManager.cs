using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Confi
{
    public interface IConfiManager
    {
        void NavigationConfi();
        void SkinInit();

        void FileSaveConfiInit();
        void InputConfiInit();
        void MultiplexConfiInit();

    }
}
