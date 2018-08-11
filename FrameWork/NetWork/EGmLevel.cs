using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public enum EGmLevel
    {
        Anyone = 1,
        Staff = 2,
        GM = 3,
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
