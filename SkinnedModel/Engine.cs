using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DopaEngine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Engine : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SkinnedModel CharacterMesh;
        
        SkinnedModelAnimation AnimationIdle;
        SkinnedModelAnimation AnimationRunning;
        SkinnedModelAnimation AnimationWalk;
        SkinnedModelInstance ModelInstance;

        public Effect SimpleModelEffect { get; set; }
        public Effect SkinnedModelEffect { get; set; }

        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public float HeadYRotation { get; set; } = 0;

        SimpleModel ModelRifle { get; set; }

        SkinnedModelInstance.BoneAnimationInstance HeadBoneAnimationInstance { get; set; }
        SkinnedModelInstance.BoneAnimationInstance HandBoneAnimationInstance { get; set; }
        Matrix RifleTransformation { get; set; }

        Texture2D CharacterTexture { get; set; }
        Texture2D RifleTexture { get; set; }

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += GraphicsDeviceManager_PreparingDeviceSettings;
            Content.RootDirectory = "Content";
        }

        private void GraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            CharacterTexture = Content.Load<Texture2D>("swat");
            RifleTexture = Content.Load<Texture2D>("hk_mp5n");

            CharacterMesh = new SkinnedModel();
            CharacterMesh.GraphicsDevice = GraphicsDevice;
            CharacterMesh.FilePath = @"Content\Soldier.fbx";
            CharacterMesh.Initialize();
            CharacterMesh.Meshes[0].Texture = Content.Load<Texture2D>("swat");

            AnimationIdle = new SkinnedModelAnimation();
            AnimationIdle.FilePath = @"Content\Rifle Idle.dae";
            AnimationIdle.Load();

            AnimationRunning = new SkinnedModelAnimation();
            AnimationRunning.FilePath = @"Content\Rifle Run.dae";
            AnimationRunning.Load();

            AnimationWalk = new SkinnedModelAnimation();
            AnimationWalk.FilePath = @"Content\Rifle Walking.dae";
            AnimationWalk.Load();
            

            ModelRifle = new SimpleModel();
            ModelRifle.GraphicsDevice = GraphicsDevice;
            ModelRifle.FilePath = @"Content\mp5.3ds";
            ModelRifle.Initialize();

            ModelInstance = new SkinnedModelInstance();
            ModelInstance.Mesh = CharacterMesh;
            ModelInstance.SpeedTransitionSecond = 0.4f;
            ModelInstance.Initialize();
            ModelInstance.SetAnimation(AnimationIdle);

            HeadBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_Head");
            HandBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_RightHand");

            SimpleModelEffect = this.Content.Load<Effect>("SimpleModelEffect");
            SkinnedModelEffect = this.Content.Load<Effect>("SkinnedModelEffect");

            var Position = new Vector3(500, 200, 0);
            var Direction = new Vector3(0, 200, 0);
            ViewMatrix = Matrix.CreateLookAt(Position, Direction, Vector3.Up);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 5000f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1) && ModelInstance.Animation != AnimationIdle)
            {
                ModelInstance.SetAnimation(AnimationIdle, gameTime);
                HeadBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_Head");
                HandBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_RightHand");
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D2) && ModelInstance.Animation != AnimationRunning)
            {
                ModelInstance.SetAnimation(AnimationRunning, gameTime);
                HeadBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_Head");
                HandBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_RightHand");
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D3) && ModelInstance.Animation != AnimationWalk)
            {
                ModelInstance.SetAnimation(AnimationWalk, gameTime);
                HeadBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_Head");
                HandBoneAnimationInstance = ModelInstance.GetBoneAnimationInstance("soldier_RightHand");
            }

            var HeadYRotationFrame = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                HeadYRotation = 1;
            }
            else if(Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                HeadYRotation = -1;
            }
            else
            {
                if(HeadYRotation < -0.01)
                {
                    HeadYRotation += 1;
                }
                else if (HeadYRotation > 0.01)
                {
                    HeadYRotation = -1;
                }
            }

            HeadYRotation += HeadYRotationFrame * 1.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 2.5f);
            HeadYRotation = MathHelper.Clamp(HeadYRotation, -1.1f,1.1f);
            HeadBoneAnimationInstance.AdditionalTransform = Matrix.CreateRotationY(HeadYRotation);

            ModelInstance.Transformation = Matrix.CreateRotationY(2.5f);
            ModelInstance.UpdateBoneAnimations(gameTime);
            ModelInstance.UpdateBones(gameTime);
            RifleTransformation = Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(10, 5, -4f) * Matrix.CreateRotationY(3.14f) * Matrix.CreateRotationX(3.14f / 2f) * ModelInstance.GetTransform(HandBoneAnimationInstance, gameTime) * ModelInstance.Transformation;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            SkinnedModelEffect.Parameters["Texture1"].SetValue(CharacterTexture);
            DrawSkinnedModel(ModelInstance, gameTime);

            SimpleModelEffect.Parameters["Texture1"].SetValue(RifleTexture);
            DrawSimpleModel(ModelRifle, RifleTransformation, gameTime);

            base.Draw(gameTime);
        }

        void DrawSimpleModel(SimpleModel simpleModel, Matrix transformation, GameTime gameTime)
        {
            SimpleModelEffect.Parameters["World"].SetValue(transformation);
            SimpleModelEffect.Parameters["WorldViewProjection"].SetValue(transformation * ViewMatrix * ProjectionMatrix);

            GraphicsDevice.SetVertexBuffer(simpleModel.VertexBuffer);
            GraphicsDevice.Indices = simpleModel.IndexBuffer;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (EffectPass pass in SimpleModelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, 0, 0, simpleModel.FaceCount);
            }
        }

        void DrawSkinnedModel(SkinnedModelInstance skinnedModelInstance, GameTime gameTime)
        {
            SkinnedModelEffect.Parameters["SunOrientation"].SetValue(Vector3.Normalize(new Vector3(3, 5, 2)));
            SkinnedModelEffect.Parameters["World"].SetValue(skinnedModelInstance.Transformation);
            SkinnedModelEffect.Parameters["WorldViewProjection"].SetValue(skinnedModelInstance.Transformation * ViewMatrix * ProjectionMatrix);

            foreach (var meshInstance in skinnedModelInstance.MeshInstances)
            {
                SkinnedModelEffect.Parameters["gBonesOffsets"].SetValue(meshInstance.BonesOffsets);
                SkinnedModelEffect.Parameters["Texture1"].SetValue(meshInstance.Mesh.Texture);

                GraphicsDevice.SetVertexBuffer(meshInstance.Mesh.VertexBuffer);
                GraphicsDevice.Indices = meshInstance.Mesh.IndexBuffer;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                foreach (EffectPass pass in SkinnedModelEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshInstance.Mesh.FaceCount);
                }
            }
        }
    }
}
