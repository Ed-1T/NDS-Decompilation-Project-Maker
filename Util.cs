using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NDSDecompilationProjectMaker
{
	public static class Util
	{
		public static MainForm Main;

		// paths
		private static string DecompressorPath
		{
			get => Properties.Settings.Default.outputPath + @"\blz.exe";
		}

		public static string ROMPath
		{
			get => Properties.Settings.Default.romPath;
			set
			{
				Properties.Settings.Default.romPath = value;
				Properties.Settings.Default.Save();
			}
		}
		public static string SymbolsPath
		{
			get => Properties.Settings.Default.symbolsPath;
			set
			{
				Properties.Settings.Default.symbolsPath = value;
				Properties.Settings.Default.Save();
			}
		}
		public static string InputPath
		{
			get => Properties.Settings.Default.inputPath;
			set
			{
				Properties.Settings.Default.inputPath = value;
				Properties.Settings.Default.Save();
			}
		}
		public static string OutputPath
		{
			get => Properties.Settings.Default.outputPath;
			set
			{
				Properties.Settings.Default.outputPath = value;
				Properties.Settings.Default.Save();
			}
		}

		// settings
		public static bool FillBSS;
		public static byte FillValue;
		public static bool AutoNameSections;
		public static bool SymbolsAsFunctions;

		public static void UpdateSettings()
		{
			FillBSS = Main.FillBssCheckBox.Checked;
			FillValue = (byte)Main.BssFillValueChooser.Value;
			AutoNameSections = Main.AutoNameCheckBox.Checked;
			SymbolsAsFunctions = Main.SymbolsAsFunctionsCheckBox.Checked;
		}

		// decompressor
		public static void CreateDecompressor()
		{
			File.WriteAllBytes(Util.DecompressorPath, Properties.Resources.blz);
		}
		public static bool CallDecompressor(string args)
		{
			Process process = new Process();
			process.StartInfo.FileName = Util.DecompressorPath;
			process.StartInfo.Arguments = args;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			Console.WriteLine("decompressor called:");
			Console.WriteLine("{0:s} {1:s}", Util.DecompressorPath, args);
			Console.WriteLine(output);

			return process.ExitCode == 0;
		}
		public static void DeleteDecompressor()
		{
			File.Delete(Util.DecompressorPath);
		}

		// structure reading
		public static T ReadStruct<T>(BinaryReader reader, int size)
		{
			byte[] raw = reader.ReadBytes(size);
			IntPtr heap = Marshal.AllocHGlobal(size);

			Marshal.Copy(raw, 0, heap, size);
			T res = (T)Marshal.PtrToStructure(heap, typeof(T));
			Marshal.FreeHGlobal(heap);

			return res;
		}
		public static T ConvertToStruct<T>(byte[] raw, int size)
		{
			IntPtr heap = Marshal.AllocHGlobal(size);

			Marshal.Copy(raw, 0, heap, size);
			T res = (T)Marshal.PtrToStructure(heap, typeof(T));
			Marshal.FreeHGlobal(heap);

			return res;
		}
	}
}
