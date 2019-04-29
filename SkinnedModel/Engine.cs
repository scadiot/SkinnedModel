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

        public Effect MainEffect { get; set; }
        public Effect SkinnedEffect { get; set; }

        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public float headYRotation = 0;

        Model modelRifle;

        SkinnedModelInstance.NodeInstance HeadNodeInstance { get; set; }
        SkinnedModelInstance.NodeInstance HandNodeInstance { get; set; }
        Matrix RifleTransformation;

        Texture2D Maintexture { get; set; }
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
            Maintexture = Content.Load<Texture2D>("Soldier_head_diffuse");// Soldier_Body_diffuse");

            CharacterMesh = new SkinnedModel(GraphicsDevice);
            CharacterMesh.FilePath = @"Content\swat.dae";
            CharacterMesh.Initialize();
            foreach(var mesh in CharacterMesh.Meshes)
            {
                if(mesh.TextureFilePath.Contains("Body"))
                {
                    mesh.Texture = Content.Load<Texture2D>("Soldier_Body_diffuse");
                }
                else
                {
                    mesh.Texture = Content.Load<Texture2D>("Soldier_head_diffuse");
                }
            }

            AnimationIdle = new SkinnedModelAnimation();
            AnimationIdle.FilePath = @"Content\Rifle Idle.dae";
            AnimationIdle.Load();

            AnimationRunning = new SkinnedModelAnimation();
            AnimationRunning.FilePath = @"Content\Running.dae";
            AnimationRunning.Load();

            AnimationWalk = new SkinnedModelAnimation();
            AnimationWalk.FilePath = @"Content\Walking.dae";
            AnimationWalk.Load();
            

            modelRifle = new Model(this);
            modelRifle.FilePath = @"Content\mp5.3ds";
            modelRifle.Initialize();

            ModelInstance = new SkinnedModelInstance();
            ModelInstance.Mesh = CharacterMesh;
            ModelInstance.SpeedTransitionSecond = 0.4f;
            ModelInstance.Initialize();
            ModelInstance.SetAnimation(AnimationIdle);

            HeadNodeInstance = ModelInstance.GetNodeInstance("swat_Head");
            HandNodeInstance = ModelInstance.GetNodeInstance("swat_RightHand");

            MainEffect = this.Content.Load<Effect>("BlocEffect");
            SkinnedEffect = this.Content.Load<Effect>("SkinnedEffect");

            Vector3 Position = new Vector3(250, 100, 0);
            Vector3 Direction = new Vector3(0, 100, 0);
            ViewMatrix = Matrix.CreateLookAt(Position, Direction, Vector3.Up);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 5000f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1) && ModelInstance.Animation != AnimationIdle)
            {
                ModelInstance.SetAnimation(AnimationIdle, gameTime);
                HeadNodeInstance = ModelInstance.GetNodeInstance("swat_Head");
                HandNodeInstance = ModelInstance.GetNodeInstance("swat_RightHand");
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D2) && ModelInstance.Animation != AnimationRunning)
            {
                ModelInstance.SetAnimation(AnimationRunning, gameTime);
                HeadNodeInstance = ModelInstance.GetNodeInstance("swat_Head");
                HandNodeInstance = ModelInstance.GetNodeInstance("swat_RightHand");
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D3) && ModelInstance.Animation != AnimationWalk)
            {
                ModelInstance.SetAnimation(AnimationWalk, gameTime);
                HeadNodeInstance = ModelInstance.GetNodeInstance("swat_Head");
                HandNodeInstance = ModelInstance.GetNodeInstance("swat_RightHand");
            }

            
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                headYRotation += 1.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 2.5f);
                if (headYRotation > 1.1f)
                {
                    headYRotation = 1.1f;
                }
            }
            else if(Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                headYRotation -= 1.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 2.5f);
                if (headYRotation < -1.1f)
                {
                    headYRotation = -1.1f;
                }
            }
            else
            {
                if(headYRotation < -0.01)
                {
                    headYRotation += 1.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 2.5f);
                }
                else if (headYRotation > 0.01)
                {
                    headYRotation -= 1.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 2.5f);
                }
                else
                {
                    headYRotation = 0;
                }
            }
            HeadNodeInstance.AdditionalTransform = Matrix.CreateRotationY(headYRotation);
            // TODO: Add your update logic here
            ModelInstance.Transformation = Matrix.CreateRotationY(2.5f);// (float)gameTime.TotalGameTime.TotalSeconds);

            

            ModelInstance.UpdateNodes(gameTime);
            ModelInstance.UpdateBones(gameTime);
            RifleTransformation = Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(10, 5, -4f) * Matrix.CreateRotationY(3.14f) * Matrix.CreateRotationX(3.14f / 2.0f) * ModelInstance.GetTransform(HandNodeInstance, gameTime) * ModelInstance.Transformation;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            SkinnedEffect.Parameters["Texture1"].SetValue(Maintexture);
            DrawSkinnedModel(ModelInstance, gameTime);


            MainEffect.Parameters["Texture1"].SetValue(Maintexture);
            modelRifle.Draw(gameTime, RifleTransformation);

            base.Draw(gameTime);
        }

        void DrawSkinnedModel(SkinnedModelInstance skinnedModelInstance, GameTime gameTime)
        {
            SkinnedEffect.Parameters["SunOrientation"].SetValue(Vector3.Normalize(new Vector3(3, 5, 2)));
            SkinnedEffect.Parameters["World"].SetValue(skinnedModelInstance.Transformation);
            SkinnedEffect.Parameters["WorldViewProjection"].SetValue(skinnedModelInstance.Transformation * ViewMatrix * ProjectionMatrix);

            foreach (var meshInstance in skinnedModelInstance.MeshInstances)
            {
                SkinnedEffect.Parameters["gBonesOffsets"].SetValue(meshInstance.BonesOffsets);
                SkinnedEffect.Parameters["Texture1"].SetValue(meshInstance.Mesh.Texture);

                GraphicsDevice.SetVertexBuffer(meshInstance.Mesh.VertexBuffer);
                GraphicsDevice.Indices = meshInstance.Mesh.IndexBuffer;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                foreach (EffectPass pass in SkinnedEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshInstance.Mesh.FaceCount);
                }
            }
        }
    }
}
