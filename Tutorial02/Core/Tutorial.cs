using System;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;
using static Fusee.Engine.Core.Input;


namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private IShaderParam _degreesParam;
        private float2 _degrees;

        private IShaderParam _mousePositionParam;
        private float2 _mousePosition;

        private Mesh _mesh;

        private const string _vertexShader = @"
        attribute vec3 fuVertex;
        uniform vec2 degrees;
        varying vec3 modelpos;
        varying mat4 xRotation;
        varying mat4 yRotation;

        void main()
        {
            modelpos = fuVertex;
            float s = sin(degrees.x);
            float c = cos(degrees.x);

            float s2 = sin(degrees.y);
            float c2 = cos(degrees.y);

            xRotation = mat4(   1,0,0,0,
                                0,c,-s,0,
                                0,s,c,0,
                                0,0,0,1);

            yRotation = mat4(   c2,0,s2,0,
                                0,1,0,0,
                                -s2,0,c2,0,
                                0,0,0,1);

            gl_Position = xRotation*yRotation*vec4(fuVertex,1.0);
        }";

        private const string _pixelShader = @"
        #ifdef GL_ES
            precision highp float;
        #endif
        varying vec3 modelpos;

        uniform vec2 mousePosition;
        float distance;

        void main()
        {
            distance = distance(vec3(mousePosition, 1), modelpos * 0.5 + 0.5);
            
            gl_FragColor = vec4(modelpos*0.5 + 0.5, 1) / (distance * 1.5f);
        }";


        // Init is called on startup. 
        public override void Init()
        {
            _mesh = new Mesh
            {
                Vertices = new[]
                {
                    new float3(-0.5f, 0, 0.5f),       // Vertex 0
                    new float3(0.5f, 0, 0.5f),        // Vertex 1
                    new float3(0.5f, 0.3f, 0.5f),     // Vertex 2
                    new float3(-0.5f, 0.3f, 0.5f),    // Vertex 3
                    new float3(0, 0.5f, 0.5f),        // Vertex 4
                    new float3(-0.5f, 0, -0.5f),      // Vertex 5
                    new float3(0.5f, 0, -0.5f),       // Vertex 6
                    new float3(0.5f, 0.3f, -0.5f),    // Vertex 7
                    new float3(-0.5f, 0.3f, -0.5f),   // Vertex 8
                    new float3(0, 0.5f, -0.5f),       // Vertex 9
                },
                Triangles = new ushort[]
                {
                    0, 1, 2,    //Wand vorn 1
                    0, 2, 3,    //Wand vorn 2
                    3, 2, 4,    //Wand vorn 3

                    0, 6, 1,    //Boden 1
                    0, 5, 6,    //Boden 2

                    1, 6, 7,    //Wand rechts 1
                    1, 7, 2,    //Wand rechts 2

                    6, 5, 8,    //Wand hinten 1
                    6, 8, 7,    //Wand hinten 2
                    7, 8, 9,    //Wand hinten 3

                    0, 8, 5,    //Wand links 1
                    0, 3, 8,    //Wand links 2

                    2, 7, 9,    //Dachseite rechts 1
                    2, 9, 4,    //Dachsetie rechts 2

                    8, 3, 4,    //Dachseite links 1
                    8, 4, 9     //Dachseite links 2
                },
            };

            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);
            _degreesParam = RC.GetShaderParam(shader, "degrees");
            _degrees = new float2 (0, 0);

            _mousePositionParam = RC.GetShaderParam(shader, "mousePosition");

            // Set the clear color for the backbuffer.
            RC.ClearColor = new float4(0.0f, 0.0f, 0.0f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            float2 speed = Mouse.Velocity;

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            RC.Render(_mesh);

            _mousePosition = new float2(Mouse.Position.x/Width, Mouse.Position.y/Height);

            if (Mouse.LeftButton)
            {
                _degrees.x += speed.y * 0.0001f;
                _degrees.y += speed.x * 0.0001f;
            }

            _degrees.x += Keyboard.UpDownAxis * 0.025f;
            _degrees.y += Keyboard.LeftRightAxis * 0.025f;

            RC.SetShaderParam(_degreesParam, _degrees);
            RC.SetShaderParam(_mousePositionParam, _mousePosition);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}