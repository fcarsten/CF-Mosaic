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
using System.Windows.Forms;
using MWA.Progress;


namespace org.carsten
{
	/// <summary>
	/// Summary description for Harvester.
	/// </summary>
	public class Harvester
	{
		public static void harvest(Form mainWindow, string path) 
		{
			if(path==null) return;
			string dbPath = MosaicWorker.getDatabasePath();
			if(dbPath == null) return;
			try 
			{
				File.Delete(dbPath + MosaicWorker.DB_NAME);
			} 
			catch (Exception e) 
			{
				Console.WriteLine("Could not delete DB file: " + e.Message);
			}

			ProgressWindow progress = new ProgressWindow(mainWindow);
			progress.setUserData(new object[] {path, dbPath});

//			progress.setCancelMessage("Are you sure you want to cancel the database creation? You can continue using the program with the images added so far, but the database will automatically recreated again next time you start the program.");
			progress.Text = "Adding Images";
			System.Threading.ThreadPool.QueueUserWorkItem( new System.Threading.WaitCallback( DoBackgroundHarvest ), progress );
			progress.ShowDialog(mainWindow);
		}

		public static void DoBackgroundHarvest( object status ) 
		{
			IProgressCallback callback = status as IProgressCallback;
			object[] data = callback.getUserData();
			string path= data[0] as string;
			string dbpath= data[1] as string;

			int numFiles = doHarvest(callback, path, dbpath, true);

			try
			{
				callback.Begin( 0, numFiles );
doHarvest(callback, path, dbpath, false);
			}
			catch( System.Threading.ThreadAbortException )
			{
				// We want to exit gracefully here (if we're lucky)
			}
			catch( System.Threading.ThreadInterruptedException )
			{
				// And here, if we can
			}
			finally
			{
				if( callback != null )
				{
					callback.End();
				}
			}
		}

		public static int doHarvest(IProgressCallback callback, string path, string dbPath, bool countOnly)
		{
			int count =0;
			string[] dirs = null;
			try 
			{
				 dirs= Directory.GetDirectories(path);
			} 
			catch (DirectoryNotFoundException) 
			{
				return 0;
			}

			foreach(string dir in dirs)
			{
				count+= doHarvest(callback, dir, dbPath, countOnly);
			}

			string[] files = Directory.GetFiles(path);

			count += files.Length;

			if(countOnly)
				return count;

			callback.SetText2("Looking in "+path+".");

			foreach( string file in files) 
			{
				Console.WriteLine(file);
				callback.SetText( String.Format( "Adding Image {0} of {1}: {2}", callback.GetCurrentCount(), callback.GetRangeMax(), file));
				callback.Increment(1);//  (  );//  StepTo( i );
				if( callback.IsAborting )
				{
					return count;
				}
				addToDB(file, dbPath);
				if( callback.IsAborting )
				{
					return count;
				}
			}
			return count;
		}

		static int imageCounter = 0;
		public static int TILE_WIDTH = 180;
		public static int TILE_HEIGHT = 128;

		public static void  addToDB(string imageFile, string dbPath)
		{
//			JJBitmapImage image = new JJBitmapImage(imageFile);
			Bitmap i = null;
			try 
			{
				i = JJBitmapImage.getThumbNail(imageFile, TILE_WIDTH, TILE_HEIGHT);
				if(i==null)
					return;
				if (i.Height > i.Width)
				{
					i.RotateFlip(RotateFlipType.Rotate90FlipNone);
				}
				string fileName = dbPath + "\\image" + (imageCounter++) + ".jpg";
		
				while(File.Exists(fileName))
				{
					fileName = dbPath + "\\image" + (imageCounter++) + ".jpg";
				}

				i.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
			} 
			finally 
			{
				if(i!=null)
				i.Dispose();
			}

		}

		public static void  addToDBSlow(string imageFile, string dbPath)
		{
			Bitmap i = null;
		
			try
			{
				i = new Bitmap(imageFile);
			}
			catch (Exception e)
			{
				System.Console.Error.WriteLine("Error: " + e.Message);
				return ;
			}
		
			if (i.Height > i.Width)
			{
				i.RotateFlip(RotateFlipType.Rotate90FlipNone);
			}
			if((TILE_WIDTH != i.Width) || 
				(TILE_HEIGHT != i.Height) ||
				i.PixelFormat != PixelFormat.Format32bppArgb)
			{
				i = JJImage.FixedSizeCopy(i, TILE_WIDTH, TILE_HEIGHT, true);
			}
			
			string fileName = dbPath + "\\image" + (imageCounter++) + ".jpg";
		
			while(File.Exists(fileName))
			{
				fileName = dbPath + "\\image" + (imageCounter++) + ".jpg";
			}

			i.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
			i.Dispose();
		}
	
	}
}
