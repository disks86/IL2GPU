using IL2GPU_API;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IL2GPU_Compiler
{
    public partial class Compiler
    {
        Type mType;
        FieldInfo[] mVariables;
        MethodInfo[] mMethods;

        UInt32 mGlslExtensionId = 0;

        List<UInt32> mInstructions = new List<UInt32>(); //used to store the combined instructions for creating a module.

        List<UInt32> mCapabilityInstructions = new List<UInt32>();
        List<UInt32> mExtensionInstructions = new List<UInt32>();
        List<UInt32> mImportExtendedInstructions = new List<UInt32>();
        List<UInt32> mMemoryModelInstructions = new List<UInt32>();
        List<UInt32> mEntryPointInstructions = new List<UInt32>();
        List<UInt32> mExecutionModeInstructions = new List<UInt32>();
        List<UInt32> mStringInstructions = new List<UInt32>();
        List<UInt32> mSourceExtensionInstructions = new List<UInt32>();
        List<UInt32> mSourceInstructions = new List<UInt32>();
        List<UInt32> mSourceContinuedInstructions = new List<UInt32>();
        List<UInt32> mNameInstructions = new List<UInt32>();
        List<UInt32> mMemberNameInstructions = new List<UInt32>();
        List<UInt32> mDecorateInstructions = new List<UInt32>();
        List<UInt32> mMemberDecorateInstructions = new List<UInt32>();
        List<UInt32> mGroupDecorateInstructions = new List<UInt32>();
        List<UInt32> mGroupMemberDecorateInstructions = new List<UInt32>();
        List<UInt32> mDecorationGroupInstructions = new List<UInt32>();
        List<UInt32> mTypeInstructions = new List<UInt32>();
        List<UInt32> mFunctionDeclarationInstructions = new List<UInt32>();
        List<UInt32> mFunctionDefinitionInstructions = new List<UInt32>();

        List<UInt32> mInputRegisters = new List<UInt32>();
        List<UInt32> mOutputRegisters = new List<UInt32>();

        UInt32 mEntryPointId;
        string mEntryPointName;

        public Compiler(Type type)
        {
            mType = type;
            mVariables = mType.GetFields();
            mMethods = mType.GetMethods();
        }

        public UInt32[] Compile()
        {
            InitCapability();
            InitExtensions();
            InitMemoryModel();
            InitSourceLanguage();
            InitVariables();
            InitMethods();
            InitEntryPoint();
            InitHeader();

            //Combine opcodes
            mInstructions.AddRange(mCapabilityInstructions);
            mInstructions.AddRange(mExtensionInstructions);
            mInstructions.AddRange(mImportExtendedInstructions);
            mInstructions.AddRange(mMemoryModelInstructions);
            mInstructions.AddRange(mEntryPointInstructions);
            mInstructions.AddRange(mExecutionModeInstructions);

            mInstructions.AddRange(mStringInstructions);
            mInstructions.AddRange(mSourceExtensionInstructions);
            mInstructions.AddRange(mSourceInstructions);
            mInstructions.AddRange(mSourceContinuedInstructions);
            mInstructions.AddRange(mNameInstructions);
            mInstructions.AddRange(mMemberNameInstructions);

            mInstructions.AddRange(mDecorateInstructions);
            mInstructions.AddRange(mMemberDecorateInstructions);
            mInstructions.AddRange(mGroupDecorateInstructions);
            mInstructions.AddRange(mGroupMemberDecorateInstructions);
            mInstructions.AddRange(mDecorationGroupInstructions);

            mInstructions.AddRange(mTypeInstructions);
            mInstructions.AddRange(mFunctionDeclarationInstructions);
            mInstructions.AddRange(mFunctionDefinitionInstructions);

            mCapabilityInstructions.Clear();
            mExtensionInstructions.Clear();
            mImportExtendedInstructions.Clear();
            mMemoryModelInstructions.Clear();
            mEntryPointInstructions.Clear();
            mExecutionModeInstructions.Clear();

            mStringInstructions.Clear();
            mSourceExtensionInstructions.Clear();
            mSourceInstructions.Clear();
            mSourceContinuedInstructions.Clear();
            mNameInstructions.Clear();
            mMemberNameInstructions.Clear();

            mDecorateInstructions.Clear();
            mMemberDecorateInstructions.Clear();
            mGroupDecorateInstructions.Clear();
            mGroupMemberDecorateInstructions.Clear();
            mDecorationGroupInstructions.Clear();

            mTypeInstructions.Clear();
            mFunctionDeclarationInstructions.Clear();
            mFunctionDefinitionInstructions.Clear();

            return mInstructions.ToArray();
        }

        protected void InitCapability()
        {
            mCapabilityInstructions.Add(Pack(2, spv.Op.OpCapability)); //size,Type
            mCapabilityInstructions.Add((UInt32)spv.Capability.CapabilityShader); //Capability
        }

        protected void InitExtensions()
        {
            mGlslExtensionId = GetNextId();
            string importStatement = "GLSL.std.450";
            //The spec says 3+variable but there are only 2 before string literal.
            UInt16 stringWordSize = (UInt16)(2 + (importStatement.Length / 4));
            if (importStatement.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mExtensionInstructions.Add(Pack(stringWordSize, spv.Op.OpExtInstImport)); //size,Type
            mExtensionInstructions.Add(mGlslExtensionId); //Result Id
            PutStringInList(importStatement, mExtensionInstructions);
        }

        protected void InitMemoryModel()
        {
            mMemoryModelInstructions.Add(Pack(3, spv.Op.OpMemoryModel)); //size,Type
            mMemoryModelInstructions.Add((UInt32)spv.AddressingModel.AddressingModelLogical); //Addressing Model
            mMemoryModelInstructions.Add((UInt32)spv.MemoryModel.MemoryModelGLSL450); //Memory Model
        }

        protected void InitSourceLanguage()
        {
            mSourceInstructions.Add(Pack(3, spv.Op.OpSource)); //size,Type
            mSourceInstructions.Add((UInt32)spv.SourceLanguage.SourceLanguageGLSL); //Source Language
            mSourceInstructions.Add(400); //Version

            string sourceExtension1 = "GL_ARB_separate_shader_objects";
            UInt16 stringWordSize = (UInt16)(2 + (sourceExtension1.Length / 4));
            if (sourceExtension1.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mSourceExtensionInstructions.Add(Pack(stringWordSize, spv.Op.OpSourceExtension)); //size,Type
            PutStringInList(sourceExtension1, mSourceExtensionInstructions);

            string sourceExtension2 = "GL_ARB_shading_language_420pack";
            stringWordSize = (UInt16)(2 + (sourceExtension2.Length / 4));
            if (sourceExtension2.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mSourceExtensionInstructions.Add(Pack(stringWordSize, spv.Op.OpSourceExtension)); //size,Type
            PutStringInList(sourceExtension2, mSourceExtensionInstructions);
        }

        protected void InitEntryPoint()
        {
            //The spec says 4+variable but there are only 3 before the string literal.
            UInt16 stringWordSize = (UInt16)(3 + (mEntryPointName.Length / 4) + mInputRegisters.Count + mOutputRegisters.Count);
            if (mEntryPointName.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mEntryPointInstructions.Add(Pack(stringWordSize, spv.Op.OpEntryPoint)); //size,Type

            if (mType == typeof(VertexShader))
            {
                mEntryPointInstructions.Add((UInt32)spv.ExecutionModel.ExecutionModelVertex); //Execution Model
            }
            else if (mType == typeof(FragmentShader))
            {
                mEntryPointInstructions.Add((UInt32)spv.ExecutionModel.ExecutionModelFragment); //Execution Model
            }
            else if (mType == typeof(GeometryShader))
            {
                mEntryPointInstructions.Add((UInt32)spv.ExecutionModel.ExecutionModelGeometry); //Execution Model
            }
            else if (mType == typeof(TessellationShader))
            {
                mEntryPointInstructions.Add((UInt32)spv.ExecutionModel.ExecutionModelTessellationControl); //Execution Model
                mEntryPointInstructions.Add((UInt32)spv.ExecutionModel.ExecutionModelTessellationEvaluation); //Execution Model
            }
            else
            {
                //ExecutionModelGLCompute
                mEntryPointInstructions.Add((UInt32)spv.ExecutionModel.ExecutionModelKernel); //Execution Model
            }

            mEntryPointInstructions.Add(mEntryPointId); //Entry Point (Id)
            PutStringInList(mEntryPointName, mEntryPointInstructions); //Name

            mEntryPointInstructions.AddRange(mInputRegisters);//Interfaces
            mEntryPointInstructions.AddRange(mOutputRegisters);//Interfaces
 
            //Write entry point name.
            PushName(mEntryPointId, mEntryPointName);

            //ExecutionMode
            if (mType == typeof(FragmentShader))
            {
                mExecutionModeInstructions.Add(Pack(3, spv.Op.OpExecutionMode)); //size,Type
                mExecutionModeInstructions.Add(mEntryPointId); //Entry Point (Id)
                mExecutionModeInstructions.Add((UInt32)spv.ExecutionMode.ExecutionModeOriginUpperLeft); //Execution Mode
            }
        }

        protected void InitHeader()
        {
            //Write SPIR-V header
            GeneratorMagicNumber generatorMagicNumber = new GeneratorMagicNumber { Version = 1, Type = 13 };
            mInstructions.Add(0x07230203); //spv::MagicNumber
            mInstructions.Add(0x00010200); //spv::Version
            mInstructions.Add(generatorMagicNumber.Word); //I don't have a generator number ATM.
            mInstructions.Add(GetNextId()); //Bound
            mInstructions.Add(0); //Reserved for instruction schema, if needed
        }

        public static UInt32[] Compile(Type type)
        {
            return new Compiler(type).Compile();
        }

    }
}
