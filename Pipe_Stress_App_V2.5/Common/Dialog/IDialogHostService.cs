﻿using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Dialog
{
    public interface IDialogHostService:IDialogService
    {
        Task<IDialogResult> ShowDialog(string name, IDialogParameters parameters = null, string dialogHostName = "Root");
    }
}
