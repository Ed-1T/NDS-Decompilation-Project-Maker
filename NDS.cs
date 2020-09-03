using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace NDSDecompilationProjectMaker
{
	namespace NDS
	{
		public class ROM
		{
			public BinaryReader Reader;
			public ARM.Binary ARM9;
			public ARM.Binary ARM7;
			public ARM.OverlayInfo[] ARM9OVT;
			public ARM.OverlayInfo[] ARM7OVT;
			public ARM.Overlay[] Overlay9;
			public ARM.Overlay[] Overlay7;

			public bool DSiGame = false;

			public uint ARM9StartModuleParamsOffset;
			public uint ARM7StartModuleParamsOffset;

			public uint ARM9StartModuleParamsAddress => ARM9StartModuleParamsOffset + ARM9.Info.ramAddress;
			public uint ARM7StartModuleParamsAddress => ARM7StartModuleParamsOffset + ARM7.Info.ramAddress;

			public uint ARM9AutoloadDoneCallbackOffset;
			public uint ARM7AutoloadDoneCallbackOffset;

			private FSFilesystem fs;

			readonly byte[] nLogo = {
					0x24,0xFF,0xAE,0x51,0x69,0x9A,0xA2,0x21,0x3D,0x84,0x82,0x0A,0x84,0xE4,0x09,0xAD,
					0x11,0x24,0x8B,0x98,0xC0,0x81,0x7F,0x21,0xA3,0x52,0xBE,0x19,0x93,0x09,0xCE,0x20,
					0x10,0x46,0x4A,0x4A,0xF8,0x27,0x31,0xEC,0x58,0xC7,0xE8,0x33,0x82,0xE3,0xCE,0xBF,
					0x85,0xF4,0xDF,0x94,0xCE,0x4B,0x09,0xC1,0x94,0x56,0x8A,0xC0,0x13,0x72,0xA7,0xFC,
					0x9F,0x84,0x4D,0x73,0xA3,0xCA,0x9A,0x61,0x58,0x97,0xA3,0x27,0xFC,0x03,0x98,0x76,
					0x23,0x1D,0xC7,0x61,0x03,0x04,0xAE,0x56,0xBF,0x38,0x84,0x00,0x40,0xA7,0x0E,0xFD,
					0xFF,0x52,0xFE,0x03,0x6F,0x95,0x30,0xF1,0x97,0xFB,0xC0,0x85,0x60,0xD6,0x80,0x25,
					0xA9,0x63,0xBE,0x03,0x01,0x4E,0x38,0xE2,0xF9,0xA2,0x34,0xFF,0xBB,0x3E,0x03,0x44,
					0x78,0x00,0x90,0xCB,0x88,0x11,0x3A,0x94,0x65,0xC0,0x7C,0x63,0x87,0xF0,0x3C,0xAF,
					0xD6,0x25,0xE4,0x8B,0x38,0x0A,0xAC,0x72,0x21,0xD4,0xF8,0x07,0x56,0xCF };

			public ROM()
			{
				Reader = new BinaryReader(File.Open(Util.ROMPath, FileMode.Open));
				fs = new FSFilesystem(this);
			}

			public bool IsValid()
			{
				byte[] data = ReadBytes(0xC0, nLogo.Length);
				return data.SequenceEqual(nLogo);
			}

			public void Init()
			{
				// create temp executable
				Util.CreateDecompressor();

				Reader.BaseStream.Seek(0x12, SeekOrigin.Begin);
				byte unitCode = Reader.ReadByte();
				DSiGame = (unitCode & 1) != 0;

				// load binaries
				LoadARM9();

				if (ARM9.Info.romAddress < 0x4000)
				{
					Util.DeleteDecompressor();
					throw new Exception("Homebrew ROMs can't be processed");
				}

				LoadARM7();

				// load and decompress overlays
				LoadARM9Overlays();
				LoadARM7Overlays();

				Reader.BaseStream.Seek(0x70, SeekOrigin.Begin);
				ARM9AutoloadDoneCallbackOffset = Reader.ReadUInt32() - ARM9.Info.ramAddress;
				ARM7AutoloadDoneCallbackOffset = Reader.ReadUInt32() - ARM7.Info.ramAddress;

				// delete temp executable
				Util.DeleteDecompressor();
			}
			public void Close()
			{ 
				fs = null;
				Reader.Close();
			}

			private ARM.Binary GetBinary(int cpu)
			{
				string CpuName = "ARM" + cpu.ToString();

				int offset = cpu == 9 ? 0x20 : 0x30;
				Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
				ARM.BinaryInfo info = Util.ReadStruct<ARM.BinaryInfo>(Reader, 16);

				Status.SetStatusText(string.Format("Reading {0:S} binary", CpuName));

				Reader.BaseStream.Seek(info.romAddress, SeekOrigin.Begin);
				byte[] data = Reader.ReadBytes((int)info.size);

				return new ARM.Binary("Unknown Binary", data, info);
			}
			private void LoadARM9()
			{
				ARM9 = GetBinary(9);
				ARM9.Name = "ARM9";
				// only arm9 is compressed
				ARM9.TryDecompress();
			}
			private void LoadARM7()
			{
				ARM7 = GetBinary(7);
				ARM7.Name = "ARM7";
			}

			private ARM.OverlayInfo[] GetOVT(int cpu)
			{
				string CpuName = "ARM" + cpu.ToString();

				int offset = cpu == 9 ? 0x50 : 0x58;
				Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
				FileInfo info = Util.ReadStruct<FileInfo>(Reader, 8);

				// create overlay table
				int overlayCount = (int)(info.size & ~4) / 32;
				ARM.OverlayInfo[] ovt = new ARM.OverlayInfo[overlayCount];

				// seek to overlay table data
				Reader.BaseStream.Seek(info.romAddress, SeekOrigin.Begin);

				if (ovt.Length > 0)
				{
					// initialize progress bar
					Status.InitProgress();
					Status.SetMaxProgress(ovt.Length);
				}

				// read overlay table
				for (int i = 0; i < overlayCount; i++)
				{
					Status.SetStatusText(string.Format("Reading {0:s} overlay table [{1:d}/{2:d}]", CpuName, i + 1, ovt.Length));

					ovt[i] = Util.ReadStruct<ARM.OverlayInfo>(Reader, 32);

					Status.IncrementProgress();
				}

				Status.FillProgress();

				if (overlayCount == 0)
					Console.WriteLine("none");

				return ovt;
			}
			private void LoadARM9OVT()
			{
				ARM9OVT = GetOVT(9);
			}
			private void LoadARM7OVT()
			{
				ARM7OVT = GetOVT(7);
			}

			private ARM.Overlay[] GetOverlays(ARM.OverlayInfo[] ovt, int cpu)
			{
				string CpuName = "ARM" + cpu.ToString();

				ARM.Overlay[] overlays = new ARM.Overlay[ovt.Length];

				if (ovt.Length > 0)
				{
					// initialize progress bar
					Status.InitProgress();
					Status.SetMaxProgress(ovt.Length);
				}

				for (int i = 0; i < ovt.Length; i++)
				{
					Status.SetStatusText(string.Format("Decompressing {0:s} overlays [{1:d}/{2:d}]", CpuName, i+1, ovt.Length));

					byte[] data = fs.GetFile(ovt[i].fileID);

					overlays[i] = new ARM.Overlay(ovt[i], data, cpu);
					overlays[i].TryDecompress();

					Status.IncrementProgress();
				}

				Status.FillProgress();

				return overlays;
			}
			private void LoadARM9Overlays()
			{
				LoadARM9OVT();
				Overlay9 = GetOverlays(ARM9OVT, 9);
			}
			private void LoadARM7Overlays()
			{
				LoadARM7OVT();
				Overlay7 = GetOverlays(ARM7OVT, 7);
			}

			// section creation

			public long FindARM9StartModuleParams(long offset)
			{
				return ReadInt(ARM9.Info.romAddress + ARM9AutoloadDoneCallbackOffset - 4) - ARM9.Info.ramAddress;
			}
			public MemorySection[] GetARM9BinarySection()
			{
				// find memory section address
				ARM9StartModuleParamsOffset = (uint)FindARM9StartModuleParams(0);
				long pos = ARM9StartModuleParamsOffset;

				if (pos < 0)
					throw new Exception("Couldn't find the '_start_ModuleParams' table for the ARM9 processor");

				List<MemorySection> sections = new List<MemorySection>();
				BinaryReader arm9 = new BinaryReader(new MemoryStream(ARM9.Data));

				uint ramBase = ARM9.Info.ramAddress;

				arm9.BaseStream.Seek(pos, SeekOrigin.Begin);
				uint tableBegin = arm9.ReadUInt32() - ramBase;
				uint tableEnd = arm9.ReadUInt32() - ramBase;
				uint dataOffset = arm9.ReadUInt32() - ramBase;

				arm9.BaseStream.Seek(0, SeekOrigin.Begin);
				byte[] baseData = arm9.ReadBytes((int)dataOffset);
				sections.Add(new MemorySection(ramBase, dataOffset, 0, "arm9_0", baseData));

				int i = 1;
				int secCount = (int)(tableEnd - tableBegin) / 12;

				Status.InitProgress();
				Status.SetMaxProgress(secCount);

				while (tableBegin < tableEnd)
				{
					Status.SetStatusText(string.Format("Reading ARM9 sections [{0:d}/{1:d}]", i, secCount));

					arm9.BaseStream.Seek(tableBegin, SeekOrigin.Begin);
					byte[] raw = arm9.ReadBytes(12);
					ARM.Section sec = Util.ConvertToStruct<ARM.Section>(raw, 12);

					arm9.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
					byte[] data = arm9.ReadBytes((int)sec.size);

					tableBegin += 12;
					dataOffset += sec.size;

					sections.Add(new MemorySection(sec, string.Format("arm9_{0:d}", i++), data));

					Status.IncrementProgress();
				}

				Status.FillProgress();

				return sections.ToArray();
			}
			public long FindARM7StartModuleParams()
			{
				return ReadInt(ARM7.Info.romAddress + ARM7AutoloadDoneCallbackOffset - 4) - ARM7.Info.ramAddress;
			}
			public MemorySection[] GetARM7BinarySection()
			{
				// find memory section address
				ARM7StartModuleParamsOffset = (uint)FindARM7StartModuleParams();
				long pos = ARM7StartModuleParamsOffset;

				if (pos < 0)
					throw new Exception("Couldn't find the '_start_ModuleParams' table for the ARM7 processor");

				List<MemorySection> sections = new List<MemorySection>();
				BinaryReader arm7 = new BinaryReader(new MemoryStream(ARM7.Data));

				uint ramBase = ARM7.Info.ramAddress;

				arm7.BaseStream.Seek(pos, SeekOrigin.Begin);
				uint tableBegin = arm7.ReadUInt32() - ramBase;
				uint tableEnd = arm7.ReadUInt32() - ramBase;
				uint dataOffset = arm7.ReadUInt32() - ramBase;

				arm7.BaseStream.Seek(0, SeekOrigin.Begin);
				byte[] baseData = arm7.ReadBytes((int)dataOffset);
				sections.Add(new MemorySection(ramBase, dataOffset, 0, "arm7_0", baseData));

				int i = 1;
				int secCount = (int)(tableEnd - tableBegin) / 12;

				Status.InitProgress();
				Status.SetMaxProgress(secCount);

				while (tableBegin < tableEnd)
				{
					Status.SetStatusText(string.Format("Reading ARM7 sections [{0:d}/{1:d}]", i, secCount));

					arm7.BaseStream.Seek(tableBegin, SeekOrigin.Begin);
					byte[] raw = arm7.ReadBytes(12);
					ARM.Section sec = Util.ConvertToStruct<ARM.Section>(raw, 12);

					arm7.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
					byte[] data = arm7.ReadBytes((int)sec.size);

					tableBegin += 12;
					dataOffset += sec.size;

					sections.Add(new MemorySection(sec, string.Format("arm7_{0:d}", i++), data));

					Status.IncrementProgress();
				}

				Status.FillProgress();

				return sections.ToArray();
			}

			public MemorySection[] GetBinarySections()
			{
				List<MemorySection> all = new List<MemorySection>();
				all.AddRange(GetARM9BinarySection());
				all.AddRange(GetARM7BinarySection());
				return all.ToArray();
			}
			public MemorySection[] GetOverlaySections()
			{
				List<MemorySection> sections = new List<MemorySection>();

				int i = 1;

				Status.InitProgress();
				Status.SetMaxProgress(Overlay9.Length);

				foreach (var ov in Overlay9)
				{
					Status.SetStatusText(string.Format("Reading ARM9 overlay sections [{0:d}/{1:d}]", i++, Overlay9.Length));

					uint start = ov.Info.ramAddress;
					uint size = ov.Info.decompressedSize;
					uint bssSize = ov.Info.bssSize;
					string name = string.Format("arm9_ov{0:d}", ov.Info.id);
					byte[] data = ov.Data;
					sections.Add(new MemorySection(start, size, bssSize, name, data));

					Status.IncrementProgress();
				}

				i = 1;

				Status.InitProgress();
				Status.SetMaxProgress(Overlay7.Length);

				foreach (var ov in Overlay7)
				{
					Status.SetStatusText(string.Format("Reading ARM7 overlay sections [{0:d}/{1:d}]", i++, Overlay7.Length));

					uint start = ov.Info.ramAddress;
					uint size = ov.Info.decompressedSize;
					uint bssSize = ov.Info.bssSize;
					string name = string.Format("arm9_ov{0:d}", ov.Info.id);
					byte[] data = ov.Data;
					sections.Add(new MemorySection(start, size, bssSize, name, data));

					Status.IncrementProgress();
				}

				Status.FillProgress();

				return sections.ToArray();
			}

			// reader operations
			public byte[] ReadBytes(long offset, int count)
			{
				Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
				return Reader.ReadBytes(count);
			}
			public int ReadInt(long offset)
			{
				Reader.BaseStream.Seek(offset, SeekOrigin.Begin); 
				return Reader.ReadInt32();
			}
		}

		public class FSFilesystem
		{
			public struct FATFileInfo
			{
				public uint startAddress;
				public uint endAddress;

				public uint GetSize()
				{
					return endAddress - startAddress;
				}
			}

			private ROM rom;
			private FileInfo fat;

			public FSFilesystem(ROM rom)
			{
				this.rom = rom;
				Init();
			}
			private void Init()
			{
				// load FAT
				byte[] data = rom.ReadBytes(0x48, 8);
				fat = Util.ConvertToStruct<FileInfo>(data, 8);
			}

			private FATFileInfo GetFileInfo(int id)
			{
				byte[] data = rom.ReadBytes(fat.romAddress + (id * 8), 8);
				return Util.ConvertToStruct<FATFileInfo>(data, 8);
			}
			public byte[] GetFile(int id)
			{
				FATFileInfo file = GetFileInfo(id);
				return rom.ReadBytes(file.startAddress, (int)file.GetSize());
			}
		}

		public struct FileInfo
		{
			public uint romAddress;
			public uint size;
		}
	}
}
