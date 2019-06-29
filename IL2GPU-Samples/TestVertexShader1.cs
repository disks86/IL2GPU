using IL2GPU_API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IL2GPU_Samples
{
    public class Light
    {
        public Vector3 Direction;
        public uint IsEnabled;
        public Vector3 Position;
        public uint Filler;
        public Vector4 Color;
    }

    public class PushConstant
    {
        public Matrix4x4 modelViewProjection;
        public Matrix4x4 model;
    }

    public class TestVertexShader1
        : VertexShader
    {
        [Layout(Binding = 0)]
        Light skyLight;

        [Layout(PushConstant = true)]
        PushConstant pushConstant;

        [Layout(Location = 0, Direction = Direction.Input)]
        Vector3 position;

        [Layout(Location = 1, Direction = Direction.Input)]
        Vector3 normal;

        [Layout(Location = 2, Direction = Direction.Input)]
        Vector4 color;

        [Layout(Location = 3, Direction = Direction.Input)]
        Vector2 texcoord;

        [Layout(Location = 0, Direction = Direction.Output)]
        float outputColor;

        [Layout(Location = 1, Direction = Direction.Output)]
        Vector2 tex;

        [Layout(BuiltIn = BuiltIn.Position, Direction = Direction.Output)]
        Vector4 gl_Position;

        void main()
        {
            tex = texcoord;
            var temp = new Vector4(position, 1.0f);

            //gl_Position = pushConstant.modelViewProjection * temp;

            float diffuse = color.X;
            float ambient = color.Y;
            float specular = color.Z;
            float shininess = color.W;

            if (skyLight.IsEnabled == 1)
            {
                //float spec = 0.0f;
                //Vector3 n = new Vector3.Normalize(pushConstant.model * new Vector4(normal, 0.0f));
                //Vector3 ldir = Vector3.Normalize(skyLight.Direction);
                //float intensity = (float)Math.Max(Vector3.Dot(n, ldir), 0.0);
                //if (intensity > 0.0)
                //{
                //    Vector3 pos = new Vector3(pushConstant.model * new Vector4(position, 0.0f));
                //    Vector3 eye = Vector3.Normalize(-pos);
                //    Vector3 h = Vector3.Normalize(ldir + eye);
                //    float intSpec = (float)Math.Max(Vector3.Dot(h, n), 0.0);
                //    spec = specular * (float)Math.Pow(intSpec, shininess);
                //}
                //outputColor = Math.Max(intensity * diffuse + spec, ambient);
            }
            else
            {
                outputColor = 0.0f;
            }

        }

    }
}
