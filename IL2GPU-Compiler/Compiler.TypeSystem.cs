using IL2GPU_API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IL2GPU_Compiler
{
    public partial class Compiler
    {
        List<UInt32> mTypeInstructions = new List<UInt32>();
        List<UInt32> mDecorateInstructions = new List<UInt32>();
        Dictionary<Type, UInt32> mTypeIds = new Dictionary<Type, UInt32>();

        /// <summary>
        /// Id the type associated with a provided id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Type GetIdType(UInt32 id)
        {
            foreach (var entry in mTypeIds)
            {
                if (entry.Value == id)
                {
                    return entry.Key;
                }
            }

            return typeof(DBNull);
        }

        /// <summary>
        /// Fetch the id for the provided type. If the type doesn't exist add it to the type list and return the new id.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        UInt32 GetTypeId(Type type)
        {
            //We'll treat enum blah : int32 {...} the same as int32 for now.
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (mTypeIds.ContainsKey(type))
            {
                return mTypeIds[type];
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
                            var pointerTypeId = GetTypeId(elementType);

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypePointer)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            //if (elementType == typeof(Sampler2D))
                            //{
                            //    mTypeInstructions.Add(spv::StorageClassUniformConstant); //Storage Class
                            //}
                            //else
                            //{
                            //    mTypeInstructions.Add(registerType.StorageClass); //Storage Class
                            //}
                            mTypeInstructions.Add(pointerTypeId); // Type
                        }
                        else if (type == typeof(Vector4))
                        {
                            var columnTypeId = GetTypeId(typeof(float));

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeVector)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(4); //ComponentCount
                        }
                        else if (type == typeof(Matrix4x4))
                        {
                            var columnTypeId = GetTypeId(typeof(float));

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
                            var columnTypeId = GetTypeId(typeof(float));

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeVector)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(3); //ComponentCount
                        }
                        else if (type == typeof(Vector2))
                        {
                            var columnTypeId = GetTypeId(typeof(float));

                            mTypeInstructions.Add(Utilities.Pack(4, SpirVOpCode.OpTypeVector)); //size,Type
                            mTypeInstructions.Add(id); //Id
                            mTypeInstructions.Add(columnTypeId); //Component/Column Type
                            mTypeInstructions.Add(2); //ComponentCount
                        }
                        else if (type.IsArray)
                        {
                            var arrayTypeId = GetTypeId(type.GetElementType());

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

                            var sampledTypeId = GetTypeId(typeof(float));

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
