using System;
using System.Collections.Generic;
using System.Text;

namespace IL2GPU_Compiler
{
    public partial class Compiler
    {
        List<Variable> mFunctionVariables = new List<Variable>();
        Stack<Variable> mVariableStack = new Stack<Variable>();
        void InitMethods()
        {
            foreach (var method in mMethods)
            {

            }

            throw new NotImplementedException();
        }
    }
}
