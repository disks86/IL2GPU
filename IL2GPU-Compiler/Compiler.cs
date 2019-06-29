using System;
using System.Reflection;

namespace IL2GPU_Compiler
{
    public partial class Compiler
    {
        Type mType;
        FieldInfo[] mVariables;
        MethodInfo[] mMethods;

        Compiler(Type type)
        {
            mType = type;
            mVariables = mType.GetFields();
            mMethods = mType.GetMethods();
        }

        public byte[] Compile()
        {
            throw new NotImplementedException();
        }
       
        public static byte[] Compile(Type type)
        {
            return new Compiler(type).Compile();
        }

    }
}
