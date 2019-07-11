using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace IL2GPU_Compiler
{
    internal class Variable
    {
        public UInt32 Id = 0;
        public Type Type;
        public spv.StorageClass StorageClass = spv.StorageClass.StorageClassPrivate;
        public string Name = string.Empty;
    }

    public partial class Compiler
    {
        List<Variable> mPublicVariables = new List<Variable>();

        void InitVariables()
        {
            foreach (var variable in mVariables)
            {
                var storageClass = spv.StorageClass.StorageClassPrivate;
                var id = GetNextId();

                PushName(id, variable.Name);

                foreach (var attr in variable.GetCustomAttributes(true))
                {
                    var layout = attr as IL2GPU_API.LayoutAttribute;
                    if (layout != null)
                    {
                        if (layout.Binding != -1)
                        {
                            storageClass = spv.StorageClass.StorageClassUniform;

                            mDecorateInstructions.Add(Pack(4, spv.Op.OpDecorate)); //size,Type
                            mDecorateInstructions.Add(id); //target (Id)
                            mDecorateInstructions.Add((UInt32)spv.Decoration.DecorationDescriptorSet); //Decoration Type (Id)
                            mDecorateInstructions.Add((UInt32)layout.DescriptorSet); //descriptor set index

                            mDecorateInstructions.Add(Pack(4, spv.Op.OpDecorate)); //size,Type
                            mDecorateInstructions.Add(id); //target (Id)
                            mDecorateInstructions.Add((UInt32)spv.Decoration.DecorationBinding); //Decoration Type (Id)
                            mDecorateInstructions.Add((UInt32)layout.Binding); //binding index.
                        }
                        else if (layout.PushConstant)
                        {
                            storageClass = spv.StorageClass.StorageClassPushConstant;
                        }
                        else if (layout.SpecId != -1)
                        {
                            //storageClass = spv.StorageClass.StorageClassPushConstant;
                            throw new NotImplementedException();

                        }
                        else
                        {
                            switch (layout.Direction)
                            {
                                case IL2GPU_API.Direction.Private:
                                    {
                                        storageClass = spv.StorageClass.StorageClassPrivate;
                                    }
                                    break;
                                case IL2GPU_API.Direction.Input:
                                    {
                                        storageClass = spv.StorageClass.StorageClassInput;

                                        mInputRegisters.Add(id);

                                        mDecorateInstructions.Add(Pack(3 + 1, spv.Op.OpDecorate)); //size,Type
                                        mDecorateInstructions.Add(id); //target (Id)
                                        mDecorateInstructions.Add((UInt32)spv.Decoration.DecorationLocation); //Decoration Type (Id)
                                        mDecorateInstructions.Add((UInt32)layout.Location); //Location offset
                                    }
                                    break;
                                case IL2GPU_API.Direction.Output:
                                    {
                                        storageClass = spv.StorageClass.StorageClassOutput;

                                        mOutputRegisters.Add(id);

                                        if (layout.BuiltIn == IL2GPU_API.BuiltIn.None)
                                        {
                                            mDecorateInstructions.Add(Pack(3 + 1, spv.Op.OpDecorate)); //size,Type
                                            mDecorateInstructions.Add(id); //target (Id)
                                            mDecorateInstructions.Add((UInt32)spv.Decoration.DecorationLocation); //Decoration Type (Id)
                                            mDecorateInstructions.Add((UInt32)layout.Location); //Location offset
                                        }
                                        else
                                        {
                                            mDecorateInstructions.Add(Pack(3 + 1, spv.Op.OpDecorate)); //size,Type
                                            mDecorateInstructions.Add(id); //target (Id)
                                            mDecorateInstructions.Add((UInt32)spv.Decoration.DecorationBuiltIn); //Decoration Type (Id)
                                            mDecorateInstructions.Add((UInt32)spv.BuiltIn.BuiltInPosition); //Location offset
                                        }
                                    }
                                    break;
                                default:
                                    Trace.TraceWarning("Unknown direction, using private storage class.");
                                    break;
                            }
                        }
                    }
                }

                var variableType = variable.GetType().MakePointerType();
                var resultTypeId = GetTypeId(variableType, storageClass);

                mIdTypePairs[id] = mIdTypePairs[resultTypeId];

                mTypeInstructions.Add(Pack(4, spv.Op.OpVariable)); //size,Type
                mTypeInstructions.Add(resultTypeId); //ResultType (Id) Must be OpTypePointer with the pointer's type being what you care about.
                mTypeInstructions.Add(id); //Result (Id)
                mTypeInstructions.Add((UInt32)storageClass); //Storage Class

                mPublicVariables.Add(new Variable() { Id = id, Type = variableType, StorageClass = storageClass, Name = variable.Name });
            }
        }
    }
}
