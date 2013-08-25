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

		Texture2D cursorTexture;

		MouseState prevMouseState;

		int menuTime = 0;

		List<Enemy> fakeEnemies = new List<Enemy>();

		public static bool sound = true;

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

			for(int i = 0; i < 10; ++i) {
				float ang = (float) Battlefield.rand.NextDouble() * (float) Math.PI * 2;
				fakeEnemies.Add(new Enemy(new Vector2(178, 26), new Vector2((float) Math.Cos(ang), (float) Math.Sin(ang))));
			}
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
			if(!sound && music.State == SoundState.Playing)
				music.Stop();
			else if(sound && music.State != SoundState.Playing)
				music.Play();

			if(prevKeyboard != null && Keyboard.GetState().IsKeyDown(Keys.Escape) && prevKeyboard.IsKeyUp(Keys.Escape))
				if(battlefield == null)
					Exit();
				else
					battlefield = null;

			MouseState mouse = Mouse.GetState();

			if(battlefield != null)
				battlefield.update();
			else {
				menuTime++;
				if(menuTime >= 1200)
					menuTime = 0;
				if(prevMouseState != null && prevMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed) {
					if(mouse.X / 2 >= 40 && mouse.Y / 2 >= 72 && mouse.X / 2 < 328 && mouse.Y / 2 < 92)
						battlefield = new Battlefield();
					else if(mouse.X / 2 >= 8 && mouse.Y / 2 >= 100 && mouse.X / 2 < 176 && mouse.Y / 2 < 120)
						sound = !sound;
					else if(mouse.X / 2 >= 192 && mouse.Y / 2 >= 100 && mouse.X / 2 < 360 && mouse.Y / 2 < 120)
						Exit();
				}
				else {
					for(int i = 0; i < fakeEnemies.Count; ++i) {
						fakeEnemies[i].position += fakeEnemies[i].velocity;

						if(fakeEnemies[i].position.Y <= 8 && fakeEnemies[i].velocity.Y < 0)
							fakeEnemies[i].velocity.Y = -fakeEnemies[i].velocity.Y;
						else if(fakeEnemies[i].position.Y >= 52 && fakeEnemies[i].velocity.Y > 0)
							fakeEnemies[i].velocity.Y = -fakeEnemies[i].velocity.Y;
						else if(fakeEnemies[i].position.X <= 8 && fakeEnemies[i].velocity.X < 0)
							fakeEnemies[i].velocity.X = -fakeEnemies[i].velocity.X;
						else if(fakeEnemies[i].position.X >= 348 && fakeEnemies[i].velocity.X > 0)
							fakeEnemies[i].velocity.X = -fakeEnemies[i].velocity.X;
						else {
							double direction = Math.Atan2(fakeEnemies[i].acceleration.Y, fakeEnemies[i].acceleration.X);
							double newDirection = direction - 0.1 + Battlefield.rand.NextDouble() * 0.2;
							double newLength = -0.1 + Battlefield.rand.NextDouble() * 0.2;
							fakeEnemies[i].acceleration = new Vector2((float) (newLength * Math.Cos(newDirection)), (float) (newLength * Math.Sin(newDirection)));

							fakeEnemies[i].velocity += fakeEnemies[i].acceleration;
						}
					}
				}
			}

			prevKeyboard = Keyboard.GetState();
			prevMouseState = mouse;
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
				spriteBatch.Draw(Battlefield.sunTexture, new Rectangle(184, 112, 32, 32), new Rectangle(0, 0, 32, 32), Color.White, (float) Math.PI * menuTime / 600f, new Vector2(168, 0), SpriteEffects.None, 0);

				foreach(Enemy f in fakeEnemies) {
					spriteBatch.Draw(Battlefield.enemyBaseTexture, f.position, Color.White);
					spriteBatch.Draw(Battlefield.enemyGunTexture, new Rectangle((int) f.position.X + 6, (int) f.position.Y + 6, 5, 2), new Rectangle(0, 0, 5, 2), Color.White, (float) Math.Atan2(f.velocity.Y, f.velocity.X), new Vector2(0, 1), SpriteEffects.None, 0);
				}

				spriteBatch.Draw(Battlefield.overlayTexture, new Rectangle(0, 0, 368, 128), new Color(0f, 0f, 0f, menuTime < 600 ? (float) Math.Abs(300 - menuTime % 600) / 600f : 1f - (float) Math.Abs(300 - menuTime % 600) / 600f));

				Battlefield.fontRenderer.DrawCenteredText(184, 32, 250, "Ten Second Days", Color.White);
				
				spriteBatch.Draw(Battlefield.bulletTexture, new Rectangle(40, 72, 288, 20), mouse.X / 2 >= 40 && mouse.Y / 2 >= 72 && mouse.X / 2 < 328 && mouse.Y / 2 < 92 ? Color.Gray : Color.DarkGray);
				Battlefield.smallFontRenderer.DrawCenteredText(184, 82, 250, "Click here to begin", Color.Black);

				spriteBatch.Draw(Battlefield.bulletTexture, new Rectangle(8, 100, 168, 20), mouse.X / 2 >= 8 && mouse.Y / 2 >= 100 && mouse.X / 2 < 176 && mouse.Y / 2 < 120 ? Color.Gray : Color.DarkGray);
				Battlefield.smallFontRenderer.DrawCenteredText(88, 110, 168, "Sound: " + (sound ? "On" : "Off"), Color.Black);

				spriteBatch.Draw(Battlefield.bulletTexture, new Rectangle(192, 100, 168, 20), mouse.X / 2 >= 192 && mouse.Y / 2 >= 100 && mouse.X / 2 < 360 && mouse.Y / 2 < 120 ? Color.Gray : Color.DarkGray);
				Battlefield.smallFontRenderer.DrawCenteredText(280, 110, 168, "Press <Esc> to Quit", Color.Black);
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
