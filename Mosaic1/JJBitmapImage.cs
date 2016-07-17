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
	/// Summary description for JJBitmapImage.
	/// </summary>
	public unsafe class JJBitmapImage : JJImage
	{
		private Bitmap image = null;
		private BitmapData bitmapData = null;
		private byte *pixelPtr=null;
int bpp=0; // byte per pixel

		public override byte[] GetPixel(int x, int y, byte[] pixel) 
		{
			if(bitmapData==null)
				LockImage();

			unsafe 
			{
				byte *ptr = pixelPtr + y*bitmapData.Stride + x*bpp;
				for(int i=0; i< bpp; i++) 
				{
					pixel[i]= *(ptr+i);
				}
				for(int i=bpp; i< pixel.Length; i++) 
				{
					pixel[i]= 255;
				}
			}
			return pixel;
		}

		public override void SetPixel(int x, int y, byte[] pixel) 
		{
			if(bitmapData==null)
				LockImage();

			unsafe 
			{
				byte *ptr = pixelPtr + y*bitmapData.Stride + x*bpp;
				for(int i=0; i< bpp; i++) 
				{
					*(ptr+i) = i>=pixel.Length? (byte)255: pixel[i];
				}			
			}
		}

		public JJBitmapImage(Bitmap bitmap) 
		{
			if(bitmap.PixelFormat == PixelFormat.Format32bppArgb)
			{
				bpp=4;
			} 
			else if(bitmap.PixelFormat == PixelFormat.Format24bppRgb)
			{
				bpp=3;
			} 
			else 
			{
				throw new Exception("Only 32 bit ARGB images supported");
			}

			image = bitmap;
			bitmapData = null;
			pixelPtr=null;
		}

		public static Bitmap getThumbNail(string fileName, int width, int height) 
		{
			Bitmap res= null;
			try 
			{
				EXIFThumbnailExtractor nase = new EXIFThumbnailExtractor(null);
				nase.DecodeExif(fileName);
				byte[] thumbNail = nase.getThumbnail();

				if(thumbNail != null) 
				{
					System.IO.MemoryStream stream = new System.IO.MemoryStream(thumbNail, false);
					res= new Bitmap(stream);
					ExifInfo exifInfo = nase.getExifInfo();

					float imgRatio = exifInfo.Width / (float) exifInfo.Height;
					float thumbRatio = res.Width / (float)res.Height;
					float ratioDiff =thumbRatio-imgRatio ;
					
					if(ratioDiff<-0.001) 
					{
						int resTargetHeight=(int)( res.Width / imgRatio);
						int off= (res.Height- resTargetHeight)/2;
						res = cropImage(res,0,off,res.Width,resTargetHeight, true).getBitmap();
					} 
					else if(ratioDiff>0.001) 
					{
						int resTargetWidth= (int)(res.Height* imgRatio);
						int off= (res.Width- resTargetWidth)/2;
						res = cropImage(res,off,0,resTargetWidth, res.Height, true).getBitmap();
					}
				} 
				else 
				{
					try 
					{
						res= new Bitmap(fileName);
					} 
					catch (System.ArgumentException e) 
					{
						Console.WriteLine("Could not load image {0} because: {1}", fileName, e.Message);
					}
				}
			} 
			catch (System.IO.IOException e)
			{
				Console.WriteLine("Could not load image " + fileName+ ": " + e.Message);
			}
			if(res != null)
			{
				if(res.Width<res.Height)
				{
					res.RotateFlip(RotateFlipType.Rotate90FlipNone);
				}
				if(res.Width!=width || res.Height!=height)
				{
					res = FixedSizeCopy(res, width, height, true);
				}
			}
			return res;
		}

		public void LockImage(ImageLockMode mode) 
		{
			bitmapData = image.LockBits( new Rectangle( 0 , 0 , image.Width , image.Height ) , 
				mode  , image.PixelFormat);
			pixelPtr = (byte *)bitmapData.Scan0;
		}

		public override Bitmap getBitmap()
		{
			return image;
		}

		public void LockImage() 
		{
			if(image == null) throw new Exception("Can't lock empty image");
			LockImage(ImageLockMode.ReadWrite);
		}

		public override void Dispose() 
		{
			if(image==null)return;
			UnlockImage();
			image.Dispose();
		}

		public void UnlockImage() 
		{
			if(bitmapData!=null)
				image.UnlockBits(bitmapData);
			bitmapData=null;
			pixelPtr=null;
		}
		public static JJImage cropImage(Bitmap img, int x, int y, int w, int h, bool disposeOld) 
		{
			//			JJBitmapImage result = new JJBitmapImage(w,h);
			JJBitmapImage result = new JJBitmapImage(new Bitmap(w,h,PixelFormat.Format32bppArgb));
			JJBitmapImage tmpImg= new JJBitmapImage(img);
			byte[] pixel= new byte[4];
			for(int i=0; i<w; i++)
			{
				for(int j=0; j<h; j++)
				{
					tmpImg.GetPixel(x+i, y+j, pixel);
					result.SetPixel(i,j,pixel);
				}
			}
			result.UnlockImage();
			tmpImg.UnlockImage();

			//			Bitmap result = new Bitmap(w,h,PixelFormat.Format32bppArgb);// (img, new Size(w,h));
			//			Graphics grPhoto = Graphics.FromImage(result);
			//			grPhoto.DrawImageUnscaled(img, -x, -y, w, h);
			if(disposeOld)
				tmpImg.Dispose();
			return result;
		}


	}
}
