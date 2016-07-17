/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace org.carsten
{
	/// <summary>
	/// Summary description for JJRasterImage.
	/// </summary>
	public class JJRasterImage : JJImage
	{
		byte[,,] raster=null;
		public JJRasterImage(int w, int h)
		{
			raster=new byte[w,h,4];
		}

		public override byte[] GetPixel(int x, int y, byte[] pixel)
		{
			if(pixel==null)
			{
				pixel= new byte[4];
			}

			for(int i=0; i<4; i++)
			{
				pixel[i] = raster[x,y,i];
			}
			return pixel;
		}

		public override void SetPixel(int x, int y, byte[] pixel)
		{
			for(int i=0; i<4; i++)
			{
				raster[x,y,i]= pixel[i];
			}
		}

		private Bitmap bitmap= null;

		public override Bitmap getBitmap()
		{
			if(bitmap == null)
			{
				unsafe 
				{
					int w= raster.GetLength(0);
					int h= raster.GetLength(1);
					bitmap= new Bitmap(w, h, PixelFormat.Format32bppArgb);
					BitmapData data = bitmap.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
					for(int i=0; i<w; i++) 
					{
						for(int j=0; j<h; j++)
						{
							for(int k=0; k<4; k++)
							{
								*( ((byte *)data.Scan0)+i*data.Stride+4*j+k) = raster[i,j,k];
							}
						}
					}
					bitmap.UnlockBits(data);
				}
			}
			return bitmap;
		}

		public override void Dispose()
		{
			if(bitmap!=null)
				bitmap.Dispose();
			raster=null;
		}

	}
}
