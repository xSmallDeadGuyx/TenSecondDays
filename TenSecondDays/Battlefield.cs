using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace TenSecondDays {
	public class Enemy {
		public Vector2 position;
		public Vector2 velocity;
		public Vector2 acceleration;
		public int shootTimer;
		public int maxX;

		public Enemy(Vector2 p, Vector2 v, int s) {
			position = p;
			velocity = v;
			shootTimer = s;

			maxX = 180 + (int) (Battlefield.rand.NextDouble() * 20);
		}
	}
	public class PlayerBullet {
		public Vector2 position;
		public float angle;
		public Color colour;

		public int length;
		public bool hit = false;
		public Enemy enemyHit;

		public int timer = 0;

		public PlayerBullet(Vector2 p, float a, Color c, List<Enemy> enemies) {
			position = p;
			angle = a;
			colour = c;

			int l = 0;
			Vector2 pos = p;
			while(!hit && pos.X >= 0 && pos.X < 368 && pos.Y >= 0 && pos.Y < 128) {
				for(int i = 0; i < enemies.Count; ++i) {
					Vector2 dif = enemies[i].position + new Vector2(6, 6) - pos;
					if(dif.Length() <= 6) {
						hit = true;
						enemyHit = enemies[i];
						length = l * 5;
						break;
					}
				}
				pos += new Vector2(5f * (float) Math.Cos(angle), 5f * (float) Math.Sin(angle));
				++l;
			}
			if(!hit)
				length = l * 5 + 5;
		}
	}
	public class EnemyBullet {
		public Vector2 position;
		public int timer = 0;

		public EnemyBullet(Vector2 p) {
			position = p;
		}
	}
	public class EnemyImploding {
		public Vector2 position;
		public int implodeTime = 0;

		public EnemyImploding(Vector2 p) {
			position = p;
		}
	}
	public class Battlefield {
		public static SpriteBatch spriteBatch;

		private static Texture2D backgroundTexture;
		private static Texture2D sandbagsTexture;
		private static Texture2D playerTexture;
		private static Texture2D gunTexture;
		public static Texture2D bulletTexture;
		private static Texture2D enemyBaseTexture;
		private static Texture2D enemyGunTexture;
		private static Texture2D enemyImplodingTexture;
		private static Texture2D sunTexture;
		private static Texture2D overlayTexture;

		public static FontRenderer fontRenderer;
		private static SoundEffectInstance shoot;

		public static Random rand = new Random();

		List<Enemy> enemies = new List<Enemy>();
		List<EnemyImploding> implodingEnemies = new List<EnemyImploding>();

		List<PlayerBullet> playerBullets = new List<PlayerBullet>();
		List<EnemyBullet> enemyBullets = new List<EnemyBullet>();

		string[] story = {
			"\"What? Where am I?\"",
			"\"I guess I'm not alone\"",
			"\"How can I get out of here?\"",
			"\"Not much to do but try to survive\"",
			"\"How much longer can this go on?\"",
			"\"I'm exhausted, no food, little sleep\"",
			"\"I hope help comes soon\"",
			"\"Dear lord, if you can hear this...\"",
			"\"The waves are far more intense now...\"",
			"\"I can't take much more of this!!!!\"",
			"\"They're everywhere, I can't escape!\"",
			"\"Maybe if I let them get me, they'll set me free\"",
			"And with one final act, he gave up hope..."
		};

		int sandbagsHealth = 100;
		int timeSurvived = -600;
		int loseTimer = 0;

		int enemyCount = 8;
		float velocity = 1;

		double gunRotation;

		MouseState mouse;
		MouseState prevMouseState;

		public static void loadContent(ContentManager content) {
			backgroundTexture = content.Load<Texture2D>("background");
			sandbagsTexture = content.Load<Texture2D>("sandbags");
			playerTexture = content.Load<Texture2D>("player_still");
			gunTexture = content.Load<Texture2D>("gun");
			bulletTexture = content.Load<Texture2D>("bullet");
			enemyBaseTexture = content.Load<Texture2D>("enemy_base");
			enemyGunTexture = content.Load<Texture2D>("enemy_gun");
			enemyImplodingTexture = content.Load<Texture2D>("enemy_implode");
			sunTexture = content.Load<Texture2D>("sun");
			overlayTexture = content.Load<Texture2D>("overlay");

			fontRenderer = new FontRenderer(FontLoader.Load(Path.Combine(content.RootDirectory, "font.fnt")), content.Load<Texture2D>("font_0"), spriteBatch);
			SoundEffect shootEffect = content.Load<SoundEffect>("shoot");
			shoot = shootEffect.CreateInstance();
			shoot.Volume = 0.2f;
		}

		public void update() {
			if(sandbagsHealth <= 0) {
				loseTimer++;
				return;
			}

			timeSurvived++;
			if(timeSurvived >= 14400)
				return;
			if(timeSurvived % 1200 == 0) {
				enemyCount += 2;
				if(timeSurvived % 3600 == 0)
					enemyCount++;
				if(timeSurvived % 4800 == 0)
					velocity += 0.3f;
			}
			else if(timeSurvived % 600 == 0) {
				implodingEnemies.Clear();
				foreach(Enemy e in enemies)
					implodingEnemies.Add(new EnemyImploding(e.position));
				enemies.Clear();
			}

			if(timeSurvived >= 0 && timeSurvived % 1200 < 450 && timeSurvived % (450 / enemyCount) == 0)
				enemies.Add(new Enemy(new Vector2(-100, (int) (108 * rand.NextDouble())), velocity * Vector2.UnitX, 0));

			prevMouseState = mouse;
			mouse = Mouse.GetState();

			gunRotation = Math.Atan2(106 - mouse.Y / 2, 288 - mouse.X / 2);
			if(gunRotation > 1)
				gunRotation = 1;
			if(gunRotation < -0.1)
				gunRotation = -0.1;

			if(prevMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed) {
				PlayerBullet b = new PlayerBullet(new Vector2(288f, 106f), (float) (Math.PI + gunRotation), Color.White, enemies);
				playerBullets.Add(b);

				shoot.Play();
				if(b.hit) {
					implodingEnemies.Add(new EnemyImploding(b.enemyHit.position));
					enemies.Remove(b.enemyHit);
				}
			}

			for(int i = 0; i < implodingEnemies.Count; ++i) {
				++implodingEnemies[i].implodeTime;
				if(implodingEnemies[i].implodeTime > 15) {
					implodingEnemies.Remove(implodingEnemies[i]);
					--i;
				}
			}

			for(int i = 0; i < enemies.Count; ++i) {
				if(enemies[i].position.Y <= 5 && enemies[i].velocity.Y < 0)
					enemies[i].velocity.Y = -enemies[i].velocity.Y;
				else if(enemies[i].position.Y >= 108 && enemies[i].velocity.Y > 0)
					enemies[i].velocity.Y = -enemies[i].velocity.Y;
				else if(enemies[i].position.X >= enemies[i].maxX) {
					enemies[i].velocity = Vector2.Zero;
					enemies[i].shootTimer++;
					if(enemies[i].shootTimer > 31) {
						enemies[i].shootTimer = 0;
						enemyBullets.Add(new EnemyBullet(enemies[i].position + new Vector2(6, 6)));
						sandbagsHealth--;

						shoot.Play();
					}
				}
				else {
					double direction = Math.Atan2(enemies[i].acceleration.Y, enemies[i].acceleration.X);
					double newDirection = direction - 0.1 + rand.NextDouble() * 0.2;
					double newLength = -0.1 + rand.NextDouble() * 0.2;
					enemies[i].acceleration = new Vector2((float) (newLength * Math.Cos(newDirection)), (float) (newLength * Math.Sin(newDirection)));

					enemies[i].velocity += enemies[i].acceleration;

					if(enemies[i].velocity.X < 0)
						enemies[i].velocity.X = -enemies[i].velocity.X;
				}

				enemies[i].position += enemies[i].velocity;
			}

			for(int i = 0; i < playerBullets.Count; ++i) {
				if(playerBullets[i].timer > 3) {
					playerBullets.Remove(playerBullets[i]);
					--i;
					continue;
				}
				playerBullets[i].timer++;
			}

			for(int i = 0; i < enemyBullets.Count; ++i) {
				if(enemyBullets[i].timer > 3) {
					enemyBullets.Remove(enemyBullets[i]);
					--i;
					continue;
				}
				enemyBullets[i].timer++;
			}
		}

		public void draw() {
			if(timeSurvived >= 14400) {
				spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, 368, 128), Color.Black);
				fontRenderer.DrawCenteredText(184, 64, 250, "You win?", Color.White);
				return;
			}
			spriteBatch.Draw(sunTexture, new Rectangle(184, 112, 32, 32), new Rectangle(0, 0, 32, 32), Color.White, (float) Math.PI * timeSurvived / 600f, new Vector2(168, 0), SpriteEffects.None, 0);
			spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height), Color.White);
			if(sandbagsHealth > 0)
				spriteBatch.Draw(sandbagsTexture, new Rectangle(246, 108, 29, 12), new Rectangle(((sandbagsHealth - 1) / 25) * 29, 0, 29, 12), Color.White);
			spriteBatch.Draw(playerTexture, new Rectangle(276, 90, 20, 32), Color.White);

			foreach(PlayerBullet b in playerBullets)
				spriteBatch.Draw(bulletTexture, new Rectangle((int) b.position.X, (int) b.position.Y, b.length, 2), new Rectangle(0, 0, 6, 2), b.colour, b.angle, new Vector2(0f, 0f), SpriteEffects.None, 0);

			foreach(EnemyBullet b in enemyBullets)
				spriteBatch.Draw(bulletTexture, new Rectangle((int) b.position.X, (int) b.position.Y, (int) (b.position - new Vector2(260, 114)).Length(), 2), new Rectangle(0, 0, 6, 2), Color.Red, (float) Math.Atan2(114 - b.position.Y, 260 - b.position.X), new Vector2(0f, 0f), SpriteEffects.None, 0);

			spriteBatch.Draw(gunTexture, new Rectangle(288, 106, 16, 10), new Rectangle(0, 0, 16, 10), Color.White, (float) gunRotation, new Vector2(10, 3), SpriteEffects.None, 0);

			foreach(EnemyImploding e in implodingEnemies)
				spriteBatch.Draw(enemyImplodingTexture, new Rectangle((int) e.position.X, (int) e.position.Y, 12, 12), new Rectangle(e.implodeTime / 4 * 12, 0, 12, 12), Color.White);

			foreach(Enemy e in enemies) {
				spriteBatch.Draw(enemyBaseTexture, e.position, Color.White);
				spriteBatch.Draw(enemyGunTexture, new Rectangle((int) e.position.X + 6, (int) e.position.Y + 6, 5, 2), new Rectangle(0, 0, 5, 2), Color.White, (float) Math.Atan2(108 - e.position.Y, 254 - e.position.X), new Vector2(0, 1), SpriteEffects.None, 0);
			}

			spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, 368, 128), new Color(0f, 0f, 0f, timeSurvived % 1200 < 600 ? (float) Math.Abs(300 - timeSurvived % 600) / 600f : 1f - (float) Math.Abs(300 - timeSurvived % 600) / 600f));

			if(timeSurvived < 0 || timeSurvived % 1200 >= 600) {
				string storyPart = story[(timeSurvived + 600) / 1200];
				fontRenderer.DrawCenteredText(184, 64, 250, storyPart, new Color(1f, 1f, 1f, timeSurvived % 1200 < 600 ? (float) Math.Abs(300 - timeSurvived % 600) / 600f : 1f - (float) Math.Abs(300 - timeSurvived % 600) / 600f));
			}

			if(sandbagsHealth <= 0) {
				spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, 368, 128), new Color(1f, 0f, 0f, Math.Min(1f, loseTimer / 60f)));
				fontRenderer.DrawCenteredText(184, 64, 250, "You lose...", new Color(1f, 1f, 1f, timeSurvived % 1200 < 600 ? (float) Math.Abs(300 - timeSurvived % 600) / 600f : 1f - (float) Math.Abs(300 - timeSurvived % 600) / 600f));
			}
		}
	}
}
