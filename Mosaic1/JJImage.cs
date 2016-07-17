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
	/// Summary description for JJImage.
	/// </summary>
	public abstract class JJImage
	{

		public abstract byte[] GetPixel(int x, int y, byte[] pixel); 

		public abstract void SetPixel(int x, int y, byte[] pixel);

		public abstract void Dispose();
		public abstract Bitmap getBitmap();

		public byte[] GetPixel(int x, int y) 
		{
			return GetPixel(x,y, new byte[4]);
		}

		public static Bitmap FixedSizeCopy(Image imgPhoto, int Width, int Height, bool disposeOld, InterpolationMode interpolMode = InterpolationMode.HighQualityBicubic)
		{
			int sourceWidth = imgPhoto.Width;
			int sourceHeight = imgPhoto.Height;

			Bitmap bmPhoto = new Bitmap(Width, Height, 
				PixelFormat.Format32bppArgb);
			bmPhoto.SetResolution(imgPhoto.HorizontalResolution, 
				imgPhoto.VerticalResolution);

			Graphics grPhoto = Graphics.FromImage(bmPhoto);
			grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode = interpolMode;
//                InterpolationMode.NearestNeighbor;// HighQualityBicubic;

			grPhoto.DrawImage(imgPhoto, 
				new Rectangle(0,0,Width,Height),
				new Rectangle(0,0,sourceWidth,sourceHeight),
				GraphicsUnit.Pixel);

			grPhoto.Dispose();
			if(disposeOld) 
			{
				imgPhoto.Dispose();
			}
			return bmPhoto;
		}

		public static float[] RGBtoYIQ(Color color)
		{
			float[] yiq =new float[3];
			float[] rgb = {color.R/256.0f, color.G/256.0f, color.B/256.0f};

			yiq[0] = (0.299f * rgb[0]) + (0.587f * rgb[1]) + (0.114f * rgb[2]);
			yiq[1] = (0.596f * rgb[0]) - (0.274f * rgb[1]) - (0.322f * rgb[2]);
			yiq[2] = (0.212f * rgb[0]) - (0.523f * rgb[1]) + (0.311f * rgb[2]);
			return yiq;
		}
		
		public static float[] RGBtoRGB(Color color)
		{
			float[] rgb = {color.R/255.0f, color.G/255.0f, color.B/255.0f};
			return rgb;
		}

		public static float[] RGBtoYIQ(byte[] color)
		{
			float[] yiq =new float[3];
			float[] rgb = {color[2]/256.0f, color[1]/256.0f, color[0]/256.0f};

			yiq[0] = (0.299f * rgb[0]) + (0.587f * rgb[1]) + (0.114f * rgb[2]);
			yiq[1] = (0.596f * rgb[0]) - (0.274f * rgb[1]) - (0.322f * rgb[2]);
			yiq[2] = (0.212f * rgb[0]) - (0.523f * rgb[1]) + (0.311f * rgb[2]);
			return yiq;
		}
		public static float[] RGBtoRGB(byte[] color)
		{
			float[] rgb = {color[2]/255.0f, color[1]/255.0f, color[0]/255.0f};
			return rgb;
		}

		public static float[] YIQtoRGB(float[] yiq)
		{
			float[] rgb =new float[3];
  
			rgb[0] =  (yiq[0] + (0.956f * yiq[1]) + (0.621f * yiq[2]));
			rgb[1] =  (yiq[0] - (0.272f * yiq[1]) - (0.647f * yiq[2]));
			rgb[2] =  (yiq[0] - (1.105f * yiq[1]) + (1.702f * yiq[2]));

			if ((rgb[0]>1.0) || (rgb[0]<0.0) ||
				(rgb[1]>1.0) || (rgb[1]<0.0) ||
				(rgb[2]>1.0) || (rgb[2]<0.0))
			{
				rgb[0] = rgb[1] = rgb[2] = 0.0f;
			}
			return rgb;
		}



	}
}
