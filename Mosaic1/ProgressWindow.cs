/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;


namespace MWA.Progress
{
	/// <summary>
	/// Summary description for ProgressWindow.
	/// </summary>
	public class ProgressWindow : System.Windows.Forms.Form, IProgressCallback
	{
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.ProgressBar progressBar;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public delegate void SetTextInvoker(String text);
		public delegate void IncrementInvoker( int val );
		public delegate void StepToInvoker( int val );
		public delegate void RangeInvoker( int minimum, int maximum );

		private String titleRoot = "";
		private System.Threading.ManualResetEvent initEvent = new System.Threading.ManualResetEvent(false);
		private System.Threading.ManualResetEvent abortEvent = new System.Threading.ManualResetEvent(false);
		private System.Windows.Forms.Label progressText2;
		private bool requiresClose = true;
		private System.Windows.Forms.Label timeLabel;

		private object[] userData = null;

		public void setUserData(object[] data) 
		{
			userData= data;
		}

		public object[] getUserData() 
		{
			return userData;
		}

		private Form mainWindow=null;
		public ProgressWindow(Form mainWindow)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.mainWindow = mainWindow;
		}

		private bool done = false;
		#region Implementation of IProgressCallback

		public void showErrorMessage(string text) 
		{
			initEvent.WaitOne();
			GracefulInvoke(new SetTextInvoker( DoShowErrorMessage), new object[] {text});				
		}
		
		public void DoShowErrorMessage(String text) 
		{
			MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>
		/// Call this method from the worker thread to initialize
		/// the progress meter.
		/// </summary>
		/// <param name="minimum">The minimum value in the progress range (e.g. 0)</param>
		/// <param name="maximum">The maximum value in the progress range (e.g. 100)</param>
		public void Begin( int minimum, int maximum )
		{
			initEvent.WaitOne();
			done = false;
			Invoke( new RangeInvoker( DoBegin ), new object[] { minimum, maximum } );
		}

		/// <summary>
		/// Call this method from the worker thread to initialize
		/// the progress callback, without setting the range
		/// </summary>
		public void Begin()
		{
			initEvent.WaitOne();
			done = false;
			Invoke( new MethodInvoker( DoBegin ) );
		}

		/// <summary>
		/// Call this method from the worker thread to reset the range in the progress callback
		/// </summary>
		/// <param name="minimum">The minimum value in the progress range (e.g. 0)</param>
		/// <param name="maximum">The maximum value in the progress range (e.g. 100)</param>
		/// <remarks>You must have called one of the Begin() methods prior to this call.</remarks>
		public void SetRange( int minimum, int maximum )
		{
			initEvent.WaitOne();
			GracefulInvoke( new RangeInvoker( DoSetRange ), new object[] { minimum, maximum } );
		}

		/// <summary>
		/// Call this method from the worker thread to update the progress text.
		/// </summary>
		/// <param name="text">The progress text to display</param>
		public void SetText( String text )
		{
			GracefulInvoke( new SetTextInvoker(DoSetText), new object[] { text } );
		}

		public void SetText2( String text )
		{
			GracefulInvoke( new SetTextInvoker(DoSetText2), new object[] { text } );
		}

		public void GracefulInvoke(Delegate method, object[] args) 
		{
			try 
			{
				Invoke(method, args);
			} 
			catch (InvalidOperationException) 
			{
				// ignore.
			}
		}

		public void GracefulInvoke(Delegate method) 
		{
			try 
			{
				Invoke(method);
			} 
			catch (InvalidOperationException) 
			{
				// ignore.
			}
		}

		/// <summary>
		/// Call this method from the worker thread to increase the progress counter by a specified value.
		/// </summary>
		/// <param name="val">The amount by which to increment the progress indicator</param>
		public void Increment( int val )
		{
			GracefulInvoke( new IncrementInvoker( DoIncrement ), new object[] { val } );
		}

		/// <summary>
		/// Call this method from the worker thread to step the progress meter to a particular value.
		/// </summary>
		/// <param name="val"></param>
		public void StepTo( int val )
		{
			GracefulInvoke( new StepToInvoker( DoStepTo ), new object[] { val } );
		}

		
		/// <summary>
		/// If this property is true, then you should abort work
		/// </summary>
		public bool IsAborting
		{
			get
			{
				return abortEvent.WaitOne( 0, false );
			}
		}

		/// <summary>
		/// Call this method from the worker thread to finalize the progress meter
		/// </summary>
		public void End()
		{
			initEvent.WaitOne();

			if( requiresClose )
			{
				GracefulInvoke( new MethodInvoker( DoEnd ) );
			}
		}
		#endregion

		#region Implementation members invoked on the owner thread
		private void DoSetText( String text )
		{
			label.Text = text;
		}

		private void DoSetText2( String text )
		{
			progressText2.Text = text;
		}

		private void DoIncrement( int val )
		{
			progressBar.Increment( val );
			UpdateStatusText();
		}

		private void DoStepTo( int val )
		{
			progressBar.Value = val;
			UpdateStatusText();
		}

		private void DoBegin( int minimum, int maximum )
		{
			DoBegin();
			DoSetRange( minimum, maximum );
		}

		DateTime startTime = DateTime.Now;

		private void DoBegin()
		{
			cancelButton.Enabled = true;
			ControlBox = true;
			DateTime startTime = DateTime.Now;
		}

		public int GetRangeMax() 
		{
			initEvent.WaitOne();
			return progressBar.Maximum;
		}

		public int GetCurrentCount() 
		{
			initEvent.WaitOne();
			return progressBar.Value;
		}

		private void DoSetRange( int minimum, int maximum )
		{
			progressBar.Minimum = minimum;
			progressBar.Maximum = maximum;
			progressBar.Value = minimum;
			titleRoot = Text;
		}

		private void DoEnd()
		{
			done = true;
			Close();
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Handles the form load, and sets an event to ensure that
		/// intialization is synchronized with the appearance of the form.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad( e );
			ControlBox = false;
			initEvent.Set();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Handler for 'Close' clicking
		/// </summary>
		/// <param name="e"></param>
//		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
//		{
//			requiresClose = false;
//			AbortWork();
//			base.OnClosing( e );
//		}
		#endregion
		
		#region Implementation Utilities
		/// <summary>
		/// Utility function that formats and updates the title bar text
		/// </summary>
		private void UpdateStatusText()
		{
			//
			// Avoids division by zero and we don't want to show the dialog anyway.
			//
			if(progressBar.Maximum - progressBar.Minimum==0)
				return;

			int gone = progressBar.Value - progressBar.Minimum;

			Text = titleRoot + String.Format( " - {0}% complete", (progressBar.Value * 100 ) / (progressBar.Maximum - progressBar.Minimum) );


			int toGo = progressBar.Maximum - progressBar.Value;

			TimeSpan duration = DateTime.Now - startTime;
			if(gone==0) gone=1;
			double tpt =  duration.TotalMilliseconds / (double)(1000.0 *gone);

			long timeLeft = (long)(tpt*toGo);//Math.Max((long)(tpt*toGo), timeLeft);
			string etaText = "" + (timeLeft/60) + ":" + 
				(timeLeft % 60 >= 10 ? "" : "0")+(timeLeft%60);
//			tileToGoText = "Tiles: "+ toGo + " of " + (toGo+gone);
			timeLabel.Text = etaText;

		}
		
		/// <summary>
		/// Utility function to terminate the thread
		/// </summary>
		private void AbortWork()
		{
			abortEvent.Set();
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.label = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.progressText2 = new System.Windows.Forms.Label();
			this.timeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(52, 84);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(248, 23);
			this.progressBar.TabIndex = 1;
			// 
			// label
			// 
			this.label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label.Location = new System.Drawing.Point(0, 44);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(392, 32);
			this.label.TabIndex = 0;
			this.label.Text = "Starting operation...";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Enabled = false;
			this.cancelButton.Location = new System.Drawing.Point(310, 84);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			// 
			// progressText2
			// 
			this.progressText2.Dock = System.Windows.Forms.DockStyle.Top;
			this.progressText2.Location = new System.Drawing.Point(0, 0);
			this.progressText2.Name = "progressText2";
			this.progressText2.Size = new System.Drawing.Size(392, 36);
			this.progressText2.TabIndex = 0;
			// 
			// timeLabel
			// 
			this.timeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.timeLabel.Location = new System.Drawing.Point(8, 88);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(40, 16);
			this.timeLabel.TabIndex = 3;
			this.timeLabel.Text = "000:00";
			this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ProgressWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(392, 114);
			this.Controls.Add(this.timeLabel);
			this.Controls.Add(this.progressText2);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.label);
			this.MaximizeBox = false;
			this.Name = "ProgressWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ProgressWindow";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ProgressWindow_Closing);
			this.SizeChanged += new System.EventHandler(this.ProgressWindow_SizeChanged);
			this.ResumeLayout(false);

		}
		#endregion

		private string cancelMessage = null;

		public void setCancelMessage(string msg) 
		{
			cancelMessage = msg;
		}

		private void ProgressWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(done||cancelMessage==null||cancelMessage.Length==0) 
			{
				requiresClose = false;
				AbortWork();
			}
			else 
			{
				DialogResult res= MessageBox.Show(this, cancelMessage, "Confirm Cancel", MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

				if(res == DialogResult.No)
				{
					Console.WriteLine("Cancel cancel");
					e.Cancel= true;
				} 
				else 
				{
					Console.WriteLine("Really cancel");
					requiresClose = false;
					AbortWork();
					//				base.OnClosing( e );
				}
			}
		}

		private void ProgressWindow_SizeChanged(object sender, System.EventArgs e)
		{
//			if(mainWindow!=null)
//			{
//				this.mainWindow.WindowState= WindowState;
//			}
		}

	}
}
