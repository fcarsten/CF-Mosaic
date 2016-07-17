/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.IO;
using org.carsten;


namespace org.carsten
{
	/// <summary>
	/// EXIFThumbnailExtractor Class
	/// 
	/// </summary>
	/// 


	public class EXIFThumbnailExtractor
	{

		//	private const int MAX_COMMENT = 1000;
		private const int  DEFAULT_SECTIONS = 20;


		//--------------------------------------------------------------------------
		// JPEG markers consist of one or more 0xFF unsigned chars, followed by a marker
		// code unsigned char (which is not an FF).  Here are the marker codes of interest
		// in this program.  (See jdmarker.c for a more complete list.)
		//--------------------------------------------------------------------------

		private const int  M_SOF0 = 0xC0;            // Start Of Frame N
		private const int  M_SOF1 = 0xC1;            // N indicates which compression process
		private const int  M_SOF2 = 0xC2;            // Only SOF0-SOF2 are now in common use
		private const int  M_SOF3 = 0xC3;
		private const int  M_SOF5 = 0xC5;            // NB: codes C4 and CC are NOT SOF markers
		private const int  M_SOF6 = 0xC6;
		private const int  M_SOF7 = 0xC7;
		private const int  M_SOF9 = 0xC9;
		private const int  M_SOF10= 0xCA;
		private const int  M_SOF11= 0xCB;
		private const int  M_SOF13= 0xCD;
		private const int  M_SOF14= 0xCE;
		private const int  M_SOF15= 0xCF;
		private const int  M_SOI  = 0xD8;           // Start Of Image (beginning of datastream)
		private const int  M_EOI  = 0xD9;          // End Of Image (end of datastream)
		private const int  M_SOS  = 0xDA;            // Start Of Scan (begins compressed data)
		private const int  M_JFIF = 0xE0;            // Jfif marker
		private const int  M_EXIF = 0xE1;            // Exif marker
		private const int  M_COM  = 0xFE;            // COMment 

		class Section_t
		{
			public byte[]    Data;
			public int      Type;
			public uint Size;
		} ;

		ExifInfo m_exifinfo;
		string m_szLastError;
		long ExifImageWidth;
		bool MotorolaOrder;
		ArrayList sections =  new ArrayList(DEFAULT_SECTIONS);
		//		Section_t Sections=new Section_t[MAX_SECTIONS];
		//		int SectionsRead;
		
		public ExifInfo getExifInfo()
		{
			return m_exifinfo;
		}

		public byte[] getThumbnail()
		{
			if(m_exifinfo==null)return null;
			return m_exifinfo.ThumbnailPointer;
		}

		public string getError() 
		{
			return m_szLastError;
		}

		public EXIFThumbnailExtractor(ExifInfo info)
		{
			if (info!=null) 
			{
				m_exifinfo = info;
			} 
			else 
			{
				m_exifinfo = new ExifInfo();
			}

			m_szLastError="";
			ExifImageWidth = 0;
			MotorolaOrder = false;
			sections.Clear();
			//			SectionsRead=0;
		}

		Encoding ascii = Encoding.ASCII;
		////////////////////////////////////////////////////////////////////////////////
		public bool DecodeExif(string fileName)
		{
			FileStream hFile = new  FileStream(fileName, FileMode.Open, FileAccess.Read);

			byte a;
			//	int HaveCom = 0;

			a= (byte)hFile.ReadByte();
			//    a = fgetc(hFile);

			if (a != 0xff || hFile.ReadByte() != M_SOI)
			{
				return false;
			}

			for(;;)
			{
				int itemlen;
				int marker = 0;
				byte ll,lh;
				int got;
				byte[] Data;

				//				if (SectionsRead >= MAX_SECTIONS)
				//				{
				//					m_szLastError= @"Too many sections in jpg file";
				//					return false;
				//				}

				for (a=0;a<7;a++)
				{
					marker = hFile.ReadByte();//fgetc(hFile);
					if (marker != 0xff) break;

					if (a >= 6)
					{
						m_szLastError= @"too many padding unsigned chars\n";
						return false;
					}
				}

				if (marker == 0xff)
				{
					// 0xff is legal padding, but if we get that many, something's wrong.
					m_szLastError= @"too many padding unsigned chars!";
					return false;
				}
				Section_t section = new Section_t();

				section.Type = marker;

				// Read the length of the section.
				lh = (byte)hFile.ReadByte();
				ll = (byte)hFile.ReadByte();

				itemlen = (lh << 8) | ll;

				if (itemlen < 2)
				{
					m_szLastError= @"invalid marker";
					return false;
				}

				section.Size = (uint)itemlen;

				Data = new byte[itemlen];
				section.Data = Data;

				// Store first two pre-read unsigned chars.
				Data[0] = lh;
				Data[1] = ll;

				got = hFile.Read(Data, 2,itemlen-2);

				//    got = fread(Data+2, 1, itemlen-2,hFile); // Read the whole section.
				if (got != itemlen-2)
				{
					m_szLastError=@"Premature end of file?";
					return false;
				}

				// 
				// We are convinced it is a real section, so let's add it to our colleciton
				//
				sections.Add(section);
				//SectionsRead += 1;

				switch(marker)
				{
					case M_SOS:   // stop before hitting compressed data 
						return true;
					case M_EOI:   // in case it's a tables-only JPEG stream
						m_szLastError=@"No image in jpeg!\n";
						return false;
					case M_COM: 
						//							process_COM(Data, itemlen);
						break;
					case M_JFIF:
						// Regular jpegs always have this tag, exif images have the exif
						// marker instead, althogh ACDsee will write images with both markers.
						// this program will re-create this marker on absence of exif marker.
						// hence no need to keep the copy from the file.
						//       free(Sections[--SectionsRead].Data);
						sections.RemoveAt(sections.Count-1);
						//						Sections[--SectionsRead].Data=null;
						break;

					case M_EXIF:
						// Seen files from some 'U-lead' software with Vivitar scanner
						// that uses marker 31 for non exif stuff.  Thus make sure 
						// it says 'Exif' in the section before treating it as exif.
						string tag = new String(ascii.GetChars(Data, 2, 4));
						if(tag == "Exif") 
						{
							// if (memcmp(Data+2, "Exif", 4) == 0){
							m_exifinfo.IsExif = process_EXIF( Data, itemlen);
						}
						else
						{
							// Discard this section.
							//                    free(Sections[--SectionsRead].Data);
							//							Sections[--SectionsRead].Data=null;
							sections.RemoveAt(sections.Count-1);
						}
						break;

					case M_SOF0: 
					case M_SOF1: 
					case M_SOF2: 
					case M_SOF3: 
					case M_SOF5: 
					case M_SOF6: 
					case M_SOF7: 
					case M_SOF9: 
					case M_SOF10:
					case M_SOF11:
					case M_SOF13:
					case M_SOF14:
					case M_SOF15:
						process_SOFn(Data, marker);
						break;
					default:
						// Skip any other sections.
						//if (ShowTags) printf("Jpeg section marker 0x%02x size %d\n",marker, itemlen);
						break;
				}
			}
			//		return true;
		}
		////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------
				 Process a EXIF marker
				 Describes all the drivel that most digital cameras include...
			--------------------------------------------------------------------------*/
		bool process_EXIF(byte[] data, int length)
		{
			m_exifinfo.FlashUsed = 0; 
			/* If it's from a digicam, and it used flash, it says so. */
			m_exifinfo.Comments="";  /* Initial value - null string */

			ExifImageWidth = 0;
			//
			//Carsten: Didn't we already do this?
			//    {   /* Check the EXIF header component */
			//        const unsigned char ExifHeader[] = "Exif\0\0";
			//        if (memcmp(CharBuf+0, ExifHeader,6)){
			//			strcpy(m_szLastError,"Incorrect Exif header");
			//			return false;
			//		}
			//    }

			if (data[8]=='I' && data[9]=='I')  
			{      // memcmp(CharBuf+6,"II",2) == 0){
				MotorolaOrder = false;
			}
			else
			{
				if (data[8]=='M' && data[9]=='M')
				{   // memcmp(CharBuf+6,"MM",2) == 0){
					MotorolaOrder = true;
				}
				else
				{
					m_szLastError=@"Invalid Exif alignment marker.";
					return false;
				}
			}

			/* Check the next two values for correctness. */
			// if (Get16u(CharBuf+8) != 0x2a)
			if (Get16u(data, 10) != 0x2a)
			{
				m_szLastError=@"Invalid Exif start (1)";
				return false;
			}

			uint FirstOffset = Get32u(data, 12);//(CharBuf+10);
			if (FirstOffset < 8 || FirstOffset > 16)
			{
				// I used to ensure this was set to 8 (website I used indicated its 8)
				// but PENTAX Optio 230 has it set differently, and uses it as offset. (Sept 11 2002)
				m_szLastError=@"Suspicious offset of first IFD value";
				return false;
			}

			// unsigned char * LastExifRefd = CharBuf;

			if (!ProcessExifDir(data, 16, 8, length-6, -1))//
				return false;

			/* This is how far the interesting (non thumbnail) part of the exif went. */
			// int ExifSettingsLength = LastExifRefd - CharBuf;

			/* Compute the CCD width, in milimeters. */
			if (m_exifinfo.FocalplaneXRes != 0 && m_exifinfo.FocalplaneUnits!=0)
			{
				m_exifinfo.CCDWidth = (float)(ExifImageWidth * (m_exifinfo.FocalplaneUnits /
					m_exifinfo.FocalplaneXRes));
			}

			return true;
		}
		//--------------------------------------------------------------------------
		// Get 16 bits motorola order (always) for jpeg header stuff.
		//--------------------------------------------------------------------------
		int Get16m(byte[] data, long offset)// void * Short)
		{
			byte off = data[offset];
			byte off2 = data[offset+1];
			int res = ( off<< 8) | off2;
			return res;
			//    return (((unsigned char *)Short)[0] << 8) | ((unsigned char *)Short)[1];
		}
		////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------
				 Convert a 16 bit unsigned value from file's native unsigned char order
			--------------------------------------------------------------------------*/
		int Get16u(byte[] data, long offset) //void * Short)
		{
			int res=0;
			byte off = data[offset];
			byte off1 = data[offset+1];

			if (MotorolaOrder)
			{
				res = (off<<8) | off1;
				//			return (((unsigned char *)Short)[0] << 8) | ((unsigned char *)Short)[1];
			}
			else
			{
				res = (off1 << 8) | off;
				//return (((unsigned char *)Short)[1] << 8) | ((unsigned char *)Short)[0];
			}
			return res;
		}
		////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------
				 Convert a 32 bit signed value from file's native unsigned char order
			--------------------------------------------------------------------------*/
		int Get32s(byte[] data, long off)
		{
			byte off0 = data[off];
			byte off1 = data[off+1];
			byte off2 = data[off+2];
			byte off3 = data[off+3];

			if (MotorolaOrder)
			{
				return   ( ((sbyte)off0) << 24) | (off1 << 16)
					| (off2 << 8 ) | off3;
			}
			else
			{
				return  ( ((sbyte)off3) << 24) | (off2 << 16)
					| (off1 << 8 ) | (off0 << 0 );
			}
		}
		////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------
				 Convert a 32 bit unsigned value from file's native unsigned char order
			--------------------------------------------------------------------------*/
		uint Get32u(byte[] data, long off)
		{
			return (uint)Get32s(data, off) & 0xffffffff;
		}
		////////////////////////////////////////////////////////////////////////////////

		/* Describes format descriptor */
		static readonly int[] BytesPerFormat = {0,1,1,2,4,8,1,1,2,4,8,4,8};
		private const int  NUM_FORMATS= 12;

		private const int  FMT_BYTE      = 1 ;
		private const int  FMT_STRING    = 2;
		private const int  FMT_USHORT    = 3;
		private const int  FMT_ULONG     = 4;
		private const int  FMT_UCFRational = 5;
		private const int  FMT_SBYTE     = 6;
		private const int  FMT_UNDEFINED = 7;
		private const int  FMT_SSHORT    = 8;
		private const int  FMT_SLONG     = 9;
		private const int  FMT_SCFRational =10;
		private const int  FMT_SINGLE    =11;
		private const int  FMT_DOUBLE    =12;

		/* Describes tag values */

		private const int  TAG_EXIF_VERSION      =0x9000;
		private const int  TAG_EXIF_OFFSET       =0x8769;
		private const int  TAG_INTEROP_OFFSET    =0xa005;

		private const int  TAG_MAKE              =0x010F;
		private const int  TAG_MODEL             =0x0110;

		private const int  TAG_ORIENTATION       =0x0112;
		private const int  TAG_XRESOLUTION       =0x011A;
		private const int  TAG_YRESOLUTION       =0x011B;
		private const int  TAG_RESOLUTIONUNIT    =0x0128;

		private const int  TAG_EXPOSURETIME      =0x829A;
		private const int  TAG_FNUMBER           =0x829D;

		private const int  TAG_SHUTTERSPEED      =0x9201;
		private const int  TAG_APERTURE          =0x9202;
		private const int  TAG_BRIGHTNESS        =0x9203;
		private const int  TAG_MAXAPERTURE       =0x9205;
		private const int  TAG_FOCALLENGTH       =0x920A;

		private const int  TAG_DATETIME_ORIGINAL =0x9003;
		private const int  TAG_USERCOMMENT       =0x9286;

		private const int  TAG_SUBJECT_DISTANCE  =0x9206;
		private const int  TAG_FLASH             =0x9209;

		private const int  TAG_FOCALPLANEXRES    =0xa20E;
		private const int  TAG_FOCALPLANEYRES    =0xa20F;
		private const int  TAG_FOCALPLANEUNITS   =0xa210;
		private const int  TAG_EXIF_IMAGEWIDTH   =0xA002;
		private const int  TAG_EXIF_IMAGELENGTH  =0xA003;

		/* the following is added 05-jan-2001 vcs */
		private const int  TAG_EXPOSURE_BIAS     =0x9204;
		//		private const int  TAG_WHITEBALANCE      =0x9208;
		private const int  TAG_METERING_MODE     =0x9207;
		private const int  TAG_EXPOSURE_PROGRAM  =0x8822;
		private const int  TAG_ISO_EQUIVALENT    =0x8827;
		private const int  TAG_COMPRESSION_LEVEL =0x9102;

		private const int  TAG_THUMBNAIL_OFFSET  =0x0201;
		private const int  TAG_THUMBNAIL_LENGTH  =0x0202;

		private const int TAG_IMAGE_TITLE				 =0x010E;
		private const int TAG_SOFTWARE				 =0x0131;

		/*--------------------------------------------------------------------------
				 Process one of the nested EXIF directories.
			--------------------------------------------------------------------------*/
		/* First directory starts 16 unsigned chars in.  Offsets start at 8 unsigned chars in. */
		bool ProcessExifGPSDir(byte[] data, long dirStart, long OffsetBase, int ExifLength, int parentTag) //byte[] DirStart, byte[] OffsetBase, uint ExifLength,
		{
		//	long a;
			int NumDirEntries;
			uint ThumbnailOffset = 0;
			uint ThumbnailSize = 0;

			NumDirEntries = Get16u(data, dirStart);

			if ((dirStart+2+NumDirEntries*12) > (OffsetBase+ExifLength))
			{
				m_szLastError=@"Illegally sized directory";
				return false;
			}

			for (int de=0;de<NumDirEntries;de++)
			{
				int Tag, Format, Components;
				long ValuePtr;
				/* This actually can point to a variety of things; it must be
							 cast to other types when used.  But we use it as a unsigned char-by-unsigned char
							 cursor, so we declare it as a pointer to a generic unsigned char here.
						*/
				int BytesCount;
				long DirEntry;
				DirEntry = dirStart+2+12*de;

				Tag = Get16u(data, DirEntry);
				Format = Get16u(data, DirEntry+2);
				Components = (int)Get32u(data, DirEntry+4);

				if ((Format-1) >= NUM_FORMATS) 
				{
					/* (-1) catches illegal zero case as unsigned underflows to positive large */
					m_szLastError=@"Illegal format code in EXIF dir";
					return false;
				}

				BytesCount = Components * BytesPerFormat[Format];

				if (BytesCount > 4)
				{
					int OffsetVal;
					OffsetVal = (int)Get32u(data, DirEntry+8);
					/* If its bigger than 4 unsigned chars, the dir entry contains an offset.*/
					if (OffsetVal+BytesCount > ExifLength)
					{
						/* Bogus pointer offset and / or unsigned charcount value */
						m_szLastError=@"Illegal pointer offset value in EXIF.";
						return false;
					}
					ValuePtr = OffsetBase+OffsetVal;
				}
				else
				{
					/* 4 unsigned chars or less and value is in the dir entry itself */
					ValuePtr = DirEntry+8;
				}

				/* Extract useful components of tag */
				switch(Tag)
				{
					case 0x0000: //EXIF_TAG_GPS_VERSION_ID
						m_exifinfo.gpsVersionId= new byte[4];
						for(int i=0; i<4; i++)
						{
							m_exifinfo.gpsVersionId[i]= data[ValuePtr+i];
						}
						break;
					case 0x0001: //EXIF_TAG_GPS_LATITUDE_REF
						m_exifinfo.gpsLattitudeRef= new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1));
						break;
					case 0x0002: //EXIF_TAG_GPS_LATITUDE
						m_exifinfo.gpsLattitude = new CFRational[3];
						m_exifinfo.gpsLattitude[0] = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						m_exifinfo.gpsLattitude[1] = (CFRational)ConvertAnyFormat(data, ValuePtr+8, Format);
						m_exifinfo.gpsLattitude[2] = (CFRational)ConvertAnyFormat(data, ValuePtr+16, Format);
						break;
					case 0x0003:// EXIF_TAG_GPS_LONGITUDE_REF
						m_exifinfo.gpsLongitudeRef= new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1));
						break;
					case 0x0004:// EXIF_TAG_GPS_LONGITUDE
						m_exifinfo.gpsLongitude = new CFRational[3];
						m_exifinfo.gpsLongitude[0] = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						m_exifinfo.gpsLongitude[1] = (CFRational)ConvertAnyFormat(data, ValuePtr+8, Format);
						m_exifinfo.gpsLongitude[2] = (CFRational)ConvertAnyFormat(data, ValuePtr+16, Format);
						break;
					case 0x0005:// EXIF_TAG_GPS_ALTITUDE_REF
						m_exifinfo.gpsAltitudeRef= data[ValuePtr];
						break;
					case 0x0006:// EXIF_TAG_GPS_ALTITUDE
						m_exifinfo.gpsAltitude = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
break;
					case 0x0007:// EXIF_TAG_GPS_TIME_STAMP
						m_exifinfo.gpsTimeStamp = new CFRational[3];
						m_exifinfo.gpsTimeStamp[0] = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						m_exifinfo.gpsTimeStamp[1] = (CFRational)ConvertAnyFormat(data, ValuePtr+8, Format);
						m_exifinfo.gpsTimeStamp[2] = (CFRational)ConvertAnyFormat(data, ValuePtr+16, Format);
						break;
					case 0x0008:// EXIF_TAG_GPS_SATELLITES
					case 0x0009:// EXIF_TAG_GPS_STATUS
					case 0x000a:// EXIF_TAG_GPS_MEASURE_MODE
					case 0x000b:// EXIF_TAG_GPS_DOP
					case 0x000c:// EXIF_TAG_GPS_SPEED_REF
					case 0x000d:// EXIF_TAG_GPS_SPEED
					case 0x000e:// EXIF_TAG_GPS_TRACK_REF
					case 0x000f:// EXIF_TAG_GPS_TRACK
					case 0x0010:// EXIF_TAG_GPS_IMG_DIRECTION_REF
					case 0x0011:// EXIF_TAG_GPS_IMG_DIRECTION
					case 0x0012:// EXIF_TAG_GPS_MAP_DATUM
					case 0x0013:// EXIF_TAG_GPS_DEST_LATITUDE_REF
					case 0x0014:// EXIF_TAG_GPS_DEST_LATITUDE
					case 0x0015:// EXIF_TAG_GPS_DEST_LONGITUDE_REF
					case 0x0016:// EXIF_TAG_GPS_DEST_LONGITUDE
					case 0x0017:// EXIF_TAG_GPS_DEST_BEARING_REF
					case 0x0018:// EXIF_TAG_GPS_DEST_BEARING
					case 0x0019:// EXIF_TAG_GPS_DEST_DISTANCE_REF
					case 0x001a:// EXIF_TAG_GPS_DEST_DISTANCE
					case 0x001b:// EXIF_TAG_GPS_PROCESSING_METHOD
					case 0x001c:// EXIF_TAG_GPS_AREA_INFORMATION
					case 0x001d:// EXIF_TAG_GPS_DATE_STAMP
					case 0x001e:// EXIF_TAG_GPS_DIFFERENTIAL
					default:
					{
						string parent="unknown section " + parentTag;
						switch(parentTag) 
						{
							case 40965: 
								parent = "Interoperability section";
								break;
							case 34665:
								parent = "EXIF section";
								break;
						}
						Console.WriteLine("Unknown tag: {0} in "+ parent, Tag);
						break;
					}
				}
			}

		{
			/* In addition to linking to subdirectories via exif tags,
					 there's also a potential link to another directory at the end
					 of each directory.  This has got to be the result of a
					 committee!  
				*/
			long SubdirStart;
			long Offset;
			Offset = Get16u(data, dirStart+2+12*NumDirEntries);
			if (Offset!=0)
			{
				SubdirStart = OffsetBase + Offset;
				if (SubdirStart < OffsetBase 
					|| SubdirStart > OffsetBase+ExifLength)
				{
					m_szLastError=@"Illegal subdirectory link";
					return false;
				}
				ProcessExifDir(data, SubdirStart, OffsetBase, ExifLength, parentTag);//, m_exifinfo, LastExifRefdP);
				//		Console.WriteLine("Can't handle exif subdirs");
				//					ProcessExifDir(&data[SubdirStart], ExifLength);
			}
		}


			if ( (ThumbnailSize !=0) && (ThumbnailOffset !=0))
			{
				if (ThumbnailSize + ThumbnailOffset <= ExifLength)
				{
					/* The thumbnail pointer appears to be valid.  Store it. */

					// m_exifinfo.ThumbnailPointer = OffsetBase + ThumbnailOffset;
					m_exifinfo.ThumbnailPointer = new byte[ThumbnailSize];
					Array.Copy(data, OffsetBase + ThumbnailOffset, m_exifinfo.ThumbnailPointer, 0, ThumbnailSize);
					;
					//	m_exifinfo.ThumbnailSize = ThumbnailSize;
				}
			}

			return true;
		}
	

		// CharBuf+14, CharBuf+6, length-6, m_exifinfo, data))//&LastExifRefd))
		// ref EXIFINFO m_exifinfo, byte[] LastExifRefdP )
		bool ProcessExifDir(byte[] data, long dirStart, long OffsetBase, int ExifLength, int parentTag) //byte[] DirStart, byte[] OffsetBase, uint ExifLength,
		{
			long a;
			int NumDirEntries;
			uint ThumbnailOffset = 0;
			uint ThumbnailSize = 0;

			NumDirEntries = Get16u(data, dirStart);

			if ((dirStart+2+NumDirEntries*12) > (OffsetBase+ExifLength))
			{
				m_szLastError=@"Illegally sized directory";
				return false;
			}

			for (int de=0;de<NumDirEntries;de++)
			{
				int Tag, Format, Components;
				long ValuePtr;
				/* This actually can point to a variety of things; it must be
							 cast to other types when used.  But we use it as a unsigned char-by-unsigned char
							 cursor, so we declare it as a pointer to a generic unsigned char here.
						*/
				int BytesCount;
				long DirEntry;
				DirEntry = dirStart+2+12*de;

				Tag = Get16u(data, DirEntry);
				Format = Get16u(data, DirEntry+2);
				Components = (int)Get32u(data, DirEntry+4);

				if ((Format-1) >= NUM_FORMATS) 
				{
					/* (-1) catches illegal zero case as unsigned underflows to positive large */
					m_szLastError=@"Illegal format code in EXIF dir";
					return false;
				}

				BytesCount = Components * BytesPerFormat[Format];

				if (BytesCount > 4)
				{
					int OffsetVal;
					OffsetVal = (int)Get32u(data, DirEntry+8);
					/* If its bigger than 4 unsigned chars, the dir entry contains an offset.*/
					if (OffsetVal+BytesCount > ExifLength)
					{
						/* Bogus pointer offset and / or unsigned charcount value */
						m_szLastError=@"Illegal pointer offset value in EXIF.";
						return false;
					}
					ValuePtr = OffsetBase+OffsetVal;
				}
				else
				{
					/* 4 unsigned chars or less and value is in the dir entry itself */
					ValuePtr = DirEntry+8;
				}

				/* Extract useful components of tag */
				switch(Tag)
				{

					case TAG_MAKE:
						string make = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1));
						m_exifinfo.CameraMake = make;
						//                strncpy(m_exifinfo.CameraMake, (char*)ValuePtr, 31);
						break;

					case TAG_MODEL:
						string model= new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1));
						m_exifinfo.CameraModel = model;
						// strncpy(m_exifinfo.CameraModel, (char*)ValuePtr, 39);
						break;

					case TAG_EXIF_VERSION:
						//				strncpy(m_exifinfo.Version,(char*)ValuePtr, 4);
						m_exifinfo.Version = new string(ascii.GetChars(data,(int)ValuePtr, 4));
						break;

					case TAG_DATETIME_ORIGINAL:
						//strncpy(m_exifinfo.DateTime, (char*)ValuePtr, 19);
						m_exifinfo.DateTime= new string(ascii.GetChars(data,(int)ValuePtr, 19));
						break;

					case TAG_USERCOMMENT:
                        if (BytesCount > 10)
                        {
                            byte[] asciiEncoding = { 0x41, 0x53, 0x43, 0x49, 0x49, 0x00, 0x00, 0x00 };
                            bool asciiP = true;
                            byte[] unicodeEncoding = { 0x55, 0x4E, 0x49, 0x43, 0x4F, 0x44, 0x45, 0x00 };
                            bool unicodeP = true;

                            byte[] format = new byte[8];
                            for (int i = 0; i < 8; i++)
                            {
                                if (data[ValuePtr + i] != asciiEncoding[i])
                                    asciiP = false;
                                if (data[ValuePtr + i] != unicodeEncoding[i])
                                    unicodeP = false;
                            }
                            Encoding encoder = null;
                            if (asciiP)
                                encoder = ascii;
                            else if (unicodeP)
                                encoder = Encoding.Unicode;

                            if (encoder != null)
                                m_exifinfo.Comments = new string(encoder.GetChars(data, (int)ValuePtr + 8,
                                    BytesCount - 9)).Trim();
                            else
                                m_exifinfo.Comments = "Unknown User Comment Encoding";
                            // +	new string(ascii.GetChars(data, (int)ValuePtr, 8)).Trim();
                        }
						break;
					case TAG_FNUMBER:
						/* Simplest way of expressing aperture, so I trust it the most.
									 (overwrite previously computd value if there is one)
									 */
						m_exifinfo.ApertureFNumber = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_APERTURE:
					case TAG_MAXAPERTURE:
						/* More relevant info always comes earlier, so only
								 use this field if we don't have appropriate aperture
								 information yet. 
								*/
						if (m_exifinfo.Aperture == 0)
						{
							m_exifinfo.Aperture = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						}
						break;

					case TAG_BRIGHTNESS:
						m_exifinfo.Brightness = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_FOCALLENGTH:
						/* Nice digital cameras actually save the focal length
									 as a function of how farthey are zoomed in. 
								*/

						m_exifinfo.FocalLength = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_SUBJECT_DISTANCE:
						/* Inidcates the distacne the autofocus camera is focused to.
									 Tends to be less accurate as distance increases.
								*/
						m_exifinfo.Distance = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_EXPOSURETIME:
						/* Simplest way of expressing exposure time, so I
									 trust it most.  (overwrite previously computd value
									 if there is one) 
								*/
						m_exifinfo.ExposureTime = 
							(CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_SHUTTERSPEED:
						/* More complicated way of expressing exposure time,
									 so only use this value if we don't already have it
									 from somewhere else.  
								*/
						if (m_exifinfo.ExposureTime == 0)
						{
							m_exifinfo.shutterSpeed = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						}
						break;

					case TAG_FLASH:
						if (((int)ConvertAnyFormat(data, ValuePtr, Format) & 7 ) !=0)
						{
							m_exifinfo.FlashUsed = 1;
						}
						else
						{
							m_exifinfo.FlashUsed = 0;
						}
						break;

					case TAG_ORIENTATION:
						m_exifinfo.orientation = (int)ConvertAnyFormat(data, ValuePtr, Format);
						if (m_exifinfo.orientation < 1 || m_exifinfo.orientation > 8)
						{
							m_szLastError=@"Undefined rotation value";
							m_exifinfo.orientation = 0;
						}
						break;

					case TAG_EXIF_IMAGELENGTH:
					case TAG_EXIF_IMAGEWIDTH:
						/* Use largest of height and width to deal with images
									 that have been rotated to portrait format.  
								*/
						if(Format == 3)
							a = (int)ConvertAnyFormat(data, ValuePtr, Format);
						else if(Format == 4)
							a = (uint)ConvertAnyFormat(data, ValuePtr, Format);
						else 
						{
							Console.WriteLine("Invalid format {0} for tag {1}", Format, Tag);
							break;
						}
						if (ExifImageWidth < a) ExifImageWidth = a;
						break;

					case TAG_FOCALPLANEXRES:
						m_exifinfo.FocalplaneXRes = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_FOCALPLANEYRES:
						m_exifinfo.FocalplaneYRes = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;

					case TAG_RESOLUTIONUNIT:
					switch((int)ConvertAnyFormat(data, ValuePtr, Format))
					{
						case 1: m_exifinfo.ResolutionUnit = new CFRational(1,1); break; /* 1 inch */
						case 2:	m_exifinfo.ResolutionUnit = new CFRational(1,1); break;
						case 3: m_exifinfo.ResolutionUnit = new CFRational(100, 254);    break;  /* 1 centimeter*/
						case 4: m_exifinfo.ResolutionUnit = new CFRational(10, 254);   break;  /* 1 millimeter*/
						case 5: m_exifinfo.ResolutionUnit = new CFRational(1, 254);  break;/* 1 micrometer*/
					}
						break;

					case TAG_FOCALPLANEUNITS:
					switch((int)ConvertAnyFormat(data, ValuePtr, Format))
					{
						case 1: m_exifinfo.FocalplaneUnits = new CFRational(1,1); break; /* 1 inch */
						case 2:	m_exifinfo.FocalplaneUnits = new CFRational(1,1); break;
						case 3: m_exifinfo.FocalplaneUnits = new CFRational(100, 254);    break;  /* 1 centimeter*/
						case 4: m_exifinfo.FocalplaneUnits = new CFRational(10, 254);   break;  /* 1 millimeter*/
						case 5: m_exifinfo.FocalplaneUnits = new CFRational(1, 254);  break;/* 1 micrometer*/
					}
						break;

						// Remaining cases contributed by: Volker C. Schoech <schoech(at)gmx(dot)de>

					case TAG_EXPOSURE_BIAS:
						m_exifinfo.ExposureBias = (CFRational) ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_METERING_MODE:
						m_exifinfo.MeteringMode = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_EXPOSURE_PROGRAM:
						m_exifinfo.ExposureProgram = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_ISO_EQUIVALENT:
						m_exifinfo.ISOequivalent = (int)ConvertAnyFormat(data, ValuePtr, Format);
						if ( m_exifinfo.ISOequivalent < 50 ) m_exifinfo.ISOequivalent *= 200;
						break;
					case TAG_COMPRESSION_LEVEL:
						m_exifinfo.CompressionLevel = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_XRESOLUTION:
						m_exifinfo.Xresolution = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_YRESOLUTION:
						m_exifinfo.Yresolution = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_THUMBNAIL_OFFSET:
						ThumbnailOffset = (uint)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_THUMBNAIL_LENGTH:
						ThumbnailSize = (uint)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case TAG_IMAGE_TITLE:
						m_exifinfo.imageTitle = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1)).Trim();
						break;
					case TAG_SOFTWARE:
						m_exifinfo.software = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1)).Trim();
						break;
					case 306: // TAG_IMAGE_CREATION_TIME:
						m_exifinfo.imageCreationTime = new string(ascii.GetChars(data, (int)ValuePtr, 19));
						break;
					case 34853: // GPS_OFFSET
						long SubdirStart;
						SubdirStart = OffsetBase + Get32u(data, ValuePtr);
						if (SubdirStart < OffsetBase || 
							SubdirStart > OffsetBase+ExifLength)
						{
							m_szLastError=@"Illegal subdirectory link";
							return false;
						}
						ProcessExifGPSDir(data, SubdirStart, OffsetBase, ExifLength, Tag);//, m_exifinfo, LastExifRefdP);

						break;
					case TAG_EXIF_OFFSET:
					case TAG_INTEROP_OFFSET:
					{
						SubdirStart = OffsetBase + Get32u(data, ValuePtr);
						if (SubdirStart < OffsetBase || 
							SubdirStart > OffsetBase+ExifLength)
						{
							m_szLastError=@"Illegal subdirectory link";
							return false;
						}
						ProcessExifDir(data, SubdirStart, OffsetBase, ExifLength, Tag);//, m_exifinfo, LastExifRefdP);
						break;
					} 
					case 531: // YCbCrPositioning
					{
						m_exifinfo.YCbCrPositioning = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					}
					case 36868: // DateTimeDigitized
						m_exifinfo.dateTimeDigitized = new string(ascii.GetChars(data, (int)ValuePtr, 19)).Trim();
						break;
					case 37121: // ComponentsConfiguration 
						for(int i=0;i<4;i++)
							m_exifinfo.componentsConfiguration[i] = data[ValuePtr+i];
						break;
					case 37500: // MakerNote 
						// Let's assume ASCII. Seems save enough
						m_exifinfo.makerNote = new byte[BytesCount];
						for(int i=0; i<BytesCount; i++)
							m_exifinfo.makerNote[i]= data[ValuePtr+i];
						break;
					case 40960: // FlashpixVersion
						m_exifinfo.flashpixVersion = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount)).Trim();
						break;
					case 40961: // ColorSpace
						m_exifinfo.colorSpace = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41728:
						m_exifinfo.fileSource = data[ValuePtr];
						break;
					case 41729: // SceneType
						m_exifinfo.sceneType = data[ValuePtr];
						break;
					case 41985: // CustomRendered
						m_exifinfo.customRendered = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41986: // ExposureMode 
						m_exifinfo.exposureMode = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41987: // WhiteBalance
						m_exifinfo.whiteBalance = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41988: // DigitalZoomRatio
						m_exifinfo.digitalZoomRatio = (CFRational)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41990:
						m_exifinfo.sceneCaptureType = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41991:
						m_exifinfo.gainControl = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 1: // InteroperabilityIndex  
						if(parentTag == 40965) // Interop 
						{
							m_exifinfo.interoperabilityIndex  = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1)).Trim();
						}
						else 
						{
							goto default;
						}
						break;
					case 2:
					{
						if(parentTag == 40965) // Interop 
						{
							m_exifinfo.interoperabilityVersion = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount)).Trim();
						}
						else 
						{
							goto default;
						}
						break;
					}
					case 41992:
						m_exifinfo.contrast = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41993: // Saturation
						m_exifinfo.saturation = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 37384: // Light Source
						m_exifinfo.lightSource = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41994: // Sharpness
						m_exifinfo.sharpness = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 50341: // EXIF_TAG_UNKNOWN_C4A5
						Console.WriteLine("Found unknown tag C4A5");
						break;
					case 259:
						m_exifinfo.compression = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 4097: // EXIF_TAG_RELATED_IMAGE_WIDTH
						m_exifinfo.relatedImageWidth = System.Convert.ToInt64( ConvertAnyFormat(data, ValuePtr, Format));
						break;
					case 4098: // EXIF_TAG_RELATED_IMAGE_Height
                        m_exifinfo.relatedImageHeight = System.Convert.ToInt64(ConvertAnyFormat(data, ValuePtr, Format));
						break;
					case 41495:
						m_exifinfo.sensingMethod = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41989:
						m_exifinfo.focalLengthIn35mm = (uint)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 41996: // SubjectDistanceRange 
						m_exifinfo.subjectDistanceRange = (int)ConvertAnyFormat(data, ValuePtr, Format);
						break;
					case 42016:
						m_exifinfo.imageUniqueID = new string(ascii.GetChars(data, (int)ValuePtr, BytesCount-1)).Trim();
						break;
					default:
					{
						string parent="unknown section " + parentTag;
						switch(parentTag) 
						{
							case 40965: 
								parent = "Interoperability section";
								break;
							case 34665:
								parent = "EXIF section";
								break;
						}
						Console.WriteLine("Unknown tag: {0} in "+ parent, Tag);
						break;
					}
				}
			}

		{
			/* In addition to linking to subdirectories via exif tags,
					 there's also a potential link to another directory at the end
					 of each directory.  This has got to be the result of a
					 committee!  
				*/
			long SubdirStart;
			long Offset;
			Offset = Get16u(data, dirStart+2+12*NumDirEntries);
			if (Offset!=0)
			{
				SubdirStart = OffsetBase + Offset;
				if (SubdirStart < OffsetBase 
					|| SubdirStart > OffsetBase+ExifLength)
				{
					m_szLastError=@"Illegal subdirectory link";
					return false;
				}
				ProcessExifDir(data, SubdirStart, OffsetBase, ExifLength, parentTag);//, m_exifinfo, LastExifRefdP);
				//		Console.WriteLine("Can't handle exif subdirs");
				//					ProcessExifDir(&data[SubdirStart], ExifLength);
			}
		}


			if ( (ThumbnailSize !=0) && (ThumbnailOffset !=0))
			{
				if (ThumbnailSize + ThumbnailOffset <= ExifLength)
				{
					/* The thumbnail pointer appears to be valid.  Store it. */

					// m_exifinfo.ThumbnailPointer = OffsetBase + ThumbnailOffset;
					m_exifinfo.ThumbnailPointer = new byte[ThumbnailSize];
					Array.Copy(data, OffsetBase + ThumbnailOffset, m_exifinfo.ThumbnailPointer, 0, ThumbnailSize);
					;
					//	m_exifinfo.ThumbnailSize = ThumbnailSize;
				}
			}

			return true;
		}
		////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------
				 Evaluate number, be it int, CFRational, or float from directory.
			--------------------------------------------------------------------------*/
		Object ConvertAnyFormat(byte[] ValuePtr, long off, int Format)
		{
			object Value;
			Value = 0;

			switch(Format)
			{
				case FMT_SBYTE:     Value = (sbyte)ValuePtr[off];  break;
				case FMT_BYTE:      Value = (byte)ValuePtr[off];        break;

				case FMT_USHORT:    Value = Get16u(ValuePtr, off);          break;
				case FMT_ULONG:     Value = Get32u(ValuePtr, off);          break;

				case FMT_UCFRational:
				case FMT_SCFRational: 
				{
					int Num,Den;
					Num = Get32s(ValuePtr, off);
					Den = Get32s(ValuePtr, off+4);
					Value =  new CFRational(Num, Den);
					//					if (Den == 0)
					//					{
					//						Value = 0;
					//					}
					//					else
					//					{
					//						Value = (double)Num/Den;
					//					}
					break;
				}

				case FMT_SSHORT:    Value = (ushort)Get16u(ValuePtr, off);  break;
				case FMT_SLONG:     Value = Get32s(ValuePtr, off);                break;

					/* Not sure if this is correct (never seen float used in Exif format)
						 */
				case FMT_SINGLE:    Value = 0; break;//(double)*(float *)ValuePtr;      break;
				case FMT_DOUBLE:    Value = 0; break;//*(double *)ValuePtr;             break;
			}
			return Value;
		}
		//////////////////////////////////////////////////////////////////////////////////
		//void Cexif::process_COM (readonly unsigned char * Data, int length)
		//{
		//    int ch;
		//    char Comment[MAX_COMMENT+1];
		//    int nch;
		//    int a;
		//
		//    nch = 0;
		//
		//    if (length > MAX_COMMENT) length = MAX_COMMENT; // Truncate if it won't fit in our structure.
		//
		//    for (a=2;a<length;a++){
		//        ch = Data[a];
		//
		//        if (ch == '\r' && Data[a+1] == '\n') continue; // Remove cr followed by lf.
		//
		//        if ((ch>=0x20) || ch == '\n' || ch == '\t'){
		//            Comment[nch++] = (char)ch;
		//        }else{
		//            Comment[nch++] = '?';
		//        }
		//    }
		//
		//    Comment[nch] = '\0'; // Null terminate
		//
		//    //if (ShowTags) printf("COM marker comment: %s\n",Comment);
		//
		//    strcpy(m_exifinfo.Comments,Comment);
		//}
		//////////////////////////////////////////////////////////////////////////////////
		void process_SOFn (byte[] Data, int marker)
		{
			int data_precision, num_components;

			data_precision = Data[2];
			m_exifinfo.Height = Get16m(Data,3);
			m_exifinfo.Width = Get16m(Data,5);
			num_components = Data[7];

			if (num_components == 3)
			{
				m_exifinfo.IsColor = 1;
			}
			else
			{
				m_exifinfo.IsColor = 0;
			}

			m_exifinfo.Process = marker;

			//if (ShowTags) printf("JPEG image is %uw * %uh, %d color components, %d bits per sample\n",
			//               ImageInfo.Width, ImageInfo.Height, num_components, data_precision);
		}
		////////////////////////////////////////////////////////////////////////////////


		/// <summary>
		/// 
		/// </summary>
//		public static byte[] getThumbnail(Image image)
//		{
//			PropertyItem[]  parr= image.PropertyItems;
//			byte[] res=null;
//
//			foreach( System.Drawing.Imaging.PropertyItem p in parr )
//			{
//				if(p.Id == 0x501B) 
//				{
//					res= p.Value; 
//				}
//
//			}
//
//			return res;
//		}

	}
}

