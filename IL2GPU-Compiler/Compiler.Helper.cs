using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IL2GPU_Compiler
{
    public partial class Compiler
    {
        UInt32 mBaseId = 1;
        UInt32 mNextId = 1;

        Dictionary<UInt32, string> mNameIdPairs = new Dictionary<UInt32, string>();

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
            Utilities.PutStringInList(registerName, nameInstructions); //Literal

            mNameInstructions.Add(Utilities.Pack((UInt16)(nameInstructions.Count + 2), SpirVOpCode.OpName));
            mNameInstructions.Add(id); //target (Id)
            mNameInstructions.AddRange(nameInstructions);

            mNameIdPairs[id] = registerName;
        }

        void PushMemberName(UInt32 id, string registerName, UInt32 index)
        {
            var nameInstructions = new List<UInt32>();
            Utilities.PutStringInList(registerName, nameInstructions); //Literal

            mNameInstructions.Add(Utilities.Pack((UInt16)(nameInstructions.Count + 3), SpirVOpCode.OpMemberName));
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

            Push(SpirVOpCode.OpCompositeExtract, resultTypeId, resultId, baseId, index);
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

            Push(SpirVOpCode.OpCompositeExtract, resultTypeId, resultId, baseId, index1, index2);
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

            PushAccessChain(resultTypeId, resultId, baseId, mConstantIntegerIds[index]);

            return resultId;
        }

        void PushAccessChain(UInt32 resultTypeId, UInt32 resultId, UInt32 baseId, UInt32 indexId)
        {
            string registerName = mNameIdPairs[baseId];
            if (registerName.Length > 0)
            {
                if (indexId == mConstantIntegerIds[0])
                {
                    registerName += "[0]";
                }
                else if (indexId == mConstantIntegerIds[1])
                {
                    registerName += "[1]";
                }
                else if (indexId == mConstantIntegerIds[2])
                {
                    registerName += "[2]";
                }
                else if (indexId == mConstantIntegerIds[3])
                {
                    registerName += "[3]";
                }

                PushName(resultId, registerName);
            }

            Push(SpirVOpCode.OpAccessChain, resultTypeId, resultId, baseId, indexId);
        }

        void PushInverseSqrt(UInt32 resultTypeId, UInt32 resultId, UInt32 argumentId)
        {
            Push(SpirVOpCode.OpExtInst, resultTypeId, resultId, mGlslExtensionId, SpirVGLSLstd450.GLSLstd450InverseSqrt, argumentId);
        }

        void PushCos(UInt32 resultTypeId, UInt32 resultId, UInt32 argumentId)
        {
            Push(SpirVOpCode.OpExtInst, resultTypeId, resultId, mGlslExtensionId, SpirVGLSLstd450.GLSLstd450Cos, argumentId);
        }

        void PushSin(UInt32 resultTypeId, UInt32 resultId, UInt32 argumentId)
        {
            Push(SpirVOpCode.OpExtInst, resultTypeId, resultId, mGlslExtensionId, SpirVGLSLstd450.GLSLstd450Sin, argumentId);
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

            Push(SpirVOpCode.OpLoad, resultTypeId, resultId, pointerId);
        }

        void PushStore(UInt32 pointerId, UInt32 objectId)
        {
            Push(SpirVOpCode.OpStore, pointerId, objectId);
        }

        void PushVariable(UInt32 resultTypeId, UInt32 resultId, SpirVStorageClass storageClass)
        {
            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpVariable));
            mTypeInstructions.Add(resultTypeId);
            mTypeInstructions.Add(resultId);
            mTypeInstructions.Add((UInt32)storageClass);
        }

        void Push(SpirVOpCode code)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(1, code)); //size,Type
        }

        void Push(SpirVOpCode code, UInt32 argument1)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(2, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);

        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(3, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(4, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(5, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(6, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, SpirVGLSLstd450 argument4, UInt32 argument5)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(6, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add((UInt32)argument4);
            mFunctionDefinitionInstructions.Add(argument5);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(7, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, SpirVGLSLstd450 argument4, UInt32 argument5, UInt32 argument6)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(7, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add((UInt32)argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(8, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(9, code)); //size,Type
            mFunctionDefinitionInstructions.Add(argument1);
            mFunctionDefinitionInstructions.Add(argument2);
            mFunctionDefinitionInstructions.Add(argument3);
            mFunctionDefinitionInstructions.Add(argument4);
            mFunctionDefinitionInstructions.Add(argument5);
            mFunctionDefinitionInstructions.Add(argument6);
            mFunctionDefinitionInstructions.Add(argument7);
            mFunctionDefinitionInstructions.Add(argument8);


        }

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8, UInt32 argument9)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(10, code)); //size,Type
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

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8, UInt32 argument9, UInt32 argument10)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(11, code)); //size,Type
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

        void Push(SpirVOpCode code, UInt32 argument1, UInt32 argument2, UInt32 argument3, UInt32 argument4, UInt32 argument5, UInt32 argument6, UInt32 argument7, UInt32 argument8, UInt32 argument9, UInt32 argument10, UInt32 argument11)
        {
            mFunctionDefinitionInstructions.Add(Utilities.Pack(12, code)); //size,Type
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
            Push(SpirVOpCode.OpCompositeConstruct, vectorTypeId, resultId, xId, yId, zId);

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
            Push(SpirVOpCode.OpCompositeConstruct, vectorTypeId, v1Id, x1Id, y1Id, z1Id);

            //
            UInt32 x2Id = PushCompositeExtract2(id, 1, 0);
            UInt32 y2Id = PushCompositeExtract2(id, 1, 1);
            UInt32 z2Id = PushCompositeExtract2(id, 1, 2);

            UInt32 v2Id = GetNextId();
            mIdTypePairs[v2Id] = mIdTypePairs[vectorTypeId];
            Push(SpirVOpCode.OpCompositeConstruct, vectorTypeId, v2Id, x2Id, y2Id, z2Id);

            //
            UInt32 x3Id = PushCompositeExtract2(id, 2, 0);
            UInt32 y3Id = PushCompositeExtract2(id, 2, 1);
            UInt32 z3Id = PushCompositeExtract2(id, 2, 2);

            UInt32 v3Id = GetNextId();
            mIdTypePairs[v3Id] = mIdTypePairs[vectorTypeId];
            Push(SpirVOpCode.OpCompositeConstruct, vectorTypeId, v3Id, x3Id, y3Id, z3Id);

            //
            UInt32 resultId = GetNextId();
            //mIdTypePairs[resultId] = mIdTypePairs[matrixTypeId];
            //Push(SpirVOpCode.OpCompositeConstruct, matrixTypeId, resultId, v1Id, v2Id, v3Id);

            return resultId;
        }

    }
}
