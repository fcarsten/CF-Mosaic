/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace org.carsten
{

	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MosaicMain : System.Windows.Forms.Form, IMosaicWorkerListener
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem FileMenu;
		private System.Windows.Forms.MenuItem OpenMenu;
		private System.Windows.Forms.MenuItem SaveAsMenu;
		private System.Windows.Forms.MenuItem ExitMenu;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.MenuItem View;
		private System.Windows.Forms.MenuItem ScaleToFit;
		private System.Windows.Forms.MenuItem ScaleOneToOne;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem MosaicMenu;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label OpacityLabel;
		private System.Windows.Forms.NumericUpDown opacityUpDownControl;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveImageDialog;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.StatusBarPanel etaStatusPanel;
		private System.Windows.Forms.StatusBarPanel numTilesStatusPanel;
		private System.Windows.Forms.MenuItem addImagesMenu;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.CheckBox continuousUpdateCheckbox;

		ColorMatrix clrMatrix = null;
		private System.Windows.Forms.StatusBarPanel databaseStatus;
		private System.Windows.Forms.MenuItem clearRepositoryMenu;
		private System.Windows.Forms.MenuItem clearDatabaseMenu;
		private System.Windows.Forms.NumericUpDown mosaicWidthUpDown;
		private System.Windows.Forms.NumericUpDown mosaicHeightUpDown;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button mosaicCancelButton;
		private System.Windows.Forms.NumericUpDown mosaicRepeatRateUpDown;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.ToolTip toolTipToolBar;
		private System.Windows.Forms.HelpProvider cfMosaicHelp;
		private System.Windows.Forms.MenuItem menuRotate;
		private System.Windows.Forms.MenuItem menuRotate90;
		private System.Windows.Forms.MenuItem menuRotate270;
		private System.Windows.Forms.MenuItem menuRotate180;
		ImageAttributes imgAttributes =
			new ImageAttributes();

		public void updateDatabaseStatus() 
		{
			databaseStatus.Text = "Database: " + MosaicWorker.getDataBaseSize(this) + " Tiles";
		}

		public MosaicMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
this.pictureBox1.AllowDrop=true;
this.panel1.AllowDrop=true;

			Cursor = Cursors.WaitCursor;
			mosaicWidthUpDown.Maximum = Harvester.TILE_WIDTH;
			mosaicHeightUpDown.Maximum = Harvester.TILE_HEIGHT;

			updateDatabaseStatus();

			clrMatrix = new ColorMatrix(ptsArray);
			clrMatrix.Matrix33 = 0.01f * (float)opacityUpDownControl.Value;
			imgAttributes.SetColorMatrix(clrMatrix,
				ColorMatrixFlag.Default,
				ColorAdjustType.Bitmap);


			// Create an ImageAttributes object

			// Set color matrix of imageAttributes
			drawTileDelegate = new DelegateDrawTile(this.drawTile);
			delegateMosaicFinished = new DelegateMosaicFinished(this.DoMosaicFinished);

			Cursor = Cursors.Default;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.FileMenu = new System.Windows.Forms.MenuItem();
			this.OpenMenu = new System.Windows.Forms.MenuItem();
			this.SaveAsMenu = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.ExitMenu = new System.Windows.Forms.MenuItem();
			this.View = new System.Windows.Forms.MenuItem();
			this.ScaleToFit = new System.Windows.Forms.MenuItem();
			this.ScaleOneToOne = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.MosaicMenu = new System.Windows.Forms.MenuItem();
			this.menuRotate = new System.Windows.Forms.MenuItem();
			this.menuRotate90 = new System.Windows.Forms.MenuItem();
			this.menuRotate270 = new System.Windows.Forms.MenuItem();
			this.menuRotate180 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.addImagesMenu = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.clearDatabaseMenu = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.clearRepositoryMenu = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.mosaicRepeatRateUpDown = new System.Windows.Forms.NumericUpDown();
			this.mosaicCancelButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.mosaicHeightUpDown = new System.Windows.Forms.NumericUpDown();
			this.mosaicWidthUpDown = new System.Windows.Forms.NumericUpDown();
			this.continuousUpdateCheckbox = new System.Windows.Forms.CheckBox();
			this.opacityUpDownControl = new System.Windows.Forms.NumericUpDown();
			this.OpacityLabel = new System.Windows.Forms.Label();
			this.saveImageDialog = new System.Windows.Forms.SaveFileDialog();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.databaseStatus = new System.Windows.Forms.StatusBarPanel();
			this.etaStatusPanel = new System.Windows.Forms.StatusBarPanel();
			this.numTilesStatusPanel = new System.Windows.Forms.StatusBarPanel();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.toolTipToolBar = new System.Windows.Forms.ToolTip(this.components);
			this.cfMosaicHelp = new System.Windows.Forms.HelpProvider();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mosaicRepeatRateUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mosaicHeightUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mosaicWidthUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityUpDownControl)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.databaseStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.etaStatusPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numTilesStatusPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlText;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(496, 328);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragEnter);
			this.pictureBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragDrop);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							this.FileMenu,
																																							this.View,
																																							this.menuItem1,
																																							this.menuItem3,
																																							this.menuItem7});
			// 
			// FileMenu
			// 
			this.FileMenu.Index = 0;
			this.FileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																						 this.OpenMenu,
																																						 this.SaveAsMenu,
																																						 this.menuItem5,
																																						 this.ExitMenu});
			this.FileMenu.Text = "File";
			// 
			// OpenMenu
			// 
			this.OpenMenu.Index = 0;
			this.OpenMenu.Text = "Open";
			this.OpenMenu.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// SaveAsMenu
			// 
			this.SaveAsMenu.Index = 1;
			this.SaveAsMenu.Text = "Save As";
			this.SaveAsMenu.Click += new System.EventHandler(this.SaveAsMenu_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 2;
			this.menuItem5.Text = "-";
			// 
			// ExitMenu
			// 
			this.ExitMenu.Index = 3;
			this.ExitMenu.Text = "Exit";
			this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
			// 
			// View
			// 
			this.View.Index = 1;
			this.View.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																				 this.ScaleToFit,
																																				 this.ScaleOneToOne});
			this.View.Text = "View";
			// 
			// ScaleToFit
			// 
			this.ScaleToFit.Index = 0;
			this.ScaleToFit.Text = "Scale to fit";
			this.ScaleToFit.Click += new System.EventHandler(this.ScaleToFit_Click);
			// 
			// ScaleOneToOne
			// 
			this.ScaleOneToOne.Index = 1;
			this.ScaleOneToOne.Text = "Original Size";
			this.ScaleOneToOne.Click += new System.EventHandler(this.ScaleOneToOne_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 2;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							this.MosaicMenu,
																																							this.menuRotate});
			this.menuItem1.Text = "Mosaic";
			// 
			// MosaicMenu
			// 
			this.MosaicMenu.Index = 0;
			this.MosaicMenu.Text = "Create Mosaic";
			this.MosaicMenu.Click += new System.EventHandler(this.createMosaic);
			// 
			// menuRotate
			// 
			this.menuRotate.Index = 1;
			this.menuRotate.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							 this.menuRotate90,
																																							 this.menuRotate270,
																																							 this.menuRotate180});
			this.menuRotate.Text = "Rotate";
			// 
			// menuRotate90
			// 
			this.menuRotate90.Index = 0;
			this.menuRotate90.Text = "Rotate 90 (->)";
			this.menuRotate90.Click += new System.EventHandler(this.menuRotate90_Click);
			// 
			// menuRotate270
			// 
			this.menuRotate270.Index = 1;
			this.menuRotate270.Text = "Rotate 270 (<-)";
			this.menuRotate270.Click += new System.EventHandler(this.menuRotate270_Click);
			// 
			// menuRotate180
			// 
			this.menuRotate180.Index = 2;
			this.menuRotate180.Text = "Rotate 180";
			this.menuRotate180.Click += new System.EventHandler(this.menuRotate180_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 3;
			this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							this.addImagesMenu,
																																							this.menuItem10,
																																							this.clearDatabaseMenu,
																																							this.menuItem4,
																																							this.clearRepositoryMenu});
			this.menuItem3.Text = "Database";
			// 
			// addImagesMenu
			// 
			this.addImagesMenu.Index = 0;
			this.addImagesMenu.Text = "Add Images to Repository";
			this.addImagesMenu.Click += new System.EventHandler(this.addImagesMenu_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 1;
			this.menuItem10.Text = "-";
			// 
			// clearDatabaseMenu
			// 
			this.clearDatabaseMenu.Index = 2;
			this.clearDatabaseMenu.Text = "Recreate Database";
			this.clearDatabaseMenu.Click += new System.EventHandler(this.clearDatabaseMenu_Click_1);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 3;
			this.menuItem4.Text = "-";
			// 
			// clearRepositoryMenu
			// 
			this.clearRepositoryMenu.Index = 4;
			this.clearRepositoryMenu.Text = "Clear Image Repository";
			this.clearRepositoryMenu.Click += new System.EventHandler(this.clearDatabaseMenu_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 4;
			this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																							this.menuItem8,
																																							this.menuItem9});
			this.menuItem7.Text = "Help";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 0;
			this.menuItem8.Text = "Manual";
			this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 1;
			this.menuItem9.Text = "About";
			this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.Title = "Open Mosaic Source Image";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.Color.Black;
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Location = new System.Drawing.Point(0, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(624, 376);
			this.panel1.TabIndex = 1;
			this.panel1.Resize += new System.EventHandler(this.panel1_Resize);
			this.panel1.SizeChanged += new System.EventHandler(this.panel1_SizeChanged);
			this.panel1.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragEnter);
			this.panel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragDrop);
			
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.label3);
			this.panel2.Controls.Add(this.mosaicRepeatRateUpDown);
			this.panel2.Controls.Add(this.mosaicCancelButton);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.mosaicHeightUpDown);
			this.panel2.Controls.Add(this.mosaicWidthUpDown);
			this.panel2.Controls.Add(this.continuousUpdateCheckbox);
			this.panel2.Controls.Add(this.opacityUpDownControl);
			this.panel2.Controls.Add(this.OpacityLabel);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(624, 64);
			this.panel2.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(140, 12);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 12);
			this.label3.TabIndex = 10;
			this.label3.Text = "Min Tile Distance:";
			this.toolTipToolBar.SetToolTip(this.label3, "The minimum distance from its first use after which the same tile can be used aga" +
				"in.\nThe distance applies vertically and horizontally.");
			// 
			// mosaicRepeatRateUpDown
			// 
			this.mosaicRepeatRateUpDown.Location = new System.Drawing.Point(240, 8);
			this.mosaicRepeatRateUpDown.Name = "mosaicRepeatRateUpDown";
			this.mosaicRepeatRateUpDown.Size = new System.Drawing.Size(48, 20);
			this.mosaicRepeatRateUpDown.TabIndex = 9;
			this.mosaicRepeatRateUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.toolTipToolBar.SetToolTip(this.mosaicRepeatRateUpDown, "The minimum distance from its first use after which the same tile can be used aga" +
				"in.\nThe distance applies vertically and horizontally.");
			this.mosaicRepeatRateUpDown.Value = new System.Decimal(new int[] {
																																				 20,
																																				 0,
																																				 0,
																																				 0});
			this.mosaicRepeatRateUpDown.ValueChanged += new System.EventHandler(this.mosaicRepeatRateUpDown_ValueChanged);
			// 
			// mosaicCancelButton
			// 
			this.mosaicCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.mosaicCancelButton.Location = new System.Drawing.Point(544, 32);
			this.mosaicCancelButton.Name = "mosaicCancelButton";
			this.mosaicCancelButton.TabIndex = 8;
			this.mosaicCancelButton.Text = "Cancel";
			this.toolTipToolBar.SetToolTip(this.mosaicCancelButton, "Cancels the computation of the mosaic.\nAll tiles which have already been computed" +
				" will stay however.");
			this.mosaicCancelButton.Visible = false;
			this.mosaicCancelButton.Click += new System.EventHandler(this.mosaicCancelButton_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 12);
			this.label2.TabIndex = 7;
			this.label2.Text = "Tile Height:";
			this.toolTipToolBar.SetToolTip(this.label2, "The height of each mosaic tile.\nDisabled while a mosaic is being computed.");
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 12);
			this.label1.TabIndex = 6;
			this.label1.Text = "Tile Width:";
			this.toolTipToolBar.SetToolTip(this.label1, "The width of each mosaic tile.\nDisabled while a mosaic is being computed.");
			// 
			// mosaicHeightUpDown
			// 
			this.mosaicHeightUpDown.Location = new System.Drawing.Point(80, 32);
			this.mosaicHeightUpDown.Maximum = new System.Decimal(new int[] {
																																			 105,
																																			 0,
																																			 0,
																																			 0});
			this.mosaicHeightUpDown.Name = "mosaicHeightUpDown";
			this.mosaicHeightUpDown.Size = new System.Drawing.Size(44, 20);
			this.mosaicHeightUpDown.TabIndex = 5;
			this.mosaicHeightUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.mosaicHeightUpDown.Value = new System.Decimal(new int[] {
																																		 75,
																																		 0,
																																		 0,
																																		 0});
			// 
			// mosaicWidthUpDown
			// 
			this.mosaicWidthUpDown.Location = new System.Drawing.Point(80, 8);
			this.mosaicWidthUpDown.Maximum = new System.Decimal(new int[] {
																																			150,
																																			0,
																																			0,
																																			0});
			this.mosaicWidthUpDown.Name = "mosaicWidthUpDown";
			this.mosaicWidthUpDown.Size = new System.Drawing.Size(44, 20);
			this.mosaicWidthUpDown.TabIndex = 4;
			this.mosaicWidthUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.toolTipToolBar.SetToolTip(this.mosaicWidthUpDown, "The width of each mosaic tile.\nDisabled while a mosaic is being computed.");
			this.mosaicWidthUpDown.Value = new System.Decimal(new int[] {
																																		100,
																																		0,
																																		0,
																																		0});
			// 
			// continuousUpdateCheckbox
			// 
			this.continuousUpdateCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.continuousUpdateCheckbox.Location = new System.Drawing.Point(500, 4);
			this.continuousUpdateCheckbox.Name = "continuousUpdateCheckbox";
			this.continuousUpdateCheckbox.Size = new System.Drawing.Size(120, 24);
			this.continuousUpdateCheckbox.TabIndex = 3;
			this.continuousUpdateCheckbox.Text = "Continuous Update";
			this.toolTipToolBar.SetToolTip(this.continuousUpdateCheckbox, "If checked, each tile is drawn immediatley after it is computed. \nThis slows down" +
				" things, but you see what\'s happening.\nNote: You can always force a refresh by s" +
				"crolling or resizing the window.");
			this.continuousUpdateCheckbox.CheckedChanged += new System.EventHandler(this.continuousUpdateCheckbox_CheckedChanged);
			// 
			// opacityUpDownControl
			// 
			this.opacityUpDownControl.Location = new System.Drawing.Point(240, 32);
			this.opacityUpDownControl.Minimum = new System.Decimal(new int[] {
																																				 10,
																																				 0,
																																				 0,
																																				 0});
			this.opacityUpDownControl.Name = "opacityUpDownControl";
			this.opacityUpDownControl.Size = new System.Drawing.Size(48, 20);
			this.opacityUpDownControl.TabIndex = 2;
			this.opacityUpDownControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.toolTipToolBar.SetToolTip(this.opacityUpDownControl, "The transparency with which the tile is blended into the original image.\nValid va" +
				"lues are 10 to 100.\n  100: The tile totally replaces the background.\n   10: The " +
				"result is 10% tile and 90% background.");
			this.opacityUpDownControl.Value = new System.Decimal(new int[] {
																																			 95,
																																			 0,
																																			 0,
																																			 0});
			this.opacityUpDownControl.ValueChanged += new System.EventHandler(this.opacityUpDownControl_ValueChanged);
			// 
			// OpacityLabel
			// 
			this.OpacityLabel.Location = new System.Drawing.Point(160, 28);
			this.OpacityLabel.Name = "OpacityLabel";
			this.OpacityLabel.Size = new System.Drawing.Size(64, 23);
			this.OpacityLabel.TabIndex = 1;
			this.OpacityLabel.Text = "Opacity %:";
			this.OpacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolTipToolBar.SetToolTip(this.OpacityLabel, "The transparency with which the tile is blended into the original image.\nValid va" +
				"lues are 10 to 100.\n  100: The tile totally replaces the background.\n  10: The r" +
				"esult is 10% tile and 90% background.");
			// 
			// saveImageDialog
			// 
			this.saveImageDialog.DefaultExt = "jpg";
			this.saveImageDialog.Filter = "JPEG Image|*.jpg";
			this.saveImageDialog.Title = "Select Mosaic File Name";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 437);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																																								 this.databaseStatus,
																																								 this.etaStatusPanel,
																																								 this.numTilesStatusPanel});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(624, 16);
			this.statusBar.TabIndex = 3;
			this.statusBar.Text = " Status:";
			// 
			// databaseStatus
			// 
			this.databaseStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.databaseStatus.Text = "Database: 0 Tiles";
			this.databaseStatus.Width = 102;
			// 
			// etaStatusPanel
			// 
			this.etaStatusPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.etaStatusPanel.Text = "Time Remaining: 00:00";
			this.etaStatusPanel.Width = 131;
			// 
			// numTilesStatusPanel
			// 
			this.numTilesStatusPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.numTilesStatusPanel.Text = "Tiles: 0 of 0";
			this.numTilesStatusPanel.Width = 72;
			// 
			// toolTipToolBar
			// 
			this.toolTipToolBar.ShowAlways = true;
			// 
			// cfMosaicHelp
			// 
			this.cfMosaicHelp.HelpNamespace = "F:\\Documents and Settings\\Carsten\\Desktop\\program\\Mosaic1\\Mosaic1\\cf mosaic oh.ch" +
				"m";
			// 
			// MosaicMain
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(624, 453);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(444, 216);
			this.Name = "MosaicMain";
			this.Text = "CF Mosaic";
			this.Load += new System.EventHandler(this.MosaicMain_Load);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mosaicRepeatRateUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.mosaicHeightUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.mosaicWidthUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityUpDownControl)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.databaseStatus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.etaStatusPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numTilesStatusPanel)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MosaicMain());
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			OpenFile();
		}

		private void OpenFile() 
		{
			openFileDialog.ShowDialog();

			String file= openFileDialog.FileName;
			if(file ==null || file.Length==0)
				return;

			loadImage(file);
//			Bitmap image =  new Bitmap(file);
//			pictureBox1.Image=image;
		}
		
		private void ExitMenu_Click(object sender, System.EventArgs e)
		{
			//Application.Exit();
			Close();
		}

		private void MosaicMain_Load(object sender, System.EventArgs e)
		{
		Console.WriteLine("Bah");
		}

		private void ScaleToFit_Click(object sender, System.EventArgs e)
		{
			panel1.AutoScroll= false;
			pictureBox1.SizeMode=PictureBoxSizeMode.StretchImage;
			pictureBox1.Width = panel1.Width;
			pictureBox1.Height = panel1.Height;
		}

		private void ScaleOneToOne_Click(object sender, System.EventArgs e)
		{
			panel1.AutoScroll= true;
			pictureBox1.SizeMode=PictureBoxSizeMode.AutoSize;
			pictureBox1.Width = panel1.Width;
			pictureBox1.Height = panel1.Height;		
		}

		private void panel1_SizeChanged(object sender, System.EventArgs e)
		{
			updatePictureBoxDimensions();
		}

		private void panel1_Resize(object sender, System.EventArgs e)
		{
			updatePictureBoxDimensions();
		}

		private void updatePictureBoxDimensions() 
		{
			if(pictureBox1.Image != null &&
				pictureBox1.SizeMode==PictureBoxSizeMode.StretchImage) 
			{
				double wRatio = panel1.Width / (double)pictureBox1.Image.Width;
				double hRatio = panel1.Height / (double)pictureBox1.Image.Height;
				double ratio = Math.Min(wRatio, hRatio);
				double newW = ratio * (double)pictureBox1.Image.Width;
				double newH = ratio * (double)pictureBox1.Image.Height;
				double newX = (panel1.Width - newW)/2.0;
				double newY = (panel1.Height - newH)/2.0;
				pictureBox1.SetBounds((int)newX, (int)newY, (int)newW, (int)newH);
				pictureBox1.Refresh();

//				pictureBox1.Width = panel1.Width;
//				pictureBox1.Height = panel1.Height;
			}
		}

		private void createMosaic(object sender, System.EventArgs e)
		{
			if(pictureBox1.Image==null)
				OpenFile();
			if(pictureBox1.Image==null)
				return;

			enableControls(false);

//			MosaicWorker mw= getMosaicWorker();
//			mw.setImage(new Bitmap(pictureBox1.Image));
//			mw.mosaicWidth = (float)mosaicWidthUpDown.Value;
//			mw.mosaicHeight = (float)mosaicHeightUpDown.Value;
//			mw.setRepeatRate((int)mosaicRepeatRateUpDown.Value);

			if(mosaicInfo!=null) mosaicInfo.Dispose();
			mosaicInfo= new CFMosaicInfo(pictureBox1.Image, (int)mosaicWidthUpDown.Value, (int)mosaicHeightUpDown.Value, (int)mosaicRepeatRateUpDown.Value);

			mosaicCancelButton.Visible=true;
			mosaicCancelButton.Enabled=true;
			try 
			{
				getMosaicWorker().Run(mosaicInfo); 
			} 
			catch( MosaicException ex) 
			{
				MessageBox.Show(this, ex.Message, @"Mosaic Error");
			} 
		}

		private MosaicWorker mosaicWorker=null;
		MosaicWorker getMosaicWorker() 
		{
			if(mosaicWorker==null) mosaicWorker=new MosaicWorker(this);
			return mosaicWorker;
		}

		private CFMosaicInfo mosaicInfo = null;

		delegate void DelegateMosaicFinished();
	private DelegateMosaicFinished delegateMosaicFinished;

		public void DoMosaicFinished() 
		{
			enableControls(true);
			mosaicCancelButton.Hide();
			pictureBox1.Refresh();
		}

		private DelegateDrawTile drawTileDelegate ;
		delegate void DelegateDrawTile(MosaicWorker mw, Bitmap tile, Rectangle r1, Rectangle r2);

		public void DrawTile(MosaicWorker mw, Bitmap tile, Rectangle r1, Rectangle r2)
		{
			Object[] args = {mw, tile, r1, r2};
			BeginInvoke(drawTileDelegate, args);
		}

		public void MosaicFinished(MosaicWorker mw) 
		{
			Invoke(delegateMosaicFinished, new Object[] {});

		}

		private void enableControls(bool f) 
		{
			MosaicMenu.Enabled=f;
			addImagesMenu.Enabled=f;
			clearDatabaseMenu.Enabled=f;
			clearRepositoryMenu.Enabled=f;
			menuRotate.Enabled=f;
			mosaicWidthUpDown.Enabled=f;
			mosaicHeightUpDown.Enabled=f;
			panel1.AllowDrop = f;
			pictureBox1.AllowDrop = f;
		}

		private readonly float[][] ptsArray = 
{
	new float[] { 1, 0, 0, 0, 0},
	new float[] { 0, 1, 0, 0, 0},
	new float[] { 0, 0, 1, 0, 0},
	new float[] { 0, 0, 0, 1, 0},
	new float[] { 0, 0, 0, 0, 1}
};

		public void drawTile(MosaicWorker source, Bitmap tile, Rectangle r1, Rectangle r2)
		{
			etaStatusPanel.Text = source.etaText;
			numTilesStatusPanel.Text= source.tileToGoText;

			Image tmpImage = pictureBox1.Image;
			Graphics g = Graphics.FromImage(tmpImage);

			g.DrawImage(tile, r1, r2.X, r2.Y, r2.Width, r2.Height, GraphicsUnit.Pixel,imgAttributes );
			if(continuousUpdateCheckbox.Checked)
			{
				if(pictureBox1.SizeMode==PictureBoxSizeMode.StretchImage)
				{ 
					double imgW= pictureBox1.Image.Width;
					double imgH= pictureBox1.Image.Height;
					double boxW= pictureBox1.Width;
					double boxH= pictureBox1.Height;
					double ratioX = boxW/imgW;
					double ratioY= boxH/imgH;
					r1.X= (int)(r1.X*ratioX);
					r1.Width = (int)(r1.Width*ratioX+2);
					r1.Y = (int)(r1.Y*ratioY);
					r1.Height = (int)(ratioY*r1.Height+2);
				}
				pictureBox1.Invalidate(r1, false);
			}
			g.Dispose();
		}

		private void SaveAsMenu_Click(object sender, System.EventArgs e)
		{
			if(pictureBox1.Image == null)
			{
				MessageBox.Show(this, "You have to create a mosaic first!");
				return;
			}
			saveImageDialog.ShowDialog();

			String file= saveImageDialog.FileName;
			if(file != null && file != "")
				pictureBox1.Image.Save(file);
		}

		private void addImagesMenu_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog fd = new FolderBrowserDialog();
			fd.Description = @"Select a folder with images to add. Images in subfolders will also be added.";
			fd.ShowDialog(this);
			string path = fd.SelectedPath;
			if(path != null && path !="")
			{
				enableControls(false);
				Harvester.harvest(this, path);
				MosaicWorker.clearDatabase();
				updateDatabaseStatus();
				enableControls(true);
			}
		}

		private void clearDatabaseMenu_Click(object sender, System.EventArgs e)
		{
			DialogResult res= MessageBox.Show(this, "Are you sure you want to delete your current image repository?", "Confirm Clear Repository", MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if(res == DialogResult.Yes)
			{
				Console.WriteLine("Deleting repository");
				MosaicWorker.clearDatabase();
				MosaicWorker.clearImageRepository();
				updateDatabaseStatus();
			} 
			else 
			{
				Console.WriteLine("Not deleting");
			}

		}

		private void opacityUpDownControl_ValueChanged(object sender, System.EventArgs e)
		{
			clrMatrix.Matrix33 = 0.01f * (float)opacityUpDownControl.Value;
			imgAttributes.SetColorMatrix(clrMatrix,
				ColorMatrixFlag.Default,
				ColorAdjustType.Bitmap);
		}

		private void clearDatabaseMenu_Click_1(object sender, System.EventArgs e)
		{
			DialogResult res= MessageBox.Show(this, "Are you sure you want to recreate your current database?", "Confirm Recreate Database", MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if(res == DialogResult.Yes)
			{
				Console.WriteLine("Deleting database");
				MosaicWorker.clearDatabase();
				updateDatabaseStatus();
			} 
			else 
			{
				Console.WriteLine("Not deleting");
			}
		}

		private void mosaicCancelButton_Click(object sender, System.EventArgs e)
		{
			mosaicCancelButton.Enabled=false;

			if(getMosaicWorker() != null)
				getMosaicWorker().cancel(true);
		}

		private void mosaicRepeatRateUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			getMosaicWorker().setRepeatRate((int)mosaicRepeatRateUpDown.Value);
		}

		private void menuItem8_Click(object sender, System.EventArgs e)
		{
		Help.ShowHelp(this, this.cfMosaicHelp.HelpNamespace);
		}

		private void menuItem9_Click(object sender, System.EventArgs e)
		{
			new AboutBox().ShowDialog();
		}

		private void continuousUpdateCheckbox_CheckedChanged(object sender, System.EventArgs e)
		{
			pictureBox1.Refresh();
		}

		private void pictureBox1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

			foreach( string file in files )
			{
				if(file ==null || file.Length==0)
					continue;
				try 
				{
					loadImage(file);
					return;
				} 
				catch (Exception ex) 
				{
					Console.WriteLine("Could not load image: " +ex.Message);
				}
			}
		}

		private void loadImage(string fileName) 
		{
			Bitmap image =  new Bitmap(fileName);
			EXIFThumbnailExtractor exif = new EXIFThumbnailExtractor(null);
			exif.DecodeExif(fileName);
			RotateFlipType rft = RotateFlipType.RotateNoneFlipNone;

			switch(exif.getExifInfo().orientation) 
			{ 
				case 2:
					rft = RotateFlipType.RotateNoneFlipX;
					break;
				case 3:
					rft = RotateFlipType.Rotate180FlipNone;
					break;
				case 4:
					rft = RotateFlipType.RotateNoneFlipY;
					break;
				case 5:
					rft = RotateFlipType.Rotate90FlipX;
					break;
				case 6:
					rft = RotateFlipType.Rotate90FlipNone;
					break;
				case 7:
					rft = RotateFlipType.Rotate270FlipY;
					break;
				case 8:
					rft = RotateFlipType.Rotate270FlipNone;
					break;
			}

			if(rft!=RotateFlipType.RotateNoneFlipNone)
				image.RotateFlip(rft);

			Image oldImage = pictureBox1.Image;
			pictureBox1.Image=image;
			updatePictureBoxDimensions();
			if(oldImage!=null)
			oldImage.Dispose();
		}

		private void pictureBox1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if( e.Data.GetDataPresent(DataFormats.FileDrop, false) == true )
				e.Effect = DragDropEffects.All;
		}

		private void menuRotate90_Click(object sender, System.EventArgs e)
		{
			if(pictureBox1.Image!=null)
			{
				pictureBox1.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
				updatePictureBoxDimensions();
			}
		}

		private void menuRotate270_Click(object sender, System.EventArgs e)
		{
			if(pictureBox1.Image!=null)
			{
				pictureBox1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
				updatePictureBoxDimensions();
			}
		}

		private void menuRotate180_Click(object sender, System.EventArgs e)
		{
			if(pictureBox1.Image!=null)
			{
				pictureBox1.Image.RotateFlip(RotateFlipType.Rotate180FlipNone);
				updatePictureBoxDimensions();
			}
		}
	}
}
