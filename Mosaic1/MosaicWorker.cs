/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Win32;
using MWA.Progress;

namespace org.carsten
{
	public class MosaicException : Exception 
	{
		public MosaicException(string msg) : base(msg)
		{

		}
	}

	/// <summary>
	/// Summary description for MosaicWorker.
	/// </summary>
	public class MosaicWorker
	{

		// Haar distances of less than the cull value are not added to the total
		// error.
		//		private float cullValue = 0.25f;
		private IMosaicWorkerListener listener=null;

		private CFMosaicInfo mosaicInfo=null;

		public enum MosaicAlgorithm 
		{
			RGB,
			HAAR
		};

		public MosaicAlgorithm Algorithm 
		{
			get	
			{
				return algorithm;
			}
			set	
			{
				algorithm	=	value;
			}
		}

		private MosaicAlgorithm algorithm = MosaicAlgorithm.RGB;//HAAR;//RGB;

//		public float getCullValue() 
//		{
//			return cullValue;
//		}

		public MosaicWorker(IMosaicWorkerListener l)
		{
			listener = l;
			mosaicInfo=null;
		}

		private Thread thread = null;

		public void Run(CFMosaicInfo info) 
		{
			if(info == null) return;

			mosaicInfo = info;
			cancelled= false;

			//
			// Let's make sure we have a valid database of tiles first!
			//
			string dbPath = getDatabasePath();
			if(dbPath==null) return;

			if(thread!=null && thread.ThreadState != ThreadState.Unstarted &&
				thread.ThreadState != ThreadState.Stopped)
			{
				throw new MosaicException("Already computing a mosaic");
			}
		
			thread= new Thread(new ThreadStart(apply));

			thread.Priority= ThreadPriority.BelowNormal;
			thread.IsBackground=true;
			thread.Start();
		}

		private const string REG_KEY = "Software\\CFMosaic";
		private const string REG_VALUE = "DatabasePath";
		private static string databasePath = null;
		public static string getDatabasePath() 
		{
			if(databasePath!=null) 
			{
				return databasePath;
			}
			RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY);

			// If the return value is null, the key doesn't exist
			if ( key == null ) 
			{
				// The key doesn't exist; create it / open it
				key = Registry.CurrentUser.CreateSubKey( REG_KEY );
			}

			databasePath = (string)key.GetValue( REG_VALUE );
			if(databasePath==null || !System.IO.Directory.Exists(databasePath)) 
			{
				MessageBox.Show("Image repository not found.\n\nYou either have not yet specified a folder which CF Mosaic should\nuse as its image repository, or that folder has been deleted or moved.\nPlease specify a new folder.",
					"CF Mosaic: Image repository does not exist", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			
			while(databasePath==null || !System.IO.Directory.Exists(databasePath)) 
			{
				FolderBrowserDialog fd = new FolderBrowserDialog();
				fd.Description = "Please select the folder which CF Mosaic should use as image repository:";
				fd.ShowDialog();
				databasePath = fd.SelectedPath;
				if(databasePath==null || databasePath=="") 
				{
					MessageBox.Show("CF Mosaic can not work without an image repository\nExiting ...",
						"CF Mosaic: No Image Repository specified", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(0);
				}

				if(!System.IO.Directory.Exists(databasePath))
					System.IO.Directory.CreateDirectory(databasePath);
				
				if(!System.IO.Directory.Exists(databasePath))
					databasePath = null;
				
				if(databasePath==null) 
				{
					MessageBox.Show("The specified path does not exist and can not be created.\nPlease specify a different one.",
						"CF Mosaic: Invalid Image Repository Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			
				key.SetValue(REG_VALUE, databasePath);
			}
			key.Close();
			return databasePath;
		}

		public string etaText = "Time Remaining: 00:00";
		public string tileToGoText = "Tiles: 0 of 0";

		bool cancelled= false;

		public void cancel(bool flag) 
		{
			cancelled= flag;
		}

	//	static int counter = 1;

		public void setRepeatRate(int v) 
		{
			if(mosaicInfo!=null)
				mosaicInfo.setRepeatRate(v);
		}

		public void apply() 
		{
			try 
			{
				getDataBase(listener as Form);
//				bool imagesValid = true;
//
//				foreach (DBEntry candidate in getDataBase()) 
//				{
//					//				candidate.used = 0;
//					if(!imagesValid)
//						candidate.resetImage();
//				}
  
				DateTime startTime = DateTime.Now;
				Console.WriteLine(startTime);

				//			long  = System.currentTimeMillis();
				long timeLeft =  0;//UInt32.MaxValue;
  
				int gone = 0;
  
				int kMax = mosaicInfo.getTilesPerColumn();
				int iMax = mosaicInfo.getTilesPerRow();
				int mosaicWidth = mosaicInfo.getTileWidth();
				int mosaicHeight= mosaicInfo.getTileHeight();
				Bitmap image = mosaicInfo.getImage();
				
				int toGo = kMax* iMax ;
				Console.WriteLine("Computing " + toGo + " tiles");
				Console.WriteLine("");

				for(int k = 0; k<kMax; k++)
				{
					for(int i = 0; i<iMax; i++)
					{
						int destX= (int)(i*mosaicWidth);
						int destY= (int)(k*mosaicHeight);
						int destWidth= (int)Math.Min(mosaicWidth, image.Width-i*mosaicWidth);
						int destHeight=	(int)Math.Min(mosaicHeight, image.Height-k*mosaicHeight);

						if(cancelled) 
							return;

						JJImage cropImg = JJBitmapImage.cropImage(image, destX, destY, destWidth, destHeight,false);
//						string fileName = "D:\\tmp\\tiles\\tile"+(counter++)+".jpg";
//						cropImg.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
						DBEntry tile = new DBEntry(cropImg.getBitmap());
     
						if(cancelled) 
							return;
						DBEntry stone = findClosest(tile, mosaicInfo.getTiles(), k, i, true);

						tile.resetImage();

						if(cancelled) 
							return;

						if(stone!=null)
						{
							Bitmap img = stone.getImage();
							listener.DrawTile(this, img,
								new Rectangle(destX,destY,destWidth,destHeight),
								new Rectangle(0,0,img.Width,img.Height));
						}
						gone++;
						toGo--;
						TimeSpan duration = DateTime.Now - startTime;
						double tpt =  duration.TotalMilliseconds / (double)(1000.0 *gone);

						timeLeft = (long)(tpt*toGo);//Math.Max((long)(tpt*toGo), timeLeft);
						etaText = "Time Remaining: " + (timeLeft/60) + ":" + 
							(timeLeft % 60 >= 10 ? "" : "0")+(timeLeft%60);
						tileToGoText = "Tiles: "+ toGo + " of " + (toGo+gone);

					}
				}
			}							 			
			finally 
			{
				Console.WriteLine("");
				listener.MosaicFinished(this);
			}

			//			return image;
		}

		public const string DB_NAME = "\\dataNet.db";

		public static void clearDatabase() 
		{
			File.Delete(getDatabasePath() + DB_NAME);
			database=null;
		}

		public static void clearImageRepository() 
		{
			string[] files = Directory.GetFiles(getDatabasePath());

			foreach( string file in files) 
			{
				Console.WriteLine("Deleting: "+file);
				File.Delete(file);
			}
		}

		public static void createDatabase(Form window) 
		{
			ProgressWindow progress = new ProgressWindow(window);
			progress.setCancelMessage("Are you sure you want to cancel the database creation? You can continue using the program with the images added so far, but the database will automatically recreated again next time you start the program.");
			progress.Text = "Creating DB";
			System.Threading.ThreadPool.QueueUserWorkItem( new System.Threading.WaitCallback( DoCreateDatabase ), progress );
			progress.ShowDialog(window);
		}

		private static void DoCreateDatabase(object status )
		{
			IProgressCallback callback = status as IProgressCallback;
			
			try
			{
   
				database = new ArrayList();

				string[] files = Directory.GetFiles(getDatabasePath());
				if(files.Length == 0) 
				{
//					callback.showErrorMessage("Repository does not contain any images. Please add some.");
					return;
				}

				callback.Begin( 0, files.Length-1 );
								
				callback.SetText2("The image database is being created.\nPlease hold on, this might take a second ...");

				for(int i = 0; i<files.Length; i++)
				{
					callback.SetText( String.Format( "Adding Image: {0} of {1}", i,  files.Length-1) );
					callback.StepTo( i );
					if( callback.IsAborting )
					{
						return;
					}
					
					addToDatabase(files[i]);
					if( callback.IsAborting )
					{
						return;
					}
				}

				System.Console.Error.WriteLine("Saving database");
				callback.SetText("Saving database to disc ...");
				try
				{
					System.IO.FileStream dbOut = new System.IO.FileStream(getDatabasePath() + DB_NAME, System.IO.FileMode.Create);
					BinaryWriter bw = new BinaryWriter(dbOut);

					BinaryFormatter b=new BinaryFormatter();
					foreach (DBEntry entry in getDataBase(null))  // Should never pop up the dialog anyway
					{
//						entry.saveTo(bw);
						b.Serialize(dbOut,entry);
					}
					dbOut.Flush();
					dbOut.Close();
				}
				catch (System.IO.IOException ex)
				{
					System.Console.Error.WriteLine("Error saving database: " + ex.Message);
				}
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

		public static int getDataBaseSize(Form window) 
		{
			return getDataBase(window).Count;
		}

		public static IList getDataBase(Form window)
		{
			if(database ==null)
			{
				System.IO.FileStream inDB = null;
				try
				{
					inDB = new System.IO.FileStream(getDatabasePath() + DB_NAME, System.IO.FileMode.Open, System.IO.FileAccess.Read);
					database = Deserialize(inDB);
					inDB.Close();
				} 
				catch(System.UnauthorizedAccessException e) 
				{
					MessageBox.Show("Could not save repository path to registry.\nPlease close all applications which might access\nthe CF Mosaic registry entry and try again.\nIf this does not help try rebooting the machine.",
						"CF Mosaic: Registry Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Console.WriteLine("Error: " +e.Message);
					Environment.Exit(-1);
				}
				catch (Exception e)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					System.Console.Error.WriteLine("Could not find database file because: " + e.Message);
					if(inDB != null) 
					{
						inDB.Close();
					}

					Console.WriteLine("Initialising database ");
					createDatabase(window) ;
					Console.WriteLine("Creating new one");
 
				}
			}
			return database;
		}

		private static void addToDatabase(String s)
		{
			try
			{
				DBEntry entry = new DBEntry(s);//,this);
				getDataBase(null).Add(entry);
			}
			catch (Exception e)
			{
				Console.WriteLine("Can't handle \"image\" "+s+" beacuse " +e.Message);
			}
  
		}
		private static IList database= null;
		public static IList Deserialize(System.IO.FileStream inDB)
		{
			System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(inDB);

			BinaryFormatter formatter = new BinaryFormatter();
			IList database=new ArrayList();
			while(inDB.Position < inDB.Length) 
			{
//				database.Add(new DBEntry(binaryReader));
				database.Add(formatter.Deserialize(inDB));
			}
			return database;
		}

		DBEntry findClosest(DBEntry tile, Object[,] tiles, int k, int l, bool skipDuplicates)
		{
			DBEntry res = null;
			double minErr = Double.NaN;

			foreach (DBEntry candidate in getDataBase(listener as Form) ) 
			{
				double error = 0;
   
				if(algorithm == MosaicAlgorithm.RGB) 
				{
					error = tile.distanceRGB(candidate, minErr);
				} 
				else 
				{
					error = tile.distanceHaar(candidate, minErr);
				}

				// Let's first test if the tile is good enough
   
				if(error > minErr)//[topX-1])
					continue;
   
				// Fine, it's good enough, but has it been used recently?
				if(skipDuplicates) 
				{
					bool shouldContinue=false;
					for(int kk = Math.Max(0, k- mosaicInfo.getRepeatRate()); kk<= Math.Min(tiles.GetLength(0) -1,
						k+mosaicInfo.getRepeatRate()); kk++)
					{
						for(int ll = Math.Max(0, l- mosaicInfo.getRepeatRate()); ll<= Math.Min(tiles.GetLength(1) -1,
							l+mosaicInfo.getRepeatRate()); ll++)
						{
							if(tiles[kk,ll] == candidate)
							{
								shouldContinue= true;
								break;
							}
						}
						if(shouldContinue) break;
					}
					if(shouldContinue)continue;
				}

				if(Double.IsNaN(minErr) || (error<minErr))
				{
					minErr= error;
					res = candidate;
				}
			}
    
			if(res==null) 
			{
				if(!skipDuplicates)
					return null;
				else
					return findClosest(tile, tiles, k, l, false);
			}

			try
			{
				//Bitmap resImg = res.getImage();//res[retIndex].getImage();//this);
				tiles[k,l] = res;//[retIndex];
				return res;//resImg;
			}
			catch(IOException e)
			{
				Console.Error.WriteLine("Could not read tile: " + e.Message);
				database.Remove(res);//[retIndex]);
			}
			return findClosest(tile, tiles, k, l, skipDuplicates);
  
		}
		static public System.Random Random = new System.Random();

	}
}
