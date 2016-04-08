using System;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private Mesh _mesh;
        private Mesh _mesh2;

        private List<ShaderProgram> _shaders = new List<ShaderProgram>();

        // Init is called on startup. 
        public override void Init()
        {

            // Set the clear color for the backbuffer to light green.
            RC.ClearColor = new float4(0.5f, 1, 0.7f, 1);

            _shaders.Add(RC.CreateShader(_vertexShader, _pixelShader));
            _shaders.Add(RC.CreateShader(_vertexShader, _pixelShader2));

            _mesh = new Mesh
            {
                Vertices = new[]
            {
                new float3(-0.75f, -0.75f, 0),
                new float3(0.75f, -0.75f, 0),
                new float3(0, 0.75f, 0)
            },
                Triangles = new ushort[] { 0, 1, 2},
            };

            _mesh2 = new Mesh
            {
                Vertices = new[]
            {
                new float3(0.75f, -0.75f, 0),
                new float3(0.95f, -0.75f, 0),
                new float3(0.85f, 0.75f, 0)
            },
                Triangles = new ushort[] { 0, 1, 2 },
            };
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            RC.SetShader(_shaders[0]);
            RC.Render(_mesh);

            RC.SetShader(_shaders[1]);
            RC.Render(_mesh2);

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

        private const string _vertexShader = @"
            attribute vec3 fuVertex;

            void main()
            {
                gl_Position = vec4(fuVertex, 1.0);
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif

            void main()
            {
                gl_FragColor = vec4(1, 0, 1, 1);
            }";

        private const string _pixelShader2 = @"
            #ifdef GL_ES
                precision highp float;
            #endif

            void main()
            {
                gl_FragColor = vec4(0, 0, 1, 1);
            }";
    }
}