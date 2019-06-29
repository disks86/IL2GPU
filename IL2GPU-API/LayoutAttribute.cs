using System;
using System.Collections.Generic;
using System.Text;

namespace IL2GPU_API
{
    /// <summary>
    /// https://www.khronos.org/opengl/wiki/Layout_Qualifier_(GLSL)
    /// https://www.khronos.org/registry/OpenGL/extensions/ARB/ARB_enhanced_layouts.txt
    /// https://www.khronos.org/registry/spir-v/specs/1.2/SPIRV.html
    /// Shader stage input and output variables define a shader stage's interface. Depending on the available feature set, these variables can have layout qualifiers that define what resources they use.
    /// </summary>
    public class LayoutAttribute
        : Attribute
    {
        /// <summary>
        /// Shader inputs can specify the attribute index that the particular input uses.
        /// </summary>
        public int Location { get; set; } = -1;

        /// <summary>
        /// Buffer backed interface blocks and all opaque types have a setting which represents an index in the GL context where a buffer or texture object is bound so that it can be accessed through that interface. These binding points, like input attribute indices and output data locations, can be set from within the shader. This is done by using the "binding" layout qualifier.
        /// </summary>
        public int Binding { get; set; } = -1;

        /// <summary>
        /// The "align" qualifier can only be used on blocks or block members, and only for blocks declared with "std140" or "std430" layouts.The "align" qualifier makes the start of each block member have a minimum byte alignment. It does not affect the internal layout within each member, which will still follow the std140 or std430 rules.
        /// </summary>
        public int Align { get; set; } = -1;

        /// <summary>
        /// The offsets of contained values (whether in arrays, structs, or members of an interface block if the whole block has an offset) are computed, based on the sizes of prior components to pack them in the order specified. Any explicitly provided offsets are not allowed to violate alignment restrictions. So if a definition contains a double (either directly or indirectly), the offset must be 8-byte aligned.
        /// </summary>
        public int Offset { get; set; } = -1;

        /// <summary>
        /// Tessellation Control Shaders (TCS) output patches with a particular vertex count.
        /// </summary>
        public int Vertices { get; set; } = -1;

        /// <summary>
        /// Geometry Shaders take a particular primitive type as input and return a particular primitive type as outputs.
        /// </summary>
        public InputPrimitiveType InputPrimitiveType { get; set; }

        /// <summary>
        /// Geometry Shaders take a particular primitive type as input and return a particular primitive type as outputs.
        /// </summary>
        public OutputPrimitiveType OutputPrimitiveType { get; set; }

        /// <summary>
        /// Geometry shaders have a defined maximum number of vertices that they can output.
        /// </summary>
        public int MaximumVertices { get; set; } = -1;

        /// <summary>
        /// The qualifier origin_upper_left specifies that gl_FragCoord will have the origin (0, 0) in the upper-left of the screen. The standard OpenGL convention is to have it in the lower-left. This does not change the Z or W of the gl_FragCoord value.
        /// </summary>
        public Origin Origin { get; set; } = Origin.LowerLeft;

        /// <summary>
        /// The qualifier pixel_center_integer specifies that the X and Y of gl_FragCoord will be shifted by a half-pixel, so that the center of each pixel is an integer value. The standard OpenGL convention puts the integer values at the corner of the pixel.
        /// </summary>
        public PixelCenter PixelCenter { get; set; } = PixelCenter.Float;

        /// <summary>
        /// By the OpenGL specification, the depth and stencil tests are performed after the fragment shader's execution (implementations can and will do it before the fragment shader, but only if it won't affect the apparent output). However, with a fragment shader's ability to write to arbitrary images and buffers in OpenGL 4.2+, it is useful to be able to enforce early tests.
        /// </summary>
        public bool EarlyFragmentTests { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public Direction Direction { get; set; } = Direction.Private;

        /// <summary>
        /// Apply to an object or a member of a structure type. Indicates which built-in variable the entity represents. See BuiltIn for more information.
        /// </summary>
        public BuiltIn BuiltIn { get; set; } = BuiltIn.None;

        /// <summary>
        /// Apply to a scalar specialization constant. Forms the API linkage for setting a specialized value. See specialization.
        /// </summary>
        public int SpecId { get; set; } = -1;

        /// <summary>
        /// Applies only to a member of a structure type. Only valid on a matrix or array whose most basic element is a matrix.
        /// </summary>
        public int MatrixLayout { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        public bool PushConstant { get; set; } = false;

    }

    public enum InputPrimitiveType
    {
        Points
        , Lines
        , LinesAdjacency
        , Triangles
        , TrianglesAdjacency
    }

    public enum OutputPrimitiveType
    {
        Points
        , LineStrip
        , TriangleStrip
    }

    public enum Origin
    {
        LowerLeft,
        UpperLeft
    }

    public enum PixelCenter
    {
        Float,
        Integer
    }

    public enum Direction
    {
        Private,
        Input,
        Output     
    }

    public enum BuiltIn
    {
        None = -1,

        /// <summary>
        /// Output vertex position from a vertex processing Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        Position = 0,
        /// <summary>
        /// Output point size from a vertex processing Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        PointSize = 1,
        /// <summary>
        /// Array of clip distances. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        ClipDistance = 3,
        /// <summary>
        /// Array of clip distances. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        CullDistance = 4,
        /// <summary>
        /// Input vertex ID to a Vertex Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        VertexId = 5,
        /// <summary>
        /// Input instance ID to a Vertex Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        InstanceId = 6,
        /// <summary>
        /// Primitive ID in a Geometry Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        PrimitiveId = 7,
        /// <summary>
        /// Invocation ID, input to Geometry and TessellationControl Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        InvocationId = 8,
        /// <summary>
        /// Layer output by a Geometry Execution Model, input to a Fragment Execution Model, for multi-layer framebuffer. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        Layer = 9,
        /// <summary>
        /// Viewport Index output by a Geometry stage, input to a Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        ViewportIndex = 10,
        /// <summary>
        /// Output patch outer levels in a TessellationControl Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        TessLevelOuter = 11,
        /// <summary>
        /// Output patch inner levels in a TessellationControl Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        TessLevelInner = 12,
        /// <summary>
        /// Input vertex position in TessellationEvaluation Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        TessCoord = 13,
        /// <summary>
        /// Input patch vertex count in a tessellation Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        PatchVertices = 14,
        /// <summary>
        /// Coordinates (x, y, z, 1/w) of the current fragment, input to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        FragCoord = 15,
        /// <summary>
        /// Coordinates within a point, input to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        PointCoord = 16,
        /// <summary>
        /// Face direction, input to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        FrontFacing = 17,
        /// <summary>
        /// Input sample number to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        SampleId = 18,
        /// <summary>
        /// Input sample position to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        SamplePosition = 19,
        /// <summary>
        /// Input or output sample mask to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        SampleMask = 20,
        /// <summary>
        /// Output fragment depth from the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        FragDepth = 22,
        /// <summary>
        /// Input whether a helper invocation, to the Fragment Execution Model. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        HelperInvocation = 23,
        /// <summary>
        /// Number of workgroups in GLCompute or Kernel Execution Models. See OpenCL, Vulkan, or OpenGL API specifications for more detail.
        /// </summary>
        NumWorkgroups = 24,
        /// <summary>
        /// Work-group size in GLCompute or Kernel Execution Models. See OpenCL, Vulkan, or OpenGL API specifications for more detail.
        /// </summary>
        WorkgroupSize = 25,
        /// <summary>
        /// Work-group ID in GLCompute or Kernel Execution Models. See OpenCL, Vulkan, or OpenGL API specifications for more detail.
        /// </summary>
        WorkgroupId = 26,
        /// <summary>
        /// Local invocation ID in GLCompute or Kernel Execution Models. See OpenCL, Vulkan, or OpenGL API specifications for more detail.
        /// </summary>
        LocalInvocationId = 27,
        /// <summary>
        /// Global invocation ID in GLCompute or Kernel Execution Models. See OpenCL, Vulkan, or OpenGL API specifications for more detail.
        /// </summary>
        GlobalInvocationId = 28,
        /// <summary>
        /// Local invocation index in GLCompute Execution Models. See Vulkan or OpenGL API specifications for more detail. 
        /// Work-group Linear ID in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        LocalInvocationIndex = 29,
        /// <summary>
        /// Work dimensions in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        WorkDim = 30,
        /// <summary>
        /// Global size in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        GlobalSize = 31,
        /// <summary>
        /// Enqueued work-group size in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        EnqueuedWorkgroupSize = 32,
        /// <summary>
        /// Global offset in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        GlobalOffset = 33,
        /// <summary>
        /// Global linear ID in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        GlobalLinearId = 34,
        /// <summary>
        /// Subgroup size. See OpenCL or OpenGL API specifications for more detail.
        /// </summary>
        SubgroupSize = 36,
        /// <summary>
        /// Subgroup maximum size in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        SubgroupMaxSize = 37,
        /// <summary>
        /// Number of subgroups in GLCompute or Kernel Execution Models. See OpenCL or OpenGL API specifications for more detail.
        /// </summary>
        NumSubgroups = 38,
        /// <summary>
        /// Number of enqueued subgroups in Kernel Execution Models. See OpenCL API specification for more detail.
        /// </summary>
        NumEnqueuedSubgroups = 39,
        /// <summary>
        /// Subgroup ID in GLCompute or Kernel Execution Models. See OpenCL or OpenGL API specifications for more detail.
        /// </summary>
        SubgroupId = 40,
        /// <summary>
        /// Subgroup local invocation ID. See OpenCL or OpenGL API specifications for more detail.
        /// </summary>
        SubgroupLocalInvocationId = 41,
        /// <summary>
        /// Vertex index. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        VertexIndex = 42,
        /// <summary>
        /// Instance index. See Vulkan or OpenGL API specifications for more detail.
        /// </summary>
        InstanceIndex = 43,
        SubgroupEqMaskKHR = 4416,
        SubgroupGeMaskKHR = 4417,
        SubgroupGtMaskKHR = 4418,
        SubgroupLeMaskKHR = 4419,
        SubgroupLtMaskKHR = 4420,
        BaseVertex = 4424,
        BaseInstance = 4425,
        DrawIndex = 4426,
        DeviceIndex = 4438,
        ViewIndex = 4440,
        BaryCoordNoPerspAMD = 4992,
        BaryCoordNoPerspCentroidAMD = 4993,
        BaryCoordNoPerspSampleAMD = 4994,
        BaryCoordSmoothAMD = 4995,
        BaryCoordSmoothCentroidAMD = 4996,
        BaryCoordSmoothSampleAMD = 4997,
        BaryCoordPullModelAMD = 4998,
        FragStencilRefEXT = 5014,
        ViewportMaskNV = 5253,
        SecondaryPositionNV = 5257,
        SecondaryViewportMaskNV = 5258,
        PositionPerViewNV = 5261,
        ViewportMaskPerViewNV = 5262,
        FullyCoveredEXT = 5264
    }

    public enum MatrixLayout
    {
        /// <summary>
        /// Applies only to a member of a structure type. Only valid on a matrix or array whose most basic element is a matrix. Indicates that components within a row are contiguous in memory.
        /// </summary>
        RowMajor = 4,
        /// <summary>
        /// Applies only to a member of a structure type. Only valid on a matrix or array whose most basic element is a matrix. Indicates that components within a column are contiguous in memory.
        /// </summary>
        ColMajor = 5
    }

}
