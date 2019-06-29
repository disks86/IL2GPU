using IL2GPU_API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IL2GPU_Samples
{
    public class TestFragmentShader1
        : FragmentShader
    {

        [Layout(Binding = 1)]
        Sampler2D[] textures = new Sampler2D[6]; //uniform

        [Layout(Location = 0,Direction = Direction.Input)]
        float color;

        [Layout(Location = 1, Direction = Direction.Input)]
        Vector2 texcoord;

        [Layout(Location = 0, Direction = Direction.Output)]
        Vector4 uFragColor;

        void main()
        {
            //var tex = ShaderFunctions.Texture(textures[(uint)(Math.Clamp(color, 0.0, 1.0) * 5)], texcoord);
            //uFragColor = new Vector4(tex.X, tex.Y, tex.Z, 1.0f);
        }

    }
}
