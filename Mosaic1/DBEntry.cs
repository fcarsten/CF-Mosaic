/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;


namespace org.carsten
{
	/// <summary>
	/// Summary description for DBEntry.
	/// </summary>
	[Serializable]
	public class DBEntry 
	{
		static readonly double sqrt2 = Math.Sqrt(2);

		private string name;

				static readonly float[] wy= {5.0f,0.83f,1.01f,0.52f,0.47f,0.3f};
				static readonly float[] wi= {19.21f, 1.26f, 0.44f,0.53f,0.28f,0.14f};
				static readonly float[] wq= {34.37f,0.36f,0.45f,0.14f,0.18f,0.27f};
//		static readonly float[] wy= {1.0f};
//		static readonly float[] wi= {1.0f};
//		static readonly float[] wq= {1.0f};

		const int HAAR_WIDTH=16; // must be 2^X
		const int HAAR_HEIGHT=16; // must be 2^X
//		[NonSerialized]
//		float[,] colY = new float[HAAR_WIDTH,HAAR_HEIGHT];
//		[NonSerialized]
//		float[,] colI = new float[HAAR_WIDTH,HAAR_HEIGHT];
//		[NonSerialized]
//		float[,] colQ = new float[HAAR_WIDTH,HAAR_HEIGHT];
 
		private byte[,,] haar = new byte[HAAR_WIDTH,HAAR_HEIGHT,3];

		const int RGB_WIDTH=12; 
		const int RGB_HEIGHT=9;
		private byte[,,] rgb = new byte[RGB_WIDTH,RGB_HEIGHT,3];
        
//    Luminance (standard, objective): (0.2126*R) + (0.7152*G) + (0.0722*B)
//    Luminance (perceived option 1): (0.299*R + 0.587*G + 0.114*B)
//    Luminance (perceived option 2, slower to calculate): sqrt( 0.241*R^2 + 0.691*G^2 + 0.068*B^2 )
   //     static readonly double[] RGB_WEIGHT = {0, 0,1 };
        static readonly double[] RGB_WEIGHT = { 1.0 - 0.241, 1.0 - 0.691, 1.0 - 0.068 }; // Quit good that one!
        //        static readonly double[] RGB_WEIGHT = { 1,1,1};
//        static readonly double[] RGB_WEIGHT = { 0.241, 0.691, 0.068 };

		static int bin(int i, int k)
		{
			return Math.Min(Math.Max(i,k), wy.Length-1);
		}
 
		public void resetImage()
		{
//			image.Dispose();
//			image=null;
		}
 
		public long distanceRGB(DBEntry entry, double currentBest) 
		{
			double diffR=0, diffG=0, diffB=0;
			double res= 0;
			double tmp=0;
			for(int i=0; i<RGB_WIDTH; i++)
			{
				for(int j=0; j<RGB_HEIGHT; j++)
				{
                    tmp =(rgb[i, j, 0] - entry.rgb[i, j, 0]);
					diffB += tmp < 0 ? -tmp : tmp;
					tmp=(rgb[i,j,1] - entry.rgb[i,j,1]);
					diffG += tmp < 0 ? -tmp : tmp;
					tmp= (rgb[i,j,2] - entry.rgb[i,j,2]);
					diffR += tmp < 0 ? -tmp : tmp;
				}
                // res = RGB_WEIGHT[0] * diffR * diffR + RGB_WEIGHT[1] * diffG * diffG + RGB_WEIGHT[2] * diffB * diffB;
                res = 2 * diffR + 4 * diffG + diffB;
                if (res > currentBest)
					return (long)res;
			}

			return (long) res;
		}

		public static float abs(float x) 
		{
			return x<0? x : -x;
		}

		public double distanceHaar(DBEntry entry, double currentBest)
		{
			double error=0;
			double tmp=0;

			for(int i = 0; i<haar.GetLength(0); i++)
			{
				for(int k=0; k<haar.GetLength(1); k++)
				{
					if(i==0 && k==0)
					{
						double tmpErr=0;
						tmpErr = wy[0] * abs(haar[0,0,0] - (float)entry.haar[0,0,0]);
						tmpErr += wi[0] * abs(haar[0,0,1] - (float)entry.haar[0,0,1]);
						tmpErr += wq[0] * abs(haar[0,0,2] - (float)entry.haar[0,0,2]);
						error += tmpErr*tmpErr;
					}
					else
					{
						tmp = 0;
							tmp += wy[bin(i,k)] * abs(haar[i,k,0] - (float)entry.haar[i,k,0]);
							tmp += wi[bin(i,k)] * abs(haar[i,k,1] - (float)entry.haar[i,k,1]);
							tmp += wq[bin(i,k)] * abs(haar[i,k,2] - (float)entry.haar[i,k,2]);
						error += tmp*tmp;
					}
					if(error>currentBest)
						return error;
				}
			}
			return error;
		}

		public DBEntry(String n)
		{
			name = n;  
			Bitmap image= getPlanarImage();
			computeStatistics(image);
		}


		//static int counter=0;
		public Bitmap getPlanarImage() //MosaicWorker mw) 
		{
			Bitmap i = new Bitmap(name);
			
			if(i.Height > i.Width)
			{
				i.RotateFlip(RotateFlipType.Rotate90FlipNone);
			}
  
			//			if((mw.mosaicWidth != i.Width) || 
			//				(mw.mosaicHeight!= i.Height))
			//			{
			//				i = FixedSize(i, (int)mw.mosaicWidth, (int)mw.mosaicHeight);
			//			}
		 
			
			//
			// Coinvert to 24 bit argb. Do we really need this?
			//
			if(i.PixelFormat != PixelFormat.Format32bppArgb) 
			{
				i = JJImage.FixedSizeCopy(i, i.Width, i.Height, true);				
			}

			return i;
		}
 
		public Bitmap getImage()//MosaicWorker mw) 
		{
//			if(image != null)
//				return image;
//
//			image = getPlanarImage();//mw);
//			return image;
			return getPlanarImage();
		}
 
 
		void computeHaar(Bitmap theImage)
		{
			int width = 1;
			while(width*2 < theImage.Width) 
			{
				width *=2;
			}
			int height = 1;
			while(height*2 < theImage.Height) 
			{
				height *=2;
			}

			JJImage img= new JJBitmapImage(JJImage.FixedSizeCopy(theImage, width, height, false));
			try 
			{
				float [,] tmpColY = new float[width, height];
				float [,] tmpColI = new float[width, height];
				float [,] tmpColQ = new float[width, height];
				byte[] pixel= new byte [4];

				for(int i=0;i<width;i++)
				{
					for(int k=0;k<height;k++)
					{
						//					Color pixel= img.GetPixel(i,k);
						img.GetPixel(i,k, pixel);
						float[] yiq = JJImage.RGBtoRGB(pixel);
						tmpColY[i,k] = yiq[0];
						tmpColI[i,k] = yiq[1];
						tmpColQ[i,k] = yiq[2];
					}
				}
				haarDecompose(tmpColY);
				haarDecompose(tmpColI);
				haarDecompose(tmpColQ);
			
				for(int i=0;i<HAAR_WIDTH ;i++)
				{
					for(int k=0;k<HAAR_HEIGHT ;k++)
					{
                        if (i < width && k < height)
                        {
                            haar[i, k, 0] = (byte)(tmpColY[i, k] == 1 ? 255 : 256.0 * tmpColY[i, k]);
                            haar[i, k, 1] = (byte)(tmpColI[i, k] == 1 ? 255 : 256.0 * tmpColI[i, k]);
                            haar[i, k, 2] = (byte)(tmpColQ[i, k] == 1 ? 255 : 256.0 * tmpColQ[i, k]);
                        }
                        else
                        {
                            haar[i, k, 0] = 0;
                            haar[i, k, 1] = 0;
                            haar[i, k, 2] = 0;
                        }
					}
				}
			} 
			finally 
			{
				img.Dispose();
			}
		}
 
 
		void decomposeRow(float[,] img, int row)
		{
			int h= img.GetLength(1);
			for(int i=0; i<h; i++)
			{
				img[row,i] /= (float)Math.Sqrt(h);//4; // Math.sqrt(HAAR_HEIGHT)
			}

			double[] temp = new double[h];
  
			while(h > 1)
			{
				h /= 2;
				for(int i=0; i<h; i++)
				{
					temp[i] = (img[row,2*i]+ img[row,2*i+1])/sqrt2;
					temp[h+i] = (img[row,2*i]-img[row,2*i+1])/sqrt2;
				}

				for(int k=0; k<img.GetLength(1); k++)
				{
					img[row,k] = (float)temp[k];
				}
			}
		}

		void decomposeCol(float[,] img, int col)
		{
			int h= img.GetLength(0);
			double hSqrt = Math.Sqrt(h);
			for(int i=0; i<h; i++)
			{
				img[i,col] /=(float) hSqrt;//4; // Math.sqrt(16)
			}

			double[] temp = new double[h];
  
			while(h > 1)
			{
				h /= 2;
				for(int i=0; i<h; i++)
				{
					temp[i] = (img[2*i,col]+ img[2*i+1,col])/sqrt2;
					temp[h+i] = (img[2*i,col]-img[2*i+1,col])/sqrt2;
				}

				for(int k=0; k<img.GetLength(0); k++)
				{
					img[k,col] = (float)temp[k];
				}
			}
		}
 
		void haarDecompose(float[,] img)
		{
			for(int row = 0; row < img.GetLength(0); row++)
			{
				decomposeRow(img, row);
			}
			for(int col= 0; col< img.GetLength(1); col++)
			{
				decomposeCol(img, col);
			}
  
		}
 
		public DBEntry(Bitmap i)
		{
			name = null;
			computeStatistics(i);
		}

 
		void computeStatistics(Bitmap i)
		{
			computeHaar(i);
			computeRGB(i);
		}
 
		void computeRGB(Bitmap img)
		{
            JJImage image = new JJBitmapImage(JJImage.FixedSizeCopy(img, RGB_WIDTH, RGB_HEIGHT, false, InterpolationMode.NearestNeighbor));

			for(int i=0;i<RGB_WIDTH; i++)
			{
				for(int j=0;j<RGB_HEIGHT;j++) 
				{
					byte[] pixel = image.GetPixel(i,j);
  
					rgb[i,j,0] = pixel[0];
					rgb[i,j,1] = pixel[1];
					rgb[i,j,2] = pixel[2];
				}
			}
			image.Dispose();
		}

		//		public DBEntry (SerializationInfo info, StreamingContext ctxt) 
		//		{
		//			if (info == null)
		//				throw new System.ArgumentNullException("info");
		//			name = (string)info.GetValue("name", typeof(string));
		//			colI = (float[,]) info.GetValue("colI", typeof(float[,]));
		//			colQ = (float[,]) info.GetValue("colQ", typeof(float[,]));
		//			colY = (float[,]) info.GetValue("colY", typeof(float[,]));
		//		}
		//		#region ISerializable Members
		//
		//		public void GetObjectData(SerializationInfo info, StreamingContext context)
		//		{
		//			info.AddValue("colI", colI);
		//			info.AddValue("colQ", colQ);
		//			info.AddValue("colY", colY);
		//			info.AddValue("name", name);
		//		}
		//
		//		#endregion
	}


}
