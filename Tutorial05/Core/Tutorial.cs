using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;
using System;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {

        private IShaderParam _albedoParam;
        private float _alpha = 0.001f;
        private float _beta;
        private float shine;

        private float cameraPan;
        private float cameraZoom;
        private float cameraTilt = -0.5f;

        private SceneContainer _wuggy;
        private SceneContainer _floor;

        private TransformComponent _wuggyTrans;
        private TransformComponent _wheelBigL;
        private TransformComponent _wheelBigR;

        private TransformComponent _wheelSmallL;
        private TransformComponent _wheelSmallR;

        private TransformComponent _wuggyHead;

        private TransformComponent _floorTrans;

        private Renderer _renderer;


        public static Mesh LoadMesh(string assetName)
        {
            SceneContainer sc = AssetStorage.Get<SceneContainer>(assetName);
            MeshComponent mc = sc.Children.FindComponents<MeshComponent>(c => true).First();
            return new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            shine = 1.0f;

            _wuggy = AssetStorage.Get<SceneContainer>("wuggy.fus");
            _wuggyTrans = _wuggy.Children.First().GetTransform();
            _wheelBigL = _wuggy.Children.FindNodes(n => n.Name == "WheelBigL").First().GetTransform();
            _wheelBigR = _wuggy.Children.FindNodes(n => n.Name == "WheelBigR").First().GetTransform();
            _wheelSmallL = _wuggy.Children.FindNodes(n => n.Name == "WheelSmallL").First().GetTransform();
            _wheelSmallR = _wuggy.Children.FindNodes(n => n.Name == "WheelSmallR").First().GetTransform();
            _wuggyHead = _wuggy.Children.FindNodes(n => n.Name == "Eyes_Pitch").First().GetTransform();

            _floor = AssetStorage.Get<SceneContainer>("Cube.fus");
            _floorTrans = _floor.Children.FindNodes(n => n.Name == "Cube").First().GetTransform();
            _floorTrans.Scale = new float3(1.0f, 1.0f, 1.0f);
            _floorTrans.Translation = new float3(0f, 0f, 0f);

            _renderer = new Renderer(RC);

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot)
                   *float4x4.CreateRotationY(rot.y)
                   *float4x4.CreateRotationX(rot.x)
                   *float4x4.CreateRotationZ(rot.z)
                   *float4x4.CreateTranslation(-pivot);
        }

        void RenderSceneOb(SceneOb so, float4x4 modelView)
        {
            modelView = modelView * ModelXForm(so.Pos, so.Rot, so.Pivot) * float4x4.CreateScale(so.Scale);
            if (so.Mesh != null)
            {
                RC.ModelView = modelView*float4x4.CreateScale(so.ModelScale);
                RC.SetShaderParam(_albedoParam, so.Albedo);
                RC.Render(so.Mesh);
            }

            if (so.Children != null)
            {
                foreach (var child in so.Children)
                {
                    RenderSceneOb(child, modelView);
                }
            }
        }


        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }

            // Setup matrices
            var aspectRatio = Width / (float)Height;
            //RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 0.01f, 20);

            _wheelBigL.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);
            _wheelBigR.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);

            _wheelSmallL.Rotation += new float3(-0.1f * Keyboard.WSAxis, -0.05f * Keyboard.ADAxis, 0);
            if (_wheelSmallL.Rotation.y > 0.3f)
            {
                _wheelSmallL.Rotation.y = 0.3f;
            }
            else if (_wheelSmallL.Rotation.y < -0.3f)
            {
                _wheelSmallL.Rotation.y = -0.3f;
            }

            _wheelSmallR.Rotation += new float3(-0.1f * Keyboard.WSAxis, -0.05f * Keyboard.ADAxis, 0);
            if (_wheelSmallR.Rotation.y > 0.3f)
            {
                _wheelSmallR.Rotation.y = 0.3f;
            }
            else if (_wheelSmallR.Rotation.y < -0.3f)
            {
                _wheelSmallR.Rotation.y = -0.3f;
            }

            if (!Keyboard.GetKey(KeyCodes.A) && !Keyboard.GetKey(KeyCodes.D))
            {
                if (_wheelSmallR.Rotation.y > 0)
                {_wheelSmallR.Rotation.y -= 0.025f;}
                else if (_wheelSmallR.Rotation.y < 0)
                { _wheelSmallR.Rotation.y += 0.025f; }

                if (_wheelSmallL.Rotation.y > 0)
                { _wheelSmallL.Rotation.y -= 0.025f; }
                else if (_wheelSmallL.Rotation.y < 0)
                { _wheelSmallL.Rotation.y += 0.025f; }

            }

            if (Keyboard.GetKey(KeyCodes.W)) {
                _wuggyTrans.Rotation += new float3(0, _wheelSmallL.Rotation.y * -0.1f, 0); //_wheelSmallL.Rotation.y/-10
            } else if (Keyboard.GetKey(KeyCodes.S))
            {
                _wuggyTrans.Rotation += new float3(0, _wheelSmallL.Rotation.y * 0.1f, 0); //_wheelSmallL.Rotation.y/-10
            }

            //_wuggyTrans.Translation.x += Keyboard.WSAxis * -0.05f * (float)System.Math.Sin(_wuggyTrans.Rotation.y);
            _wuggyTrans.Translation += new float3(-0.05f * Keyboard.WSAxis * (float)System.Math.Sin(_wuggyTrans.Rotation.y), 0, -0.05f * Keyboard.WSAxis * (float)System.Math.Cos(_wuggyTrans.Rotation.y));

            if (Keyboard.GetKey(KeyCodes.Q))
            {
                cameraTilt += 0.05f;
                cameraTilt = Clamp(cameraTilt, -1.0f, 1.0f);
            }

            if (Keyboard.GetKey(KeyCodes.E))
            {
                cameraTilt -= 0.05f;
                cameraTilt = Clamp(cameraTilt, -1.0f, 1.0f);
            }

            _wuggyHead.Rotation.y = _wuggyTrans.Rotation.y * -1 + (float)System.Math.Atan2(_wuggyTrans.Translation.x, _wuggyTrans.Translation.z + 8f);
            _wuggyHead.Rotation.x = -cameraTilt;

            cameraPan += Keyboard.LeftRightAxis * 0.05f;
            cameraZoom += Keyboard.UpDownAxis * 0.05f;

            /*float4x4 view = float4x4.CreateTranslation(0, 0, 10 + _wuggyTrans.Translation.z) * float4x4.CreateRotationY((float)System.Math.PI + _wuggyTrans.Rotation.y) * float4x4.CreateTranslation(0, -0.5f, cameraZoom);
            _renderer.View = view;
            _renderer.Traverse(_wuggy.Children);
            */
            float4x4 view = float4x4.CreateTranslation(0, 0, 10)  * float4x4.CreateRotationX(cameraTilt) * float4x4.CreateRotationY(cameraPan) * float4x4.CreateTranslation(0, -0.5f, cameraZoom);
            _renderer.View = view;
            _renderer.Traverse(_wuggy.Children);

            if (Keyboard.GetKey(KeyCodes.R))
            {
                shine += 0.05f;
                shine = Clamp(shine, 0, 2);
            }
            if (Keyboard.GetKey(KeyCodes.F))
            {
                shine -= 0.05f;
                shine = Clamp(shine, 0, 2) ;
            }


            //shine += Keyboard.ADAxis;

            _renderer.RC.SetShaderParam(_renderer.ShininessFactorParam, shine);
            //_renderer.RC.SetShaderParam(_renderer.ShininessParam, shine);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
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
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

    }
}