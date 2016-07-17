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
	/// Summary description for IMosaicWorkerListener.
	/// </summary>
	public interface IMosaicWorkerListener
	{
		void DrawTile(MosaicWorker mw, Bitmap tile, Rectangle r1, Rectangle r2);
		void MosaicFinished(MosaicWorker mw) ;
	}
}
