using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace IL2GPU_Compiler
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PackStructure
    {
        [FieldOffset(0)]
        public spv.Op Opcode;
        [FieldOffset(2)]
        public UInt16 WordCount;

        [FieldOffset(0)]
        public char Char1;
        [FieldOffset(1)]
        public char Char2;
        [FieldOffset(2)]
        public char Char3;
        [FieldOffset(3)]
        public char Char4;

        [FieldOffset(0)]
        public UInt32 Word;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct GeneratorMagicNumber
    {
        [FieldOffset(0)]
        public UInt16 Version;
        [FieldOffset(2)]
        public UInt16 Type;

        [FieldOffset(0)]
        public UInt32 Word;
    }

    public partial class Compiler
    {
        UInt32 mBaseId = 1;
        UInt32 mNextId = 1;

        Dictionary<UInt32, string> mNameIdPairs = new Dictionary<UInt32, string>();

        /// <summary>
        /// Combined the word count and opcode into a single word.
        /// </summary>
        /// <param name="wordCount"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        protected static UInt32 Pack(UInt16 wordCount, spv.Op opcode)
        {
            var opcodeDescription = new PackStructure();

            opcodeDescription.WordCount = wordCount;
            opcodeDescription.Opcode = opcode;

            return opcodeDescription.Word;
        }

        /// <summary>
        /// Pack 4 characters into a single word.
        /// </summary>
        /// <param name="char1"></param>
        /// <param name="char2"></param>
        /// <param name="char3"></param>
        /// <param name="char4"></param>
        /// <returns></returns>
        protected static UInt32 Pack(char char1, char char2, char char3, char char4)
        {
            var opcodeDescription = new PackStructure();

            opcodeDescription.Char1 = char1;
            opcodeDescription.Char2 = char2;
            opcodeDescription.Char3 = char3;
            opcodeDescription.Char4 = char4;

            return opcodeDescription.Word;
        }

        /// <summary>
        /// Pack a string into words (including null terminator) and add them to a list of words.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="words"></param>
        protected static void PutStringInList(string text, List<UInt32> words)
        {
            char[] value = text.ToCharArray();
            for (Int32 i = 0; i < (Int32)text.Length; i += 4)
            {
                Int32 difference = text.Length - (i);

                switch (difference)
                {
                    case 0:
                        break;
                    case 1:
                        words.Add(Pack('\0', '\0', '\0', value[i]));
                        break;
                    case 2:
                        words.Add(Pack('\0', '\0', value[i + 1], value[i]));
                        break;
                    case 3:
                        words.Add(Pack('\0', value[i + 2], value[i + 1], value[i]));
                        break;
                    default:
                        words.Add(Pack(value[i + 3], value[i + 2], value[i + 1], value[i]));
                        break;
                }
            }

            if (text.Length % 4 == 0)
            {
                words.Add(0); //null terminator if all words have characters.
            }
        }

        UInt32 GetNextId()
        {
            return mNextId++;
        }

        TypeRequest GetPointerComponentType(TypeRequest inputType)
        {
            var pointerOfElementType = inputType.mType.GetElementType().GetElementType().MakePointerType();

            return new TypeRequest(pointerOfElementType,inputType.mStorageClass);
        }

        TypeRequest GetComponentType(TypeRequest inputType)
        {
            var elementType = inputType.mType.GetElementType();

            return new TypeRequest(elementType, inputType.mStorageClass);
        }

        TypeRequest GetValueType(TypeRequest inputType)
        {
            var elementType = inputType.mType.GetElementType();

            return new TypeRequest(elementType, inputType.mStorageClass);
        }

        void PushName(UInt32 id, string registerName)
        {
            var nameInstructions = new List<UInt32>();
            PutStringInList(registerName, nameInstructions); //Literal

            mNameInstructions.Add(Pack((UInt16)(nameInstructions.Count + 2), spv.Op.OpName));
            mNameInstructions.Add(id); //target (Id)
            mNameInstructions.AddRange(nameInstructions);

            mNameIdPairs[id] = registerName;
        }

        void PushMemberName(UInt32 id, string registerName, UInt32 index)
        {
            var nameInstructions = new List<UInt32>();
            PutStringInList(registerName, nameInstructions); //Literal

            mNameInstructions.Add(Pack((UInt16)(nameInstructions.Count + 3), spv.Op.OpMemberName));
            mNameInstructions.Add(id); //target (Id)
            mNameInstructions.Add(index);
            mNameInstructions.AddRange(nameInstructions);

            mNameIdPairs[id] = registerName;
        }

        UInt32 PushCompositeExtract(UInt32 baseId, UInt32 index)
        {
            UInt32 resultId = GetNextId();

            PushCompositeExtract(resultId, baseId, index);

            return resultId;
        }

        UInt32 PushCompositeExtract(UInt32 resultId, UInt32 baseId, UInt32 index)
        {
            var baseType = mIdTypePairs[baseId];
            UInt32 baseTypeId = GetTypeId(baseType);

            var resultType = GetComponentType(baseType);
            UInt32 resultTypeId = GetTypeId(resultType);

            mIdTypePairs[resultId] = resultType;

            PushCompositeExtract(resultTypeId, resultId, baseId, index);

            return resultId;
        }

        void PushCompositeExtract(UInt32 resultTypeId, UInt32 resultId, UInt32 baseId, UInt32 index)
        {
            string registerName = mNameIdPairs[baseId];
            if (registerName.Length > 0)
            {
                if (index == 0)
                {
                    registerName += ".x";
                }
                else if (index == 1)
                {
                    registerName += ".y";
                }
                else if (index == 2)
                {
                    registerName += ".z";
                }
                else if (index == 3)
                {
                    registerName += ".w";
                }

                PushName(resultId, registerName);
            }

            Push(spv.Op.OpCompositeExtract, resultTypeId, resultId, baseId, index);
        }

        UInt32 PushCompositeExtract2(UInt32 baseId, UInt32 index1, UInt32 index2)
        {
            UInt32 resultId = GetNextId();

            PushCompositeExtract2(resultId, baseId, index1, index2);

            return resultId;
        }

        UInt32 PushCompositeExtract2(UInt32 resultId, UInt32 baseId, UInt32 index1, UInt32 index2)
        {
            var baseType = mIdTypePairs[baseId];
            UInt32 baseTypeId = GetTypeId(baseType);

            var resultType = GetComponentType(baseType);
            UInt32 resultTypeId = GetTypeId(resultType);

            mIdTypePairs[resultId] = resultType;

            PushCompositeExtract2(resultTypeId, resultId, baseId, index1, index2);

            return resultId;
        }

        void PushCompositeExtract2(UInt32 resultTypeId, UInt32 resultId, UInt32 baseId, UInt32 index1, UInt32 index2)
        {
            string registerName = mNameIdPairs[baseId];
            if (registerName.Length > 0)
            {
                registerName += "[" + index1.ToString() + "][" + index2.ToString() + "]";

                PushName(resultId, registerName);
            }

            Push(spv.Op.OpCompositeExtract, resultTypeId, resultId, baseId, index1, index2);
        }

        UInt32 PushAccessChain(UInt32 baseId, UInt32 index)
        {
            UInt32 resultId = GetNextId();

            PushAccessChain(resultId, baseId, index);

            return resultId;
        }

        UInt32 PushAccessChain(UInt32 resultId, UInt32 baseId, UInt32 index)
        {
            var baseType = mIdTypePairs[baseId];
            UInt32 baseTypeId = GetTypeId(baseType);

            var resultType = GetPointerComponentType(baseType);
            UInt32 resultTypeId = GetTypeId(resultType);

            mIdTypePairs[resultId] = resultType;

            PushAccessChain(resultTypeId, resultId, baseId, GetConstantId(index));

            return resultId;
        }

        void PushAccessChain(UInt32 resultTypeId, UInt32 resultId, UInt32 baseId, UInt32 indexId)
        {
            string registerName = mNameIdPairs[baseId];
            if (registerName.Length > 0)
            {
                if (indexId == GetConstantId(0))
                {
                    registerName += "[0]";
                }
                else if (indexId == GetConstantId(1))
                {
                    registerName += "[1]";
                }
                else if (indexId == GetConstantId(2))
                {
                    registerName += "[2]";
                }
                else if (indexId == GetConstantId(3))
                {
                    registerName += "[3]";
                }

                PushName(resultId, registerName);
            }

            Push(spv.Op.OpAccessChain, resultTypeId, resultId, baseId, indexId);
        }

        void PushInverseSqrt(UInt32 resultTypeId, UInt32 resultId, UInt32 argumentId)
        {
            Push(spv.Op.OpExtInst, resultTypeId, resultId, mGlslExtensionId, spv.GLSLstd450.GLSLstd450InverseSqrt, argumentId);
        }

        void PushCos(UInt32 resultTypeId, UInt32 resultId, UInt32 argumentId)
        {
            Push(spv.Op.OpExtInst, resultTypeId, resultId, mGlslExtensionId, spv.GLSLstd450.GLSLstd450Cos, argumentId);
        }

        void PushSin(UInt32 resultTypeId, UInt32 resultId, UInt32 argumentId)
        {
            Push(spv.Op.OpExtInst, resultTypeId, resultId, mGlslExtensionId, spv.GLSLstd450.GLSLstd450Sin, argumentId);
        }

        UInt32 PushLoad(UInt32 pointerId)
        {
            UInt32 resultId = GetNextId();

            PushLoad(resultId, pointerId);

            return resultId;
        }

        UInt32 PushLoad(UInt32 resultId, UInt32 pointerId)
        {
            var pointerType = mIdTypePairs[pointerId];
            UInt32 pointerTypeId = GetTypeId(pointerType);

            var resultType = GetValueType(pointerType);
            UInt32 resultTypeId = GetTypeId(resultType);

            mIdTypePairs[resultId] = resultType;

            PushLoad(resultTypeId, resultId, pointerId);

            return resultId;
        }

        void PushLoad(UInt32 resultTypeId, UInt32 resultId, UInt32 pointerId)
        {
            string registerName = mNameIdPairs[pointerId];
            if (registerName.Length > 0)
            {
                registerName += "_loaded";
                PushName(resultId, registerName);
            }

            Push(spv.Op.OpLoad, resultTypeId, resultId, pointerId);
        }

        void PushStore(UInt32 pointerId, UInt32 objectId)
        {
            Push(spv.Op.OpStore, pointerId, objectId);
        }

        void PushVariable(UInt32 resultTypeId, UInt32 resultId, spv.StorageClass storageClass)
        {
            mTypeInstructions.Add(Pack(4, spv.Op.OpVariable));
            mTypeInstructions.Add(resultTypeId);
            mTypeInstructions.Add(resultId);
            mTypeInstructions.Add((UInt32)storageClass);
        }

        void Push(spv.Op code)
        {
            mFunctionDefinitionInstructions.Add(Pack(1, code)); //size,Type
        }

        void Push(spv.Op code, UInt32 argument1)
        {
            mFunctionDefinitionInstructions.Add(Pack(2, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);

        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2)
        {
            mFunctionDefinitionInstructions.Add(Pack(3, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3)
        {
            mFunctionDefinitionInstructions.Add(Pack(4, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4)
        {
            mFunctionDefinitionInstructions.Add(Pack(5, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5)
        {
            mFunctionDefinitionInstructions.Add(Pack(6, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, spv.GLSLstd450 argument4, UInt32 argument5)
        {
            mFunctionDefinitionInstructions.Add(Pack(6, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add((UInt32)argument4);
            mFunctionDefinitionInstructions.Add(argument5);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6)
        {
            mFunctionDefinitionInstructions.Add(Pack(7, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, spv.GLSLstd450 argument4, UInt32 argument5, UInt32 argument6)
        {
            mFunctionDefinitionInstructions.Add(Pack(7, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add((UInt32)argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7)
        {
            mFunctionDefinitionInstructions.Add(Pack(8, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8)
        {
            mFunctionDefinitionInstructions.Add(Pack(9, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
            mFunctionDefinitionInstructions.Add(argument8);


        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8, UInt32 argument9)
        {
            mFunctionDefinitionInstructions.Add(Pack(10, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
            mFunctionDefinitionInstructions.Add(argument8);
            mFunctionDefinitionInstructions.Add(argument9);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8, UInt32 argument9, UInt32 argument10)
        {
            mFunctionDefinitionInstructions.Add(Pack(11, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
            mFunctionDefinitionInstructions.Add(argument8);
            mFunctionDefinitionInstructions.Add(argument9);
            mFunctionDefinitionInstructions.Add(argument10);
        }

        void Push(spv.Op code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8, UInt32 argument9, UInt32 argument10, UInt32 argument11)
        {
            mFunctionDefinitionInstructions.Add(Pack(12, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
            mFunctionDefinitionInstructions.Add(argument8);
            mFunctionDefinitionInstructions.Add(argument9);
            mFunctionDefinitionInstructions.Add(argument10);
            mFunctionDefinitionInstructions.Add(argument11);
        }

        UInt32 ConvertVec4ToVec3(UInt32 id)
        {
            //Check to see if the input is a pointer and if so load it first.
            var originalType = mIdTypePairs[id];
            UInt32 originalTypeId = GetTypeId(originalType);

            //Finally build the new vector.
            UInt32 floatTypeId = GetTypeId(typeof(float), originalType.mStorageClass);
            UInt32 vectorTypeId = GetTypeId(typeof(Vector3), originalType.mStorageClass);

            //
            UInt32 xId = PushCompositeExtract(id, 0);
            UInt32 yId = PushCompositeExtract(id, 1);
            UInt32 zId = PushCompositeExtract(id, 1);

            //
            UInt32 resultId = GetNextId();
            mIdTypePairs[resultId] = mIdTypePairs[vectorTypeId];
            Push(spv.Op.OpCompositeConstruct, vectorTypeId, resultId, xId, yId, zId);

            return resultId;
        }

        UInt32 ConvertMat4ToMat3(UInt32 id)
        {
            throw new NotImplementedException(); //Waiting on Matrix3x3

            //Check to see if the input is a pointer and if so load it first.
            var originalType = mIdTypePairs[id];
            UInt32 originalTypeId = GetTypeId(originalType);

            //Finally build the new matrix.
            UInt32 floatTypeId = GetTypeId(typeof(float), originalType.mStorageClass);
            UInt32 vectorTypeId = GetTypeId(typeof(Vector3), originalType.mStorageClass);
            //UInt32 matrixTypeId = GetTypeId(typeof(Matrix3x3), originalType.mStorageClass);
            //
            UInt32 x1Id = PushCompositeExtract2(id, 0, 0);
            UInt32 y1Id = PushCompositeExtract2(id, 0, 1);
            UInt32 z1Id = PushCompositeExtract2(id, 0, 2);

            UInt32 v1Id = GetNextId();
            mIdTypePairs[v1Id] = mIdTypePairs[vectorTypeId];
            Push(spv.Op.OpCompositeConstruct, vectorTypeId, v1Id, x1Id, y1Id, z1Id);

            //
            UInt32 x2Id = PushCompositeExtract2(id, 1, 0);
            UInt32 y2Id = PushCompositeExtract2(id, 1, 1);
            UInt32 z2Id = PushCompositeExtract2(id, 1, 2);

            UInt32 v2Id = GetNextId();
            mIdTypePairs[v2Id] = mIdTypePairs[vectorTypeId];
            Push(spv.Op.OpCompositeConstruct, vectorTypeId, v2Id, x2Id, y2Id, z2Id);

            //
            UInt32 x3Id = PushCompositeExtract2(id, 2, 0);
            UInt32 y3Id = PushCompositeExtract2(id, 2, 1);
            UInt32 z3Id = PushCompositeExtract2(id, 2, 2);

            UInt32 v3Id = GetNextId();
            mIdTypePairs[v3Id] = mIdTypePairs[vectorTypeId];
            Push(spv.Op.OpCompositeConstruct, vectorTypeId, v3Id, x3Id, y3Id, z3Id);

            //
            UInt32 resultId = GetNextId();
            //mIdTypePairs[resultId] = mIdTypePairs[matrixTypeId];
            //Push(spv.Op.OpCompositeConstruct, matrixTypeId, resultId, v1Id, v2Id, v3Id);

            return resultId;
        }

    }
}
