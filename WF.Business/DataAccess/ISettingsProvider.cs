using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Business.Model;

namespace WF.Business.DataAccess
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
    }
}
