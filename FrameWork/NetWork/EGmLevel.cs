using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public enum EGmLevel
    {
        Staff = 1,
        GM = 2,
        TrustedGM = 4,
        DatabaseDev = 8,
        SourceDev = 16,

        AnyGM = 6,
        TrustedStaff = 28,
        EmpoweredStaff = 30,
        AllStaff = 31,
        Management = 32
    }
}
