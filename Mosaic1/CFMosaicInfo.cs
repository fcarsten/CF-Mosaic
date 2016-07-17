/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Drawing;

namespace org.carsten
{
	/// <summary>
	/// 
	/// </summary>
	public class CFMosaicInfo 
	{
		private Bitmap image;
		private int tileWidth=0;
		private int tileHeight=0;
		private int repeatRate=0;
		private int tilesPerRow=0;
		private int tilesPerColumn=0;

		public Bitmap getImage() 
		{
			return image;
		}

		public DBEntry[,] getTiles() 
		{
			return tiles;
		}

		public int getTilesPerRow() 
		{
			return tilesPerRow;
		}

		public int getTilesPerColumn() 
		{
			return tilesPerColumn;
		}

		public int getTileWidth() 
		{
			return tileWidth;
		}

		public int getTileHeight() 
		{
			return tileHeight;
		}

		private DBEntry[,] tiles=null;

		public int getRepeatRate() 
		{
			return repeatRate;
		}

		public void setRepeatRate(int r) 
		{
			repeatRate = r;
		}

		public void setImage(Bitmap img) 
		{
			this.image=img;
		}

		public void Dispose()
		{
			image.Dispose();
			tiles= null;
		}
	

		public CFMosaicInfo(Image img, int mosaicW, int mosaicH, int repRate)
		{
			image= new Bitmap(img);
			tileWidth= mosaicW;
			tileHeight= mosaicH;
			repeatRate= repRate;

			int width = image.Width;
			int height = image.Height;

			tilesPerRow = (int)(width/tileWidth);
			tilesPerColumn = (int)(height/tileHeight);
			if(width%tileWidth != 0)
				tilesPerRow++;
  
			if(height%tileHeight != 0)
				tilesPerColumn++;
  
			tiles = new DBEntry[tilesPerColumn, tilesPerRow];
		}
	}
}
