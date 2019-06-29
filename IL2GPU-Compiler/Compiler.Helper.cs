using System;
using System.Collections.Generic;
using System.Text;

namespace IL2GPU_Compiler
{
    public partial class Compiler
    {
        UInt32 mBaseId = 1;
        UInt32 mNextId = 1;

        UInt32 GetNextId()
        {
            return mNextId++;
        }

    }
}
