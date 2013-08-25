#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace TenSecondDays {
	public class TenSecondDays : Game {
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Battlefield battlefield;
		KeyboardState prevKeyboard;

		SoundEffectInstance music;

		private Texture2D cursorTexture;

		public TenSecondDays() : base() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			IsMouseVisible = false;

			graphics.PreferredBackBufferWidth = 736;
			graphics.PreferredBackBufferHeight = 256;

			graphics.GraphicsDevice.Viewport = new Viewport(0, 0, 368, 128);
		}

		protected override void Initialize() {
			base.Initialize();
		}

		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Battlefield.spriteBatch = spriteBatch;

			cursorTexture = Content.Load<Texture2D>("cursor");

			SoundEffect musicEffect = Content.Load<SoundEffect>("music");
			music = musicEffect.CreateInstance();
			music.Volume = 1f;
			music.IsLooped = true;

			Battlefield.loadContent(Content);
		}

		protected override void UnloadContent() {
		}

		protected override void Update(GameTime gameTime) {
			if(music.State != SoundState.Playing)
				music.Play();

			if(prevKeyboard != null && Keyboard.GetState().IsKeyDown(Keys.Escape) && prevKeyboard.IsKeyUp(Keys.Escape))
				if(battlefield == null)
					Exit();
				else
					battlefield = null;

			MouseState mouse = Mouse.GetState();

			if(battlefield != null)
				battlefield.update();
			else if(mouse.X / 2 >= 24 && mouse.Y / 2 >= 24 && mouse.X / 2 < 344 && mouse.Y / 2 < 104 && mouse.LeftButton == ButtonState.Pressed)
				battlefield = new Battlefield();

			prevKeyboard = Keyboard.GetState();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			SpriteBatch targetBatch = new SpriteBatch(GraphicsDevice);
			RenderTarget2D target = new RenderTarget2D(GraphicsDevice, 368, 128);
			GraphicsDevice.SetRenderTarget(target);

			GraphicsDevice.Clear(new Color(156, 205, 207));

			spriteBatch.Begin();
			
			MouseState mouse = Mouse.GetState();

			if(battlefield != null)
				battlefield.draw();
			else {
				if(mouse.X / 2 >= 24 && mouse.Y / 2 >= 24 && mouse.X / 2 < 344 && mouse.Y / 2 < 104)
					spriteBatch.Draw(Battlefield.bulletTexture, new Rectangle(24, 24, 320, 80), Color.Gray);
				else
					spriteBatch.Draw(Battlefield.bulletTexture, new Rectangle(24, 24, 320, 80), Color.DarkGray);

				Battlefield.fontRenderer.DrawCenteredText(184, 64, 250, "Begin", Color.White);
			}

			spriteBatch.Draw(cursorTexture, new Vector2(mouse.X / 2 - 7, mouse.Y / 2 - 7), Color.White);

			spriteBatch.End();

			GraphicsDevice.SetRenderTarget(null);

			targetBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			targetBatch.Draw(target, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
			targetBatch.End();

			base.Draw(gameTime);
		}
	}
}
