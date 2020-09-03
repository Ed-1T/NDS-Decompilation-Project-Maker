namespace NDSDecompilationProjectMaker
{
	partial class MainForm
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
			this.MainTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.ControlsTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.FileTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.OutputBrowseButton = new System.Windows.Forms.Button();
			this.OutputGroupBox = new System.Windows.Forms.GroupBox();
			this.OutputTextBox = new System.Windows.Forms.TextBox();
			this.InputBrowseButton = new System.Windows.Forms.Button();
			this.InputGroupBox = new System.Windows.Forms.GroupBox();
			this.InputTextBox = new System.Windows.Forms.TextBox();
			this.ControlsGroupBox = new System.Windows.Forms.GroupBox();
			this.GenerationTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.SymbolsAsFunctionsCheckBox = new System.Windows.Forms.CheckBox();
			this.AutoNameCheckBox = new System.Windows.Forms.CheckBox();
			this.FillBssCheckBox = new System.Windows.Forms.CheckBox();
			this.BssValueTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.DefaultValueLabel = new System.Windows.Forms.Label();
			this.BssFillValueChooser = new System.Windows.Forms.NumericUpDown();
			this.GenerateButton = new System.Windows.Forms.Button();
			this.OpenSymbolsButton = new System.Windows.Forms.Button();
			this.ProgressBarTblLytPanel = new System.Windows.Forms.TableLayoutPanel();
			this.ProgressBar = new System.Windows.Forms.ProgressBar();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.ForceDSiIORegistersCheckBox = new System.Windows.Forms.CheckBox();
			this.GenerateIORegistersCheckBox = new System.Windows.Forms.CheckBox();
			this.MainTblLytPanel.SuspendLayout();
			this.ControlsTblLytPanel.SuspendLayout();
			this.FileTblLytPanel.SuspendLayout();
			this.OutputGroupBox.SuspendLayout();
			this.InputGroupBox.SuspendLayout();
			this.ControlsGroupBox.SuspendLayout();
			this.GenerationTblLytPanel.SuspendLayout();
			this.BssValueTblLytPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BssFillValueChooser)).BeginInit();
			this.ProgressBarTblLytPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTblLytPanel
			// 
			this.MainTblLytPanel.ColumnCount = 1;
			this.MainTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTblLytPanel.Controls.Add(this.ControlsTblLytPanel, 0, 0);
			this.MainTblLytPanel.Controls.Add(this.ProgressBarTblLytPanel, 0, 1);
			this.MainTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTblLytPanel.Location = new System.Drawing.Point(0, 0);
			this.MainTblLytPanel.Name = "MainTblLytPanel";
			this.MainTblLytPanel.RowCount = 2;
			this.MainTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.MainTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.MainTblLytPanel.Size = new System.Drawing.Size(702, 207);
			this.MainTblLytPanel.TabIndex = 0;
			// 
			// ControlsTblLytPanel
			// 
			this.ControlsTblLytPanel.ColumnCount = 2;
			this.ControlsTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54F));
			this.ControlsTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46F));
			this.ControlsTblLytPanel.Controls.Add(this.FileTblLytPanel, 0, 0);
			this.ControlsTblLytPanel.Controls.Add(this.ControlsGroupBox, 1, 0);
			this.ControlsTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ControlsTblLytPanel.Location = new System.Drawing.Point(3, 3);
			this.ControlsTblLytPanel.Name = "ControlsTblLytPanel";
			this.ControlsTblLytPanel.RowCount = 1;
			this.ControlsTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ControlsTblLytPanel.Size = new System.Drawing.Size(696, 138);
			this.ControlsTblLytPanel.TabIndex = 1;
			// 
			// FileTblLytPanel
			// 
			this.FileTblLytPanel.ColumnCount = 2;
			this.FileTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.FileTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.FileTblLytPanel.Controls.Add(this.OutputBrowseButton, 1, 1);
			this.FileTblLytPanel.Controls.Add(this.OutputGroupBox, 0, 1);
			this.FileTblLytPanel.Controls.Add(this.InputBrowseButton, 1, 0);
			this.FileTblLytPanel.Controls.Add(this.InputGroupBox, 0, 0);
			this.FileTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FileTblLytPanel.Location = new System.Drawing.Point(3, 3);
			this.FileTblLytPanel.Name = "FileTblLytPanel";
			this.FileTblLytPanel.RowCount = 2;
			this.FileTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.FileTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.FileTblLytPanel.Size = new System.Drawing.Size(369, 132);
			this.FileTblLytPanel.TabIndex = 0;
			// 
			// OutputBrowseButton
			// 
			this.OutputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputBrowseButton.Location = new System.Drawing.Point(298, 87);
			this.OutputBrowseButton.Name = "OutputBrowseButton";
			this.OutputBrowseButton.Size = new System.Drawing.Size(68, 23);
			this.OutputBrowseButton.TabIndex = 4;
			this.OutputBrowseButton.Text = "Browse...";
			this.OutputBrowseButton.UseVisualStyleBackColor = true;
			this.OutputBrowseButton.Click += new System.EventHandler(this.OutputBrowseButton_Click);
			// 
			// OutputGroupBox
			// 
			this.OutputGroupBox.Controls.Add(this.OutputTextBox);
			this.OutputGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutputGroupBox.Location = new System.Drawing.Point(3, 69);
			this.OutputGroupBox.Name = "OutputGroupBox";
			this.OutputGroupBox.Size = new System.Drawing.Size(289, 60);
			this.OutputGroupBox.TabIndex = 0;
			this.OutputGroupBox.TabStop = false;
			this.OutputGroupBox.Text = "Output folder";
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputTextBox.Location = new System.Drawing.Point(6, 21);
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.Size = new System.Drawing.Size(277, 20);
			this.OutputTextBox.TabIndex = 3;
			this.OutputTextBox.TextChanged += new System.EventHandler(this.OutputTextBox_TextChanged);
			// 
			// InputBrowseButton
			// 
			this.InputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.InputBrowseButton.Location = new System.Drawing.Point(298, 21);
			this.InputBrowseButton.Name = "InputBrowseButton";
			this.InputBrowseButton.Size = new System.Drawing.Size(68, 23);
			this.InputBrowseButton.TabIndex = 2;
			this.InputBrowseButton.Text = "Browse...";
			this.InputBrowseButton.UseVisualStyleBackColor = true;
			this.InputBrowseButton.Click += new System.EventHandler(this.InputBrowseButton_Click);
			// 
			// InputGroupBox
			// 
			this.InputGroupBox.Controls.Add(this.InputTextBox);
			this.InputGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InputGroupBox.Location = new System.Drawing.Point(3, 3);
			this.InputGroupBox.Name = "InputGroupBox";
			this.InputGroupBox.Size = new System.Drawing.Size(289, 60);
			this.InputGroupBox.TabIndex = 0;
			this.InputGroupBox.TabStop = false;
			this.InputGroupBox.Text = "Input ROM";
			// 
			// InputTextBox
			// 
			this.InputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.InputTextBox.Location = new System.Drawing.Point(6, 21);
			this.InputTextBox.Name = "InputTextBox";
			this.InputTextBox.Size = new System.Drawing.Size(277, 20);
			this.InputTextBox.TabIndex = 1;
			this.InputTextBox.TextChanged += new System.EventHandler(this.InputTextBox_TextChanged);
			// 
			// ControlsGroupBox
			// 
			this.ControlsGroupBox.Controls.Add(this.GenerationTblLytPanel);
			this.ControlsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ControlsGroupBox.Location = new System.Drawing.Point(378, 3);
			this.ControlsGroupBox.Name = "ControlsGroupBox";
			this.ControlsGroupBox.Size = new System.Drawing.Size(315, 132);
			this.ControlsGroupBox.TabIndex = 1;
			this.ControlsGroupBox.TabStop = false;
			this.ControlsGroupBox.Text = "Generation";
			// 
			// GenerationTblLytPanel
			// 
			this.GenerationTblLytPanel.ColumnCount = 3;
			this.GenerationTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.71429F));
			this.GenerationTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.71429F));
			this.GenerationTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.57143F));
			this.GenerationTblLytPanel.Controls.Add(this.SymbolsAsFunctionsCheckBox, 1, 0);
			this.GenerationTblLytPanel.Controls.Add(this.AutoNameCheckBox, 0, 0);
			this.GenerationTblLytPanel.Controls.Add(this.FillBssCheckBox, 0, 1);
			this.GenerationTblLytPanel.Controls.Add(this.BssValueTblLytPanel, 1, 1);
			this.GenerationTblLytPanel.Controls.Add(this.GenerateButton, 0, 2);
			this.GenerationTblLytPanel.Controls.Add(this.OpenSymbolsButton, 1, 2);
			this.GenerationTblLytPanel.Controls.Add(this.ForceDSiIORegistersCheckBox, 2, 1);
			this.GenerationTblLytPanel.Controls.Add(this.GenerateIORegistersCheckBox, 2, 0);
			this.GenerationTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenerationTblLytPanel.Location = new System.Drawing.Point(3, 16);
			this.GenerationTblLytPanel.Name = "GenerationTblLytPanel";
			this.GenerationTblLytPanel.RowCount = 3;
			this.GenerationTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.GenerationTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.GenerationTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.GenerationTblLytPanel.Size = new System.Drawing.Size(309, 113);
			this.GenerationTblLytPanel.TabIndex = 0;
			// 
			// SymbolsAsFunctionsCheckBox
			// 
			this.SymbolsAsFunctionsCheckBox.AutoSize = true;
			this.SymbolsAsFunctionsCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SymbolsAsFunctionsCheckBox.Location = new System.Drawing.Point(113, 3);
			this.SymbolsAsFunctionsCheckBox.Name = "SymbolsAsFunctionsCheckBox";
			this.SymbolsAsFunctionsCheckBox.Size = new System.Drawing.Size(104, 31);
			this.SymbolsAsFunctionsCheckBox.TabIndex = 5;
			this.SymbolsAsFunctionsCheckBox.Text = "Define symbols as functions";
			this.SymbolsAsFunctionsCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutoNameCheckBox
			// 
			this.AutoNameCheckBox.AutoSize = true;
			this.AutoNameCheckBox.Checked = true;
			this.AutoNameCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.AutoNameCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoNameCheckBox.Location = new System.Drawing.Point(3, 3);
			this.AutoNameCheckBox.Name = "AutoNameCheckBox";
			this.AutoNameCheckBox.Size = new System.Drawing.Size(104, 31);
			this.AutoNameCheckBox.TabIndex = 0;
			this.AutoNameCheckBox.Text = "Auto name sections";
			this.AutoNameCheckBox.UseVisualStyleBackColor = true;
			// 
			// FillBssCheckBox
			// 
			this.FillBssCheckBox.AutoSize = true;
			this.FillBssCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FillBssCheckBox.Location = new System.Drawing.Point(3, 40);
			this.FillBssCheckBox.Name = "FillBssCheckBox";
			this.FillBssCheckBox.Size = new System.Drawing.Size(104, 31);
			this.FillBssCheckBox.TabIndex = 1;
			this.FillBssCheckBox.Text = "Fill bss sections";
			this.FillBssCheckBox.UseVisualStyleBackColor = true;
			this.FillBssCheckBox.CheckedChanged += new System.EventHandler(this.FillBssCheckBox_CheckedChanged);
			// 
			// BssValueTblLytPanel
			// 
			this.BssValueTblLytPanel.ColumnCount = 2;
			this.BssValueTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.BssValueTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.BssValueTblLytPanel.Controls.Add(this.DefaultValueLabel, 1, 0);
			this.BssValueTblLytPanel.Controls.Add(this.BssFillValueChooser, 0, 0);
			this.BssValueTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BssValueTblLytPanel.Location = new System.Drawing.Point(113, 40);
			this.BssValueTblLytPanel.Name = "BssValueTblLytPanel";
			this.BssValueTblLytPanel.RowCount = 1;
			this.BssValueTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.BssValueTblLytPanel.Size = new System.Drawing.Size(104, 31);
			this.BssValueTblLytPanel.TabIndex = 2;
			// 
			// DefaultValueLabel
			// 
			this.DefaultValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultValueLabel.AutoSize = true;
			this.DefaultValueLabel.Location = new System.Drawing.Point(55, 2);
			this.DefaultValueLabel.Name = "DefaultValueLabel";
			this.DefaultValueLabel.Size = new System.Drawing.Size(46, 26);
			this.DefaultValueLabel.TabIndex = 0;
			this.DefaultValueLabel.Text = "Default value";
			// 
			// BssFillValueChooser
			// 
			this.BssFillValueChooser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BssFillValueChooser.Enabled = false;
			this.BssFillValueChooser.Hexadecimal = true;
			this.BssFillValueChooser.Location = new System.Drawing.Point(3, 5);
			this.BssFillValueChooser.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.BssFillValueChooser.Name = "BssFillValueChooser";
			this.BssFillValueChooser.Size = new System.Drawing.Size(46, 20);
			this.BssFillValueChooser.TabIndex = 1;
			// 
			// GenerateButton
			// 
			this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateButton.Location = new System.Drawing.Point(3, 82);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = new System.Drawing.Size(104, 23);
			this.GenerateButton.TabIndex = 3;
			this.GenerateButton.Text = "Generate";
			this.GenerateButton.UseVisualStyleBackColor = true;
			this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
			// 
			// OpenSymbolsButton
			// 
			this.OpenSymbolsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenSymbolsButton.Location = new System.Drawing.Point(113, 82);
			this.OpenSymbolsButton.Name = "OpenSymbolsButton";
			this.OpenSymbolsButton.Size = new System.Drawing.Size(104, 23);
			this.OpenSymbolsButton.TabIndex = 4;
			this.OpenSymbolsButton.Text = "Load symbols";
			this.OpenSymbolsButton.UseVisualStyleBackColor = true;
			this.OpenSymbolsButton.Click += new System.EventHandler(this.OpenSymbolsButton_Click);
			// 
			// ProgressBarTblLytPanel
			// 
			this.ProgressBarTblLytPanel.ColumnCount = 1;
			this.ProgressBarTblLytPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ProgressBarTblLytPanel.Controls.Add(this.ProgressBar, 0, 1);
			this.ProgressBarTblLytPanel.Controls.Add(this.StatusLabel, 0, 0);
			this.ProgressBarTblLytPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressBarTblLytPanel.Location = new System.Drawing.Point(3, 147);
			this.ProgressBarTblLytPanel.Name = "ProgressBarTblLytPanel";
			this.ProgressBarTblLytPanel.RowCount = 2;
			this.ProgressBarTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ProgressBarTblLytPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ProgressBarTblLytPanel.Size = new System.Drawing.Size(696, 57);
			this.ProgressBarTblLytPanel.TabIndex = 2;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressBar.Location = new System.Drawing.Point(3, 31);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = new System.Drawing.Size(690, 23);
			this.ProgressBar.TabIndex = 0;
			// 
			// StatusLabel
			// 
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusLabel.Location = new System.Drawing.Point(3, 0);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(690, 28);
			this.StatusLabel.TabIndex = 1;
			// 
			// DSiIORegistersCheckBox
			// 
			this.ForceDSiIORegistersCheckBox.AutoSize = true;
			this.ForceDSiIORegistersCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForceDSiIORegistersCheckBox.Location = new System.Drawing.Point(223, 40);
			this.ForceDSiIORegistersCheckBox.Name = "DSiIORegistersCheckBox";
			this.ForceDSiIORegistersCheckBox.Size = new System.Drawing.Size(83, 31);
			this.ForceDSiIORegistersCheckBox.TabIndex = 6;
			this.ForceDSiIORegistersCheckBox.Text = "Force DSi IO registers";
			this.ForceDSiIORegistersCheckBox.UseVisualStyleBackColor = true;
			// 
			// GenerateIORegistersCheckBox
			// 
			this.GenerateIORegistersCheckBox.AutoSize = true;
			this.GenerateIORegistersCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenerateIORegistersCheckBox.Location = new System.Drawing.Point(223, 3);
			this.GenerateIORegistersCheckBox.Name = "GenerateIORegistersCheckBox";
			this.GenerateIORegistersCheckBox.Size = new System.Drawing.Size(83, 31);
			this.GenerateIORegistersCheckBox.TabIndex = 7;
			this.GenerateIORegistersCheckBox.Text = "Generate IO registers";
			this.GenerateIORegistersCheckBox.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(702, 207);
			this.Controls.Add(this.MainTblLytPanel);
			this.Location = new System.Drawing.Point(630, 246);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(1920, 246);
			this.MinimumSize = new System.Drawing.Size(718, 246);
			this.Name = "MainForm";
			this.Text = "Nintendo DS Decompilation Project Maker";
			this.MainTblLytPanel.ResumeLayout(false);
			this.ControlsTblLytPanel.ResumeLayout(false);
			this.FileTblLytPanel.ResumeLayout(false);
			this.OutputGroupBox.ResumeLayout(false);
			this.OutputGroupBox.PerformLayout();
			this.InputGroupBox.ResumeLayout(false);
			this.InputGroupBox.PerformLayout();
			this.ControlsGroupBox.ResumeLayout(false);
			this.GenerationTblLytPanel.ResumeLayout(false);
			this.GenerationTblLytPanel.PerformLayout();
			this.BssValueTblLytPanel.ResumeLayout(false);
			this.BssValueTblLytPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BssFillValueChooser)).EndInit();
			this.ProgressBarTblLytPanel.ResumeLayout(false);
			this.ProgressBarTblLytPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel MainTblLytPanel;
		private System.Windows.Forms.TableLayoutPanel ControlsTblLytPanel;
		private System.Windows.Forms.Button InputBrowseButton;
		private System.Windows.Forms.TableLayoutPanel FileTblLytPanel;
		private System.Windows.Forms.GroupBox InputGroupBox;
		private System.Windows.Forms.Button OutputBrowseButton;
		private System.Windows.Forms.GroupBox OutputGroupBox;
		private System.Windows.Forms.GroupBox ControlsGroupBox;
		private System.Windows.Forms.TableLayoutPanel GenerationTblLytPanel;
		private System.Windows.Forms.TableLayoutPanel ProgressBarTblLytPanel;
		public System.Windows.Forms.ProgressBar ProgressBar;
		public System.Windows.Forms.Label StatusLabel;
		public System.Windows.Forms.TextBox InputTextBox;
		public System.Windows.Forms.TextBox OutputTextBox;
		public System.Windows.Forms.CheckBox AutoNameCheckBox;
		public System.Windows.Forms.CheckBox FillBssCheckBox;
		private System.Windows.Forms.TableLayoutPanel BssValueTblLytPanel;
		private System.Windows.Forms.Label DefaultValueLabel;
		private System.Windows.Forms.Button GenerateButton;
		public System.Windows.Forms.NumericUpDown BssFillValueChooser;
		private System.Windows.Forms.Button OpenSymbolsButton;
		public System.Windows.Forms.CheckBox SymbolsAsFunctionsCheckBox;
		public System.Windows.Forms.CheckBox ForceDSiIORegistersCheckBox;
		public System.Windows.Forms.CheckBox GenerateIORegistersCheckBox;
	}
}

