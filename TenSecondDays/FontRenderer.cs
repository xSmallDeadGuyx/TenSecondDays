﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TenSecondDays {
	public class FontRenderer {
		public FontRenderer(FontFile fontFile, Texture2D fontTexture, SpriteBatch spriteBatch) {
			_fontFile = fontFile;
			_texture = fontTexture;
			_spriteBatch = spriteBatch;
			_characterMap = new Dictionary<char, FontChar>();

			foreach(var fontCharacter in _fontFile.Chars) {
				char c = (char) fontCharacter.ID;
				_characterMap.Add(c, fontCharacter);
			}
		}

		private SpriteBatch _spriteBatch;
		private Dictionary<char, FontChar> _characterMap;
		private FontFile _fontFile;
		private Texture2D _texture;
		public void DrawText(int x, int y, string text, Color col) {
			int dx = x;
			int dy = y;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc)) {
					var sourceRectangle = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
					var position = new Vector2(dx + fc.XOffset, dy + fc.YOffset);

					_spriteBatch.Draw(_texture, position, sourceRectangle, col);
					dx += fc.XAdvance;
				}
			}
		}

		public void DrawCenteredText(int x, int y, int maxWidth, string text, Color col) {
			List<string> lines = new List<string>();
			int start = 0;
			int width = 0;
			int lastSpace = 0;
			int lineHeight = GetMaxHeight(text);
			for(int i = 0; i < text.Length; ++i) {
				char c = text[i];
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc)) {
					width += fc.Width;
					if(width >= maxWidth && c == ' ') {
						string line = text.Substring(start, lastSpace - start);
						lines.Add(line);
						i = start = lastSpace;
						width = 0;
					}
					else if(i == text.Length - 1) {
						string line = text.Substring(start, i + 1 - start);
						lines.Add(line);
					}
				}

				if(c == ' ')
					lastSpace = i;
			}

			for(int i = 0; i < lines.Count; ++i) {
				string line = lines[i];
				int lineY = y - (int) (lineHeight * (lines.Count / 2f - i));
				int lineX = x - GetTextWidth(line) / 2;
				DrawText(lineX, lineY, line, col);
			}
		}

		public int GetTextWidth(string text) {
			int dx = 0;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc))
					dx += fc.XAdvance;
			}
			return dx;
		}

		public int GetMaxHeight(string text) {
			int max = 0;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc)) {
					if(fc.Height > max)
						max = fc.Height;
				}
			}
			return max;
		}
	}
}