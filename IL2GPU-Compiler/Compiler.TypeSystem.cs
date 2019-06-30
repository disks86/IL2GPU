using IL2GPU_API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace IL2GPU_Compiler
{
    public class TypeRequest
    {
        public Type mType = typeof(DBNull);
        public SpirVStorageClass mStorageClass = SpirVStorageClass.StorageClassInput;

        public TypeRequest()
        {
        }

        public TypeRequest(Type type, SpirVStorageClass storageClass)
        {
            mType = type;
            mStorageClass = storageClass;
        }

        public static bool operator <(TypeRequest obj1, TypeRequest obj2)
        {
            return Comparison(obj1, obj2) < 0;
        }

        public static bool operator >(TypeRequest obj1, TypeRequest obj2)
        {
            return Comparison(obj1, obj2) > 0;
        }

        public static bool operator ==(TypeRequest obj1, TypeRequest obj2)
        {
            return Comparison(obj1, obj2) == 0;
        }

        public static bool operator !=(TypeRequest obj1, TypeRequest obj2)
        {
            return Comparison(obj1, obj2) != 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeRequest)) return false;

            return this == (TypeRequest)obj;
        }

        public static bool operator <=(TypeRequest obj1, TypeRequest obj2)
        {
            return Comparison(obj1, obj2) <= 0;
        }

        public static bool operator >=(TypeRequest obj1, TypeRequest obj2)
        {
            return Comparison(obj1, obj2) >= 0;
        }

        public static int Comparison(TypeRequest obj1, TypeRequest obj2)
        {
            if (obj1.mType == obj2.mType && obj1.mStorageClass == obj2.mStorageClass)
            {
                return 0;
            }

            return -1;
        }
    }
    public partial class Compiler
    {
        List<UInt32> mTypeInstructions = new List<UInt32>();
        List<UInt32> mDecorateInstructions = new List<UInt32>();
        Dictionary<TypeRequest, UInt32> mTypeIds = new Dictionary<TypeRequest, UInt32>();

        /// <summary>
        /// Id the type associated with a provided id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TypeRequest GetIdType(UInt32 id)
        {
            foreach (var entry in mTypeIds)
            {
                if (entry.Value == id)
                {
                    return entry.Key;
                }
            }

            return new TypeRequest();
        }

        /// <summary>
        /// Fetch the id for the provided type. If the type doesn't exist add it to the type list and return the new id.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="storageClass"></param>
        /// <returns></returns>
        UInt32 GetTypeId(Type type, SpirVStorageClass storageClass)
        {
            return GetTypeId(new TypeRequest(type, storageClass));
        }

        /// <summary>
        /// Fetch the id for the provided type. If the type doesn't exist add it to the type list and return the new id.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        UInt32 GetTypeId(TypeRequest typeRequest)
        {
            var type = typeRequest.mType;
            var storageClass = typeRequest.mStorageClass;

            //We'll treat enum blah : int32 {...} the same as int32 for now.
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (mTypeIds.ContainsKey(typeRequest))
            {
                return mTypeIds[typeRequest];
            }

            var typeCode = Type.GetTypeCode(type);
            var id = GetNextId();

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    {
                        mTypeInstructions.Add(Utilities.Pack(2, SpirVOpCode.OpTypeBool)); //size,Type
                        mTypeInstructions.Add(id); //Id
                    }
                    break;
                case TypeCode.Byte:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(8); //Number of bits.
                        mTypeInstructions.Add(0); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.Char:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(8); //Number of bits.
                        mTypeInstructions.Add(0); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.DateTime:
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case TypeCode.DBNull:
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case TypeCode.Decimal:
                    {
                        mTypeInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpTypeFloat)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(128); //Number of bits.
                    }
                    break;
                case TypeCode.Double:
                    {
                        mTypeInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpTypeFloat)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(64); //Number of bits.
                    }
                    break;
                case TypeCode.Empty:
                    {
                        mTypeInstructions.Add(Utilities.Pack(2, SpirVOpCode.OpTypeVoid)); //size,Type
                        mTypeInstructions.Add(id); //Id
                    }
                    break;
                case TypeCode.Int16:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(16); //Number of bits.
                        mTypeInstructions.Add(1); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.Int32:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(32); //Number of bits.
                        mTypeInstructions.Add(1); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.Int64:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(64); //Number of bits.
                        mTypeInstructions.Add(1); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.Object:
                    {
                        if (type.IsPointer)
                        {
                            //OpTypePointer
                            var elementType = type.GetElementType();
                            var pointerTypeId = GetTypeId(elementType, storageClass);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypePointer)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            if (elementType == typeof(Sampler2D))
                            {
                                mTypeInstructions.Add((UInt32)SpirVStorageClass.StorageClassUniformConstant); //Storage Class
                            }
                            else
                            {
                                mTypeInstructions.Add((UInt32)storageClass); //Storage Class
                            }
                            mTypeInstructions.Add(pointerTypeId); // Type
                        }
                        else if (type == typeof(Vector4))
                        {
                            var columnTypeId = GetTypeId(typeof(float), storageClass);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeVector)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(4); //ComponentCount
                        }
                        else if (type == typeof(Matrix4x4))
                        {
                            var columnTypeId = GetTypeId(typeof(float), storageClass);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeMatrix)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(4); //ComponentCount

                            mDecorateInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpDecorate)); //size,Type
                            mDecorateInstructions.Add(id); //target (Id)
                            mDecorateInstructions.Add((UInt32)SpirVDecoration.DecorationColMajor); //Decoration Type (Id)	
                            //mDecorateInstructions.Add((UInt32)spv::DecorationRowMajor); //Decoration Type (Id)	
                        }
                        else if (type == typeof(Vector3))
                        {
                            var columnTypeId = GetTypeId(typeof(float), storageClass);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeVector)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(3); //ComponentCount
                        }
                        else if (type == typeof(Vector2))
                        {
                            var columnTypeId = GetTypeId(typeof(float), storageClass);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeVector)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(2); //ComponentCount
                        }
                        else if (type.IsArray)
                        {
                            var arrayTypeId = GetTypeId(type.GetElementType(), storageClass);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeArray)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(arrayTypeId); // Type
                            mTypeInstructions.Add(0 /*mConstantIntegerIds[type.]*/); // Length
                            //TODO: figure out how to handle array length.
                        }
                        else if (type == typeof(Sampler2D))
                        {
                            var id2 = id;
                            id = GetNextId();

                            //OpTypeSampler

                            var sampledTypeId = GetTypeId(typeof(float), storageClass);

                            mTypeInstructions.Add(Utilities.Pack(9, SpirVOpCode.OpTypeImage)); //size,Type
                            mTypeInstructions.Add(id2); //Result (Id)
                            mTypeInstructions.Add(sampledTypeId); //Sampled Type (Id)
                            mTypeInstructions.Add((UInt32)SpirVDim.Dim2D); //dimensionality
                            mTypeInstructions.Add(0); //Depth
                            mTypeInstructions.Add(0); //Arrayed
                            mTypeInstructions.Add(0); //MS
                            mTypeInstructions.Add(1); //Sampled
                            mTypeInstructions.Add((UInt32)SpirVImageFormat.ImageFormatUnknown); //Sampled

                            mTypeInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpTypeSampledImage)); //size,Type
                            mTypeInstructions.Add(id); //Result (Id)
                            mTypeInstructions.Add(id2); //Type (Id)
                        }
                        else //assume it's a custom structure with a valid layout.
                        {
                            var fields = type.GetFields();

                            mTypeInstructions.Add(Utilities.Pack((UInt16)(2 + fields.Length), SpirVOpCode.OpTypeStruct)); //size,Type
                            mTypeInstructions.Add(id); //Result (Id)

                            //TODO: add structure name.

                            int memberIndex = 0;
                            int memberOffset = 0;
                            for (int i = 0; i < fields.Length; i++)
                            {
                                var member = fields[i];
                                var memberType = member.GetType();
                                var memberTypeId = GetTypeId(memberType, storageClass);

                                mTypeInstructions.Add(memberTypeId);

                                mDecorateInstructions.Add(Utilities.Pack(4 + 1, SpirVOpCode.OpMemberDecorate)); //size,Type
                                mDecorateInstructions.Add(id); //target (Id)
                                mDecorateInstructions.Add((UInt32)memberIndex); //Member (Literal)
                                mDecorateInstructions.Add((UInt32)SpirVDecoration.DecorationOffset); //Decoration Type (Id)
                                mDecorateInstructions.Add((UInt32)memberOffset);

                                //TODO: add member names.

                                memberIndex += 1;
                                memberOffset += Marshal.SizeOf(memberType);
                            }
                        }
                    }
                    break;
                case TypeCode.SByte:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(8); //Number of bits.
                        mTypeInstructions.Add(1); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.Single:
                    {
                        mTypeInstructions.Add(Utilities.Pack(3, SpirVOpCode.OpTypeFloat)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(32); //Number of bits.
                    }
                    break;
                case TypeCode.String:
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case TypeCode.UInt16:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(16); //Number of bits.
                        mTypeInstructions.Add(0); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.UInt32:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(32); //Number of bits.
                        mTypeInstructions.Add(0); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                case TypeCode.UInt64:
                    {
                        mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeInt)); //size,Type
                        mTypeInstructions.Add(id); //Id
                        mTypeInstructions.Add(64); //Number of bits.
                        mTypeInstructions.Add(0); //Signedness (0 = unsigned,1 = signed)
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return 0;
        }

    }
}
