using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NDSDecompilationProjectMaker
{
	namespace ARM
	{
		public class Binary
		{
			public string Name;
			public byte[] Data;
			public BinaryInfo Info;

			public Binary(string name, byte[] data, BinaryInfo info)
			{
				Name = name;
				Data = data;
				Info = info;
			}

			public void TryDecompress()
			{
				string basepath = Properties.Settings.Default.outputPath;
				string filepath = basepath + @"\temp.bin";
				string args = string.Format(@"-d {0:s}", filepath);

				// create temp file
				File.WriteAllBytes(filepath, Data);

				// call decompressor process
				Util.CallDecompressor(args);

				// read decompressed result
				Data = File.ReadAllBytes(filepath);

				// cleanup
				File.Delete(filepath);
			}
		}

		public class Overlay
		{
			public string Name;
			public byte[] Data;
			public OverlayInfo Info;

			public Overlay(OverlayInfo info, byte[] data, int cpu)
			{
				Name = string.Format("ov{0:s}_{1:d}", cpu.ToString(), info.id);
				Data = data;
				Info = info;
			}

			public void TryDecompress()
			{
				if (Info.IsCompressed())
				{
					string basepath = Properties.Settings.Default.outputPath;
					string filepath = basepath + @"\temp.bin";
					string args = string.Format(@"-d {0:s}", filepath);

					// create temp file
					File.WriteAllBytes(filepath, Data);

					Console.WriteLine("calling decompressor for {0:s}", Name);

					// call decompressor process
					if (!Util.CallDecompressor(args))
					{
						throw new Exception(string.Format("ERROR: Couldn't decompress overlay '{0:s}'", Name));
					}

					// read decompressed result
					Data = File.ReadAllBytes(filepath);

					// cleanup
					File.Delete(filepath);
				}
			}
		}

		public struct BinaryInfo
		{
			public uint romAddress;
			public uint entryAddress;
			public uint ramAddress;
			public uint size;
		}

		public struct OverlayInfo
		{
			public uint id;
			public uint ramAddress;
			public uint decompressedSize;
			public uint bssSize;
			public uint sinitStart;
			public uint sinitEnd;
			public ushort fileID;
			public ushort unused;
			public uint other;

			public uint GetCompressedSize()
			{
				return other & 0xFFFFFF;
			}
			public bool IsCompressed()
			{
				return (other & 0x1000000) != 0;
			}
		}

		public struct Section
		{
			public uint startAddress;
			public uint size;
			public uint bssSize;
		}
	}
}
