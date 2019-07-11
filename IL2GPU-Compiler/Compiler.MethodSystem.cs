using System;
using System.Collections.Generic;
using System.Text;

namespace IL2GPU_Compiler
{
    internal class Function
    {
        public UInt32 Id = 0;
        public string Name = string.Empty;
    }
    public partial class Compiler
    {
        List<List<Variable>> mFunctionVariables = new List<List<Variable>>();
        Stack<Variable> mVariableStack = new Stack<Variable>();
        Dictionary<string, Function> mFunctions = new Dictionary<string, Function>();
        void InitMethods()
        {
            foreach (var method in mMethods)
            {
                var function = new Function { Name = method.Name, Id = GetNextId() };
                var methodBody = method.GetMethodBody();

                //Each function has it's own variables.
                var variables = new List<Variable>();
                foreach (var variable in methodBody.LocalVariables)
                {
                    var id = GetNextId();

                    var variableType = variable.GetType().MakePointerType();
                    var variableTypeId = GetTypeId(variableType, spv.StorageClass.StorageClassFunction);

                    mIdTypePairs[id] = mIdTypePairs[variableTypeId];

                    Push(spv.Op.OpVariable, variableTypeId, id, (UInt32)spv.StorageClass.StorageClassFunction);

                    variables.Add(new Variable() { Id = id, Type = variableType, StorageClass = spv.StorageClass.StorageClassFunction, Name = variable.LocalIndex.ToString() });
                }
                mFunctionVariables.Add(variables);

                //Function Start
                var returnType = method.ReturnType;
                var returnTypeId = GetTypeId(returnType, spv.StorageClass.StorageClassFunction);
                
                //TODO: find or build function type.

                if (function.Name == "main")
                {
                    mEntryPointId = function.Id;
                    //mEntryPointTypeId = 
                }

                //Push(spv::OpFunction, GetSpirVTypeId(spv::OpTypeVoid), mEntryPointId, spv::FunctionControlMaskNone, mEntryPointTypeId);
                //Push(spv::OpLabel, GetNextId());

                mFunctions[function.Name] = function;
            }

            throw new NotImplementedException();
        }
    }
}
