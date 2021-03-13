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
		private static string DecompressorDir => Properties.Settings.Default.path_temp;
		private static string DecompressorPath => Path.Combine(Properties.Settings.Default.path_temp, "blz.exe");
		public static string ROMPath;
		public static string SymbolsPath;
		public static string SymbolsDir
		{
			get => Properties.Settings.Default.path_symbols;
			set
			{
				Properties.Settings.Default.path_symbols = value;
				Properties.Settings.Default.Save();
			}
		}
		public static string InputDir
		{
			get => Properties.Settings.Default.path_in;
			set
			{
				Properties.Settings.Default.path_in = value;
				Properties.Settings.Default.Save();
			}
		}
		public static string OutputDir
		{
			get => Properties.Settings.Default.path_out;
			set
			{
				Properties.Settings.Default.path_out = value;
				Properties.Settings.Default.Save();
			}
		}

		// settings
		public static bool FillBSS;
		public static byte FillValue;
		public static bool AutoNameSections;
		public static bool SymbolsAsFunctions;
		public static bool GenerateIORegisters;
		public static bool ForceDSiIORegisters;

		public static void UpdateSettings()
		{
			FillBSS = Main.FillBssCheckBox.Checked;
			FillValue = (byte)Main.BssFillValueChooser.Value;
			AutoNameSections = Main.AutoNameCheckBox.Checked;
			SymbolsAsFunctions = Main.SymbolsAsFunctionsCheckBox.Checked;
			GenerateIORegisters = Main.GenerateIORegistersCheckBox.Checked;
			ForceDSiIORegisters = Main.ForceDSiIORegistersCheckBox.Checked;
		}

		// decompressor
		public static void CreateDecompressor()
		{
			if (!Directory.Exists(DecompressorDir)) {
				Directory.CreateDirectory(DecompressorDir);
			}
			File.WriteAllBytes(DecompressorPath, Properties.Resources.blz);
		}
		public static bool CallDecompressor(string args)
		{
			Process process = new Process();
			process.StartInfo.FileName = DecompressorPath;
			process.StartInfo.Arguments = args;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			Console.WriteLine("decompressor called:");
			Console.WriteLine("{0:s} {1:s}", DecompressorPath, args);
			Console.WriteLine(output);

			return process.ExitCode == 0;
		}
		public static void DeleteDecompressor()
		{
			if (Directory.Exists(DecompressorDir)) {
				File.Delete(DecompressorPath);
			}
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
