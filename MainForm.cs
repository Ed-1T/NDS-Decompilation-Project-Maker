using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace NDSDecompilationProjectMaker
{
	public partial class MainForm : Form
	{
		public NDS.ROM rom;
		public DecompilationProjectMaker maker;

		public MainForm()
		{
			InitializeComponent();

			Util.Main = this;
			Util.ROMPath = "";
			Util.SymbolsPath = "";

			Status.InitVisualFeedback();
			Status.UpdatePathStatusText();

			// duplicate symbols form testing
			{
				//DuplicatesFixWindow dfw = new DuplicatesFixWindow();

				//Dictionary<uint, List<string>> symbols = new Dictionary<uint, List<string>>();
				//List<uint> duplicates = new List<uint>();

				//duplicates.Add(0x2000000);
				//duplicates.Add(0x1234567);
				//duplicates.Add(0x2000800);

				//symbols[0x2000000] = new List<string>();
				//symbols[0x2000000].Add("InsertThing");
				//symbols[0x2000000].Add("InsertValue");
				//symbols[0x2000000].Add("StartTimer");

				//symbols[0x1234567] = new List<string>();
				//symbols[0x1234567].Add("ExternalRegister");
				//symbols[0x1234567].Add("ExtIO");
				//symbols[0x1234567].Add("UndefinedExceptionPort");

				//symbols[0x2000800] = new List<string>();
				//symbols[0x2000800].Add("_start");
				//symbols[0x2000800].Add("_entry");
				//symbols[0x2000800].Add("StartARM9");
				//symbols[0x2000800].Add("ARM9EntryPoint");

				//dfw.Init(symbols, duplicates);
				//dfw.ShowDialog(this);
			}

			maker = new DecompilationProjectMaker();
		}

		private void InputBrowseButton_Click(object sender, EventArgs e)
		{
			Status.StopVisualFeedback();
			if (Util.InputPath != string.Empty)
				InputFileDialog.InitialDirectory = Util.InputPath;
			if (InputFileDialog.ShowDialog() == DialogResult.OK)
			{
				InputTextBox.Text = InputFileDialog.FileName;
				Status.UpdatePath();
			}
		}
		private void OutputBrowseButton_Click(object sender, EventArgs e)
		{
			Status.StopVisualFeedback();
			if (OutputFolderDialog.ShowDialog() == DialogResult.OK)
			{
				OutputTextBox.Text = OutputFolderDialog.SelectedPath;
				Status.UpdatePath();
			}
		}
		private void OpenSymbolsButton_Click(object sender, EventArgs e)
		{
			Status.StopVisualFeedback();
			if (Util.InputPath != string.Empty)
				SymbolsFileDialog.InitialDirectory = Util.InputPath;
			if (SymbolsFileDialog.ShowDialog() == DialogResult.OK)
			{
				Util.SymbolsPath = SymbolsFileDialog.FileName;
			}
		}

		private void GenerateButton_Click(object sender, EventArgs e)
		{
			Status.StopVisualFeedback();
			if (Status.CanGenerate())
			{
				Util.UpdateSettings();

				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();

				try
				{
					// create directory if it doesn't exist
					Directory.CreateDirectory(Util.OutputPath);

					rom = new NDS.ROM();
					if (rom.IsValid())
					{ 
						rom.Init();

						//MemorySection[] binaries = rom.GetBinarySections();
						MemorySection[] arm9 = rom.GetARM9BinarySection();
						MemorySection[] arm7 = rom.GetARM7BinarySection();
						MemorySection[] overlays = rom.GetOverlaySections();

						maker.CreateXmlDocument();

						// maker.CreateStandardMemorySections(binaries);

						maker.CreateStandardMemorySections(arm9);
						// arm7 likes to overwrite other things so 
						// I'll just make it an overlay
						maker.CreateARM7MemorySections(arm7);
						maker.CreateOverlayMemorySections(overlays);
						maker.FillMemoryGaps();
						maker.CreateSectionManual("shared_wram", 0x3000000, 0x1000000);

						maker.CreateSectionManual("io_ports", 0x4000000, 0x1000000);
						maker.CreateSectionManual("palette", 0x5000000, 0x1000000);
						maker.CreateSectionManual("vram", 0x6000000, 0x1000000);
						maker.CreateSectionManual("oam", 0x7000000, 0x1000000);
						maker.CreateSectionManual("oam", 0x7000000, 0x1000000);

						maker.DefineCommonSymbols();
						maker.DefineCommonFunctions();
						maker.DefineFunctionsFromFile(Util.SymbolsPath);

						maker.Save();
						rom.Close();

						stopwatch.Stop();
						long elapsedTime = stopwatch.ElapsedMilliseconds;
						Status.SetStatusText(string.Format("Done! [{0:d} ms]", elapsedTime));

					}
					else
					{
						Status.SetStatusText("Invalid ROM! (bad nintendo logo)");
						Status.StartVisualFeedback();
					}
				}
				catch (Exception ex)
				{
					Status.SetStatusText(ex.Message);
					Status.StartVisualFeedback();
				}
			}
			else 
			{
				Status.StartVisualFeedback();
			}
		}

		private void InputTextBox_TextChanged(object sender, EventArgs e)
		{
			Status.UpdatePathStatusText();
		}
		private void OutputTextBox_TextChanged(object sender, EventArgs e)
		{
			Status.UpdatePathStatusText();
		}

		private void FillBssCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (FillBssCheckBox.Checked)
			{
				BssFillValueChooser.Enabled = true;
			}
			else
			{
				BssFillValueChooser.Enabled = false;
			}
		}
	}
}
