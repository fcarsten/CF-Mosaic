/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Text;

namespace org.carsten
{
	public class ExifInfo 
	{
		Encoding ascii = Encoding.ASCII;

		public string  Version;
		public string  CameraMake  ;
		public string  CameraModel  ;
		public string  DateTime     ;
		public int   Height, Width;
		public int   orientation =-1;
		public int   IsColor;
		public int   Process;
		public int   FlashUsed;
		public CFRational FocalLength;
		public CFRational ExposureTime;
		public CFRational ApertureFNumber;
		public CFRational Aperture;
		public CFRational Distance;
		public CFRational shutterSpeed;
		public float CCDWidth;
		public CFRational ExposureBias;
		public int   MeteringMode;
		public int   ExposureProgram;
		public int   ISOequivalent;
		public CFRational   CompressionLevel;
		public CFRational FocalplaneXRes;
		public CFRational FocalplaneYRes;
		public CFRational FocalplaneUnits;
		public CFRational Xresolution;
		public CFRational Yresolution;
		public CFRational ResolutionUnit;
		public CFRational Brightness;
		public string Comments;
		public string imageTitle="";
		public string software="";
		public string imageCreationTime = "";
		public int sceneCaptureType =-1;
		public int gainControl=-1;
		public int YCbCrPositioning= -1; 
		public string dateTimeDigitized= null;
		public int colorSpace = -1; // ushort
		public string interoperabilityIndex= null;
		public int whiteBalance= -1; // SHORT
		public CFRational digitalZoomRatio;
		public int exposureMode= -1; // short
		public int customRendered = -1; // short
		public byte sceneType= 0;
		public byte fileSource = 0;
		public byte[] makerNote = null;
		public string flashpixVersion = null;
		public byte[] componentsConfiguration= new byte[4];
		public string interoperabilityVersion=null;
		public int contrast = -1;
		public int lightSource = -1;
		public int saturation = -1;
		public int sharpness = -1;
		public int compression=-1;
		public long relatedImageWidth=-1;
		public long relatedImageHeight=-1;
		public int sensingMethod=-1;
		public long focalLengthIn35mm=-1L;
		public int subjectDistanceRange=-1;
		public string imageUniqueID=null;
		public byte[] gpsVersionId=null;
		public string gpsLattitudeRef=null;
		public CFRational[] gpsLattitude=null;
		public string gpsLongitudeRef=null;
		public CFRational[] gpsLongitude=null;
        public short gpsAltitudeRef=-1;
		public CFRational gpsAltitude;
		public CFRational[] gpsTimeStamp=null;
		public string gpsTimeStampText 
		{
			get 
			{
				if(gpsTimeStamp==null) return null;
				return gpsTimeStamp[0].ToString() +":" +
					gpsTimeStamp[1].ToString()+":"+gpsTimeStamp[2].ToString();
			}
		}
		public string gpsAltitudeRefText 
		{
			get 
			{
				switch(subjectDistanceRange) 
				{
					case -1: return null;
					case 0: return "Above Sea Level";
					case 1: return "Below Sea Level";
					default: return "Reserved value: "+subjectDistanceRange;
				}
			}
		}
		public string subjectDistanceRangeText 
		{
			get 
			{
				switch(subjectDistanceRange) 
				{
					case -1: return null;
					case 0: return "unknown";
					case 1: return "Close view";
					case 2: return "Macro";
					case 3: return "Distant view";
					default: return "Reserved value: "+subjectDistanceRange;
				}
			}
		}

		public string sensingMethodText 
		{
			get 
			{
				switch(sensingMethod) 
				{
					case -1: return null;
					case 1: return "Not defined";
					case 2: return "One-chip color area sensor";
					case 3: return "Two-chip color area sensor";
					case 4: return "Three-chip color area sensor";
					case 5: return "Color sequential area sensor";
					case 7: return "Trilinear sensor";
					case 8: return "Color sequential linear sensor";
					default: return "Reserved value: "+sensingMethod;
				}
			}
		}

		public string compressionText 
		{
			get 
			{
				switch(compression) 
				{
					case -1: return null;
					case 1: return "uncompressed";
					case 6: return "JPEG compression (thumbnails only) ";
					default: return "Reserved value: "+compression;
				}
			}
		}
		public string sharpnessText 
		{
			get 
			{
				switch(sharpness) 
				{
					case -1: return null;
					case 0: return "Normal";
					case 1: return "Soft";
					case 2: return "Hard";
					default: return "Reserved value: "+sharpness;
				}
			}
		}

		public string saturationText 
		{
			get 
			{
				switch(saturation) 
				{
					case -1: return null;
					case 0: return "Normal";
					case 1: return "Low saturation";
					case 2: return "High saturation";
					default: return "Reserved value: "+saturation;
				}
			}
		}

		public string lightSourceText 
		{
			get 
			{
				switch(lightSource) 
				{
					case -1: return null;
					case 0: return "unknown";
					case 1: return "Daylight";
					case 2: return "Fluorescent";
					case 3: return "Tungsten (incandescent light)";
					case 4: return "Flash";
					case 9: return "Fine weather";
					case 10: return "Cloudy weather";
					case 11: return "Shade";
					case 12: return "Daylight fluorescent (D 5700 – 7100K)";
					case 13: return "Day white fluorescent (N 4600 – 5400K)";
					case 14: return "Cool white fluorescent (W 3900 – 4500K)";
					case 15: return "White fluorescent (WW 3200 – 3700K)";
					case 17: return "Standard light A";
					case 18: return "Standard light B";
					case 19: return "Standard light C";
					case 20: return "D55";
					case 21: return "D65";
					case 22: return "D75";
					case 23: return "D50";
					case 24: return "ISO studio tungsten";
					case 255: return "other light source";
					default: return "Reserved value: "+lightSource;
				}
			}
		}

		public string contrastText 
		{
			get 
			{
				switch(contrast) 
				{
					case -1: return null;
					case 0: return "Normal";
					case 1: return "Soft";
					case 2: return "Hard";
					default: return "Reserved value: "+contrast;
				}
			}
		}
		public string sceneCaptureTypeText 
		{
			get 
			{
				switch(sceneCaptureType) 
				{
					case -1: return null;
					case 0: return "Standard";
					case 1: return "Landscape";
					case 2: return "Portrait";
					case 3: return "Night scene";
					default: return "Reserved value: "+sceneCaptureType;
				}
			}
		}
		
		public string gainControlText 
		{
			get 
			{
				switch(gainControl) 
				{
					case -1: return null;
					case 0: return "None";
					case 1: return "Low gain up";
					case 2: return "High gain up";
					case 3: return "Low gain down";
					case 4: return "High gain down";
					default: return "Reserved value: "+gainControl;
				}
			}
		}
					
		public string YCbCrPositioningText 
		{
			get 
			{
				switch(YCbCrPositioning) 
				{
					case -1: return "not specified";
					case 0: return "centered";
					case 1: return "co-sited";
					default: return "Reserved value: "+YCbCrPositioning;
				}
			}
		}

		public string whiteBalanceText 
		{
			get 
			{
				switch(whiteBalance) 
				{
					case -1: return null;
					case 0: return "Auto white balance";
					case 1: return "Manual white balance";
					default : return "Reserved value: "+whiteBalance;
				}
			}
		}

		public string exposureModeText 
		{
			get 
			{
				switch(exposureMode) 
				{
					case -1: return null;
					case 0: return "Auto exposure";
					case 1: return "Manual exposure";
					case 2: return "Auto bracket";
					default : return "Reserved value: "+exposureMode;
				}
			}
		}

		public string customRenderedText 
		{
			get 
			{
				switch(customRendered) 
				{
					case -1: return null;
					case 0: return "Normal process";
					case 1: return "Custom process";
					default : return "Reserved value: "+customRendered;
				}
			}
		}

		public string sceneTypeText 
		{
			get 
			{
				switch(sceneType)
				{
					case 0: return null;
					case 1: return "A directly photographed image";
					default : return "Reserved Value: " +sceneType;
				}
			}
		}

		public string fileSourceText 
		{
			get 
			{
				switch(fileSource) 
				{
					case 0: return null;
					case 3: return "DSC";
					default: return "Reserved Value: " + fileSource;
				}
			}
		}
		public string colorSpaceText 
		{
			get 
			{
				switch(colorSpace) 
				{
					case 0: return null;
					case 1: return "sRGB";
					case 0xFFFF: return "Uncalibrated";
					default: return "Reserved value: " + colorSpace;
				}
			}
		}
		public string makerNoteText 
		{
			get 
			{
				if(makerNote==null) return "N/A";
				return new string(ascii.GetChars(makerNote, 0, makerNote.Length ));
			}
		}
		public string componentsConfigurationText 
		{
			get 
			{
				string res="";
				for(int i=0; i< 4; i++)
				{
					switch(componentsConfiguration[i])
					{
						case 0:
							res+= "-";
							break;
						case 1:
							res+= "Y";
							break;
						case 2:
							res += "Cb";
							break;
						case 3:
							res+= "Cr";
							break;
						case 4:
							res += "R";
							break;
						case 5:
							res+= "G";
							break;
						case 6:
							res+= "B";
							break;
						default:
							res+= "?";
							break;
					}
				}
				return res;
			}
		}

		public byte[] ThumbnailPointer;  /* Pointer at the thumbnail */
		//unsigned ThumbnailSize;     /* Size of thumbnail. */

		public bool  IsExif;
	} 
}
