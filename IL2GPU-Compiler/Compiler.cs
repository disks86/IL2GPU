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

        Compiler(Type type)
        {
            mType = type;
            mVariables = mType.GetFields();
            mMethods = mType.GetMethods();
        }

        public byte[] Compile()
        {
            //Capability
            mCapabilityInstructions.Add(Utilities.Pack(2, SpirVOpCode.OpCapability)); //size,Type
            mCapabilityInstructions.Add(spv::CapabilityShader); //Capability

            //Import
            mGlslExtensionId = GetNextId();
            string importStatement = "GLSL.std.450";
            //The spec says 3+variable but there are only 2 before string literal.
            UInt16 stringWordSize = (UInt16)(2 + (importStatement.Length / 4));
            if (importStatement.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mExtensionInstructions.Add(Utilities.Pack(stringWordSize, SpirVOpCode.OpExtInstImport)); //size,Type
            mExtensionInstructions.Add(mGlslExtensionId); //Result Id
            Utilities.PutStringInList(importStatement, mExtensionInstructions);

            //Memory Model
            mMemoryModelInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpMemoryModel)); //size,Type
            mMemoryModelInstructions.Add(spv::AddressingModelLogical); //Addressing Model
            mMemoryModelInstructions.Add(spv::MemoryModelGLSL450); //Memory Model

            //mSourceInstructions
            mSourceInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpSource)); //size,Type
            mSourceInstructions.Add(spv::SourceLanguageGLSL); //Source Language
            mSourceInstructions.Add(400); //Version

            string sourceExtension1 = "GL_ARB_separate_shader_objects";
            stringWordSize = (UInt16)(2 + (sourceExtension1.Length / 4));
            if (sourceExtension1.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mSourceExtensionInstructions.Add(Utilities.Pack(stringWordSize, SpirVOpCode.OpSourceExtension)); //size,Type
            Utilities.PutStringInList(sourceExtension1, mSourceExtensionInstructions);

            string sourceExtension2 = "GL_ARB_shading_language_420pack";
            stringWordSize = (UInt16)(2 + (sourceExtension2.Length / 4));
            if (sourceExtension2.Length % 4 == 0)
            {
                stringWordSize++;
            }
            mSourceExtensionInstructions.Add(Utilities.Pack(stringWordSize, SpirVOpCode.OpSourceExtension)); //size,Type
            Utilities.PutStringInList(sourceExtension2, mSourceExtensionInstructions);




            throw new NotImplementedException();
        }
       
        public static byte[] Compile(Type type)
        {
            return new Compiler(type).Compile();
        }

    }
}
