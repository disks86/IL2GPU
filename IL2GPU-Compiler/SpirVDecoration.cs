﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IL2GPU_Compiler
{
    public enum SpirVDecoration : UInt32
    {
        DecorationRelaxedPrecision = 0,
        DecorationSpecId = 1,
        DecorationBlock = 2,
        DecorationBufferBlock = 3,
        DecorationRowMajor = 4,
        DecorationColMajor = 5,
        DecorationArrayStride = 6,
        DecorationMatrixStride = 7,
        DecorationGLSLShared = 8,
        DecorationGLSLPacked = 9,
        DecorationCPacked = 10,
        DecorationBuiltIn = 11,
        DecorationNoPerspective = 13,
        DecorationFlat = 14,
        DecorationPatch = 15,
        DecorationCentroid = 16,
        DecorationSample = 17,
        DecorationInvariant = 18,
        DecorationRestrict = 19,
        DecorationAliased = 20,
        DecorationVolatile = 21,
        DecorationConstant = 22,
        DecorationCoherent = 23,
        DecorationNonWritable = 24,
        DecorationNonReadable = 25,
        DecorationUniform = 26,
        DecorationSaturatedConversion = 28,
        DecorationStream = 29,
        DecorationLocation = 30,
        DecorationComponent = 31,
        DecorationIndex = 32,
        DecorationBinding = 33,
        DecorationDescriptorSet = 34,
        DecorationOffset = 35,
        DecorationXfbBuffer = 36,
        DecorationXfbStride = 37,
        DecorationFuncParamAttr = 38,
        DecorationFPRoundingMode = 39,
        DecorationFPFastMathMode = 40,
        DecorationLinkageAttributes = 41,
        DecorationNoContraction = 42,
        DecorationInputAttachmentIndex = 43,
        DecorationAlignment = 44,
        DecorationMaxByteOffset = 45,
        DecorationAlignmentId = 46,
        DecorationMaxByteOffsetId = 47,
        DecorationExplicitInterpAMD = 4999,
        DecorationOverrideCoverageNV = 5248,
        DecorationPassthroughNV = 5250,
        DecorationViewportRelativeNV = 5252,
        DecorationSecondaryViewportRelativeNV = 5256,
        DecorationHlslCounterBufferGOOGLE = 5634,
        DecorationHlslSemanticGOOGLE = 5635,
        //DecorationMax = 0x7fffffff
    };

    public enum SpirVDim : UInt32
    {
        Dim1D = 0,
        Dim2D = 1,
        Dim3D = 2,
        DimCube = 3,
        DimRect = 4,
        DimBuffer = 5,
        DimSubpassData = 6,
        //DimMax = 0x7fffffff,
    }

    public enum SpirVImageFormat : UInt32
    {
        ImageFormatUnknown = 0,
        ImageFormatRgba32f = 1,
        ImageFormatRgba16f = 2,
        ImageFormatR32f = 3,
        ImageFormatRgba8 = 4,
        ImageFormatRgba8Snorm = 5,
        ImageFormatRg32f = 6,
        ImageFormatRg16f = 7,
        ImageFormatR11fG11fB10f = 8,
        ImageFormatR16f = 9,
        ImageFormatRgba16 = 10,
        ImageFormatRgb10A2 = 11,
        ImageFormatRg16 = 12,
        ImageFormatRg8 = 13,
        ImageFormatR16 = 14,
        ImageFormatR8 = 15,
        ImageFormatRgba16Snorm = 16,
        ImageFormatRg16Snorm = 17,
        ImageFormatRg8Snorm = 18,
        ImageFormatR16Snorm = 19,
        ImageFormatR8Snorm = 20,
        ImageFormatRgba32i = 21,
        ImageFormatRgba16i = 22,
        ImageFormatRgba8i = 23,
        ImageFormatR32i = 24,
        ImageFormatRg32i = 25,
        ImageFormatRg16i = 26,
        ImageFormatRg8i = 27,
        ImageFormatR16i = 28,
        ImageFormatR8i = 29,
        ImageFormatRgba32ui = 30,
        ImageFormatRgba16ui = 31,
        ImageFormatRgba8ui = 32,
        ImageFormatR32ui = 33,
        ImageFormatRgb10a2ui = 34,
        ImageFormatRg32ui = 35,
        ImageFormatRg16ui = 36,
        ImageFormatRg8ui = 37,
        ImageFormatR16ui = 38,
        ImageFormatR8ui = 39
        //ImageFormatMax = 0x7fffffff,
    }

    public enum SpirVStorageClass : UInt32
    {
        StorageClassUniformConstant = 0,
        StorageClassInput = 1,
        StorageClassUniform = 2,
        StorageClassOutput = 3,
        StorageClassWorkgroup = 4,
        StorageClassCrossWorkgroup = 5,
        StorageClassPrivate = 6,
        StorageClassFunction = 7,
        StorageClassGeneric = 8,
        StorageClassPushConstant = 9,
        StorageClassAtomicCounter = 10,
        StorageClassImage = 11,
        StorageClassStorageBuffer = 12
        //StorageClassMax = 0x7fffffff,
    }

}
