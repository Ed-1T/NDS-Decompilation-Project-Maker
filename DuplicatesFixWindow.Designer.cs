namespace NDSDecompilationProjectMaker
{
	partial class DuplicatesFixWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DuplicatesTree = new System.Windows.Forms.TreeView();
			this.MainTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.ButtonsTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.PreviousButton = new System.Windows.Forms.Button();
			this.NextButton = new System.Windows.Forms.Button();
			this.HintLabel = new System.Windows.Forms.Label();
			this.MainTblLytPanel.SuspendLayout();
			this.ButtonsTblLytPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// DuplicatesTree
			// 
			this.DuplicatesTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DuplicatesTree.Location = new System.Drawing.Point(3, 3);
			this.DuplicatesTree.Name = "DuplicatesTree";
			this.DuplicatesTree.Size = new System.Drawing.Size(290, 212);
			this.DuplicatesTree.TabIndex = 0;
			this.DuplicatesTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DuplicatesTree_NodeMouseDoubleClick);
			// 
			// MainTblLytPanel
			// 
			this.MainTblLytPanel.ColumnCount = 1;
			this.MainTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTblLytPanel.Controls.Add(this.DuplicatesTree, 0, 0);
			this.MainTblLytPanel.Controls.Add(this.ButtonsTblLytPanel, 0, 1);
			this.MainTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTblLytPanel.Location = new System.Drawing.Point(0, 0);
			this.MainTblLytPanel.Name = "MainTblLytPanel";
			this.MainTblLytPanel.RowCount = 2;
			this.MainTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.MainTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.MainTblLytPanel.Size = new System.Drawing.Size(296, 273);
			this.MainTblLytPanel.TabIndex = 1;
			// 
			// ButtonsTblLytPanel
			// 
			this.ButtonsTblLytPanel.ColumnCount = 3;
			this.ButtonsTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.ButtonsTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.ButtonsTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.ButtonsTblLytPanel.Controls.Add(this.PreviousButton, 0, 0);
			this.ButtonsTblLytPanel.Controls.Add(this.NextButton, 2, 0);
			this.ButtonsTblLytPanel.Controls.Add(this.HintLabel, 1, 0);
			this.ButtonsTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ButtonsTblLytPanel.Location = new System.Drawing.Point(3, 221);
			this.ButtonsTblLytPanel.Name = "ButtonsTblLytPanel";
			this.ButtonsTblLytPanel.RowCount = 1;
			this.ButtonsTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ButtonsTblLytPanel.Size = new System.Drawing.Size(290, 49);
			this.ButtonsTblLytPanel.TabIndex = 1;
			// 
			// PreviousButton
			// 
			this.PreviousButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.PreviousButton.Enabled = false;
			this.PreviousButton.Image = global::NDSDecompilationProjectMaker.Properties.Resources.arrow_left;
			this.PreviousButton.Location = new System.Drawing.Point(3, 8);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = new System.Drawing.Size(33, 33);
			this.PreviousButton.TabIndex = 0;
			this.PreviousButton.UseVisualStyleBackColor = true;
			this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.NextButton.Enabled = false;
			this.NextButton.Image = global::NDSDecompilationProjectMaker.Properties.Resources.arrow_right;
			this.NextButton.Location = new System.Drawing.Point(254, 8);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = new System.Drawing.Size(33, 33);
			this.NextButton.TabIndex = 1;
			this.NextButton.UseVisualStyleBackColor = true;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// HintLabel
			// 
			this.HintLabel.AutoSize = true;
			this.HintLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HintLabel.Location = new System.Drawing.Point(61, 0);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = new System.Drawing.Size(168, 49);
			this.HintLabel.TabIndex = 2;
			this.HintLabel.Text = "Double-click to edit";
			this.HintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// DuplicatesFixWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(296, 273);
			this.Controls.Add(this.MainTblLytPanel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(256, 268);
			this.Name = "DuplicatesFixWindow";
			this.Text = "Fixing duplicate symbols [0/0]";
			this.MainTblLytPanel.ResumeLayout(false);
			this.ButtonsTblLytPanel.ResumeLayout(false);
			this.ButtonsTblLytPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView DuplicatesTree;
		private System.Windows.Forms.TableLayoutPanel MainTblLytPanel;
		private System.Windows.Forms.TableLayoutPanel ButtonsTblLytPanel;
		private System.Windows.Forms.Button PreviousButton;
		private System.Windows.Forms.Button NextButton;
		private System.Windows.Forms.Label HintLabel;
	}
}