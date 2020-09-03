using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace NDSDecompilationProjectMaker
{
	public struct MemorySection
	{
		public ARM.Section info;
		public string name;
		public byte[] data;

		public MemorySection(ARM.Section info, string name, byte[] data)
		{
			this.info = info;
			this.name = name;
			this.data = data;
			Rename();
		}
		public MemorySection(uint startAddress, uint size, uint bssSize, string name, byte[] data)
		{
			info.startAddress = startAddress;
			info.size = size;
			info.bssSize = bssSize;
			this.name = name;
			this.data = data;
			Rename();
		}

		private void Rename()
		{
			if (Util.AutoNameSections)
			{
				if (info.startAddress < Util.Main.rom.ARM9.Info.ramAddress)
					name = "arm9_itcm";
				else if (info.startAddress == Util.Main.rom.ARM9.Info.ramAddress)
					name = "arm9_crt0";
				else if (info.startAddress == Util.Main.rom.ARM7.Info.ramAddress)
					name = "arm7_crt0";
			}
		}
		public bool ContainsAddress(uint address)
		{
			return (address >= GetAddress()) && (address < GetAddress() + GetSize());
		}

		public uint GetAddress()
		{
			return info.startAddress;
		}
		public uint GetBssAddress()
		{
			return GetAddress() + GetSize();
		}
		public uint GetSize()
		{
			return info.size;
		}
		public uint GetBssSize()
		{
			return info.bssSize;
		}
		public uint GetTotalSize()
		{
			return info.size + info.bssSize;
		}
	}

	public class DecompilationProjectMaker
	{
		public XDocument Document { get; private set; }
		private MemorySection[] ARM9Sections;
		private MemorySection[] ARM7Sections;
		private MemorySection[] OverlaySections;

		private List<MemorySection> ARM9MemoryGaps;

		// main functions
		public void CreateXmlDocument()
		{
			Document = XDocument.Parse(Properties.Resources.template);
			var prg = Document.Element("PROGRAM");

			prg.Add(Xml_CreateProcessor("ARM", "ARM:LE:32:v5t:default", "little"));
			prg.Add(Xml_CreateTree("Program Tree"));
		}
		public void Save()
		{
			Document.Save(Util.OutputDir + @"\out.xml");
		}

		// xml components functions
		private XElement Xml_CreateProcessor(string family, string id, string endianness)
		{
			return	new XElement("PROCESSOR",
						new XAttribute("NAME", family),
						new XAttribute("LANGUAGE_PROVIDER", id),
						new XAttribute("ENDIAN", endianness)
					);
		}
		private XElement Xml_CreateTree(string name)
		{
			var prgtrees = new XElement("PROGRAM_TREES");
			prgtrees.Add(new XElement("TREE", new XAttribute("NAME", name)));
			return prgtrees;
		}
		private XElement Xml_CreateMemorySection(string name, uint address, uint size, bool overlay = false, string ovname = "")
		{
			if (overlay)
			{
				return	new XElement("MEMORY_SECTION",
							new XAttribute("NAME", name),
							new XAttribute("START_ADDR", string.Format("{0:s}::ram:{1:X8}", ovname, address)),
							new XAttribute("LENGTH", string.Format("0x{0:X}", size)),
							new XAttribute("PERMISSIONS", "r")
						);
			}
			else
			{
				return	new XElement("MEMORY_SECTION", 
							new XAttribute( "NAME",  name),
							new XAttribute( "START_ADDR",  string.Format("{0:X8}",  address)),
							new XAttribute( "LENGTH",  string.Format("0x{0:X}",  size)),
							new XAttribute( "PERMISSIONS",  "rwx")
						);
			}
		}
		private XElement Xml_CreateFragment(string name, uint address, uint size, bool overlay = false, string ovname = "")
		{
			var frg = new XElement("FRAGMENT", new XAttribute("NAME", name));
			uint end = (size == 0 ? address : address + size - 1);

			if (overlay)
			{
				XElement rng =	new XElement("ADDRESS_RANGE",
									new XAttribute( "START",  string.Format("{0:s}::ram:{1:X8}",  ovname,  address)),
									new XAttribute( "END",  string.Format("{0:s}::ram:{1:X8}",  ovname, end))
								);

				frg.Add(rng);
			}
			else
			{
				XElement rng =	new XElement("ADDRESS_RANGE",
									new XAttribute( "START",  string.Format("{0:X8}",  address)),
									new XAttribute( "END",  string.Format("{0:X8}", end))
								);

				frg.Add(rng);
			}

			return frg;
		}
		private XElement Xml_CreateMemoryContents(string name)
		{
			return	new XElement("MEMORY_CONTENTS",
						new XAttribute("FILE_NAME",  (name + ".bytes")),
						new XAttribute("FILE_OFFSET",  string.Format("0x{0:X}",  0))
					);
		}
		private XElement Xml_CreateDatatype(string kind, string name, string type, string nspace = "/")
		{
			return	new XElement(kind,
						new XAttribute("NAME", name),
						new XAttribute("NAMESPACE", nspace),
						new XAttribute("DATATYPE", type)
					);
		}
		private XElement Xml_CreateData(uint address, uint size, string type, string type_nspace = "/")
		{
			return	new XElement("DEFINED_DATA",
						new XAttribute("ADDRESS", string.Format("{0:x08}", address)),
						new XAttribute("DATATYPE", type),
						new XAttribute("DATATYPE_NAMESPACE", type_nspace),
						new XAttribute("SIZE", string.Format("0x{0:X}", size))
					);
		}
		private XElement Xml_CreateSymbol(string name, uint address, string ovname = "")
		{
			if (ovname != string.Empty)
			{
				return new XElement("SYMBOL",
							new XAttribute("ADDRESS", string.Format("{0:s}::ram:{1:X8}", ovname, address)),
							new XAttribute("NAME", name),
							new XAttribute("NAMESPACE", ""),
							new XAttribute("TYPE", "global"),
							new XAttribute("SOURCE_TYPE", "USER_DEFINED"),
							new XAttribute("PRIMARY", "y")
						);
			}
			else
			{
				return	new XElement("SYMBOL",
							new XAttribute("ADDRESS", string.Format("{0:X8}", address)),
							new XAttribute("NAME", name),
							new XAttribute("NAMESPACE", ""),
							new XAttribute("TYPE", "global"),
							new XAttribute("SOURCE_TYPE", "USER_DEFINED"),
							new XAttribute("PRIMARY", "y")
						);
			}
		}
		private XElement Xml_CreateFunction(string name, uint address, string ovname = "")
		{
			if (ovname != string.Empty)
			{
				var func =	new XElement("FUNCTION",
								new XAttribute("ENTRY_POINT", string.Format("{0:s}::ram:{1:X8}", ovname, address)),
								new XAttribute("NAME", name),
								new XAttribute("LIBRARY_FUNCTION", "y")
							);
				func.Add(new XElement("TYPEINFO_CMT", string.Format("undefined {0:s}(void)", name)));
				return func;
			}
			else
			{ 
				var func =	new XElement("FUNCTION", 
								new XAttribute( "ENTRY_POINT",  string.Format("{0:X8}",  address)),
								new XAttribute( "NAME",  name),
								new XAttribute( "LIBRARY_FUNCTION",  "y")
							);
				func.Add(new XElement("TYPEINFO_CMT", string.Format("undefined {0:s}(void)", name)));
				return func;
			}
		}

		// memory section functions
		public void CreateMemorySection(MemorySection section, bool overlay, string ovname = "")
		{
			var prg = Document.Element("PROGRAM");
			var mmap = prg.Element("MEMORY_MAP");
			var tree = prg.Element("PROGRAM_TREES").Element("TREE");

			string name = section.name;

			if (section.info.size != 0)
			{
				var sec = Xml_CreateMemorySection(name, section.GetAddress(), section.GetSize(), overlay, ovname);
				//var frg = Xml_CreateFragment(name, section.GetAddress(), section.GetSize(), overlay, ovname);
				
				if (section.data != null)
				{
					sec.Add(Xml_CreateMemoryContents(name));

					string output = Util.OutputDir + @"\" + name + ".bytes";
					BinaryWriter bwr = new BinaryWriter(File.Open(output, FileMode.Create));
					bwr.Write(section.data, 0, (int)section.GetSize());
					bwr.Close();
				}

				mmap.Add(sec);
				//tree.Add(frg);
			}

			if (section.info.bssSize != 0)
			{
				var sec = Xml_CreateMemorySection(name + "_bss", section.GetBssAddress(), section.GetBssSize(), overlay, ovname);
				//var frg = Xml_CreateFragment(name + "_bss", section.GetBssAddress(), section.GetBssSize(), overlay, ovname);
				
				if (Util.FillBSS)
				{
					sec.Add(Xml_CreateMemoryContents(name));

					byte[] buffer = new byte[section.GetBssSize()];

					for (int i = 0; i < section.GetBssSize(); i++)
					{
						buffer[i] = Util.FillValue;
					}

					string output = Util.OutputDir + @"\" + name + "_bss.bytes";
					BinaryWriter bwr = new BinaryWriter(File.Open(output, FileMode.Create));
					bwr.Write(buffer, 0, (int)section.GetBssSize());
					bwr.Close();
				}

				mmap.Add(sec);
				//tree.Add(frg);
			}
		}
		public void CreateStandardMemorySections(MemorySection[] sections)
		{
			ARM9Sections = sections;
			foreach (MemorySection sec in sections)
				CreateMemorySection(sec, false);
		}
		public void CreateOverlayMemorySections(MemorySection[] sections)
		{
			OverlaySections = sections;
			foreach (MemorySection sec in sections)
				CreateMemorySection(sec, true, sec.name);
		}
		public void CreateARM7MemorySections(MemorySection[] sections)
		{
			ARM7Sections = sections;
			foreach (MemorySection sec in sections)
				CreateMemorySection(sec, true, "arm7");
		}
		public void CreateSectionManual(string name, uint address, uint size)
		{
			var prg = Document.Element("PROGRAM");

			var mmap = prg.Element("MEMORY_MAP");
			var tree = prg.Element("PROGRAM_TREES").Element("TREE");

			var sec = Xml_CreateMemorySection(name, address, size);
			//var frg = Xml_CreateFragment(name, address, size);

			mmap.Add(sec);
			//tree.Add(frg);
		}
		public void FillMemoryGaps()
		{
			// the program currently creates 
			// sections like this:
			// ---------------------------------------
			// code section 1
			// bss section 1
			// <possibly empty> <----
			// code section 2
			// bss section 2
			// <possibly empty> <----
			// ...
			// this function fills the gaps that might 
			// appear between bss and code sections

			ARM9MemoryGaps = new List<MemorySection>();
			ARM9MemoryGaps.AddRange((MemorySection[])ARM9Sections.Clone());
			ARM9MemoryGaps = ARM9MemoryGaps.OrderBy(o => o.GetAddress()).ToList();

			uint size = 0;
			uint lastEnd = uint.MaxValue;
			string lastName = "";

			foreach (var sec in ARM9MemoryGaps)
			{
				if (lastEnd != uint.MaxValue)
				{
					if (lastEnd >= 0x27E0000)
					{
						lastEnd = sec.GetAddress() + sec.GetTotalSize();
						break;
					}

					size = sec.GetAddress() - lastEnd;
					if (size > 0)
						CreateSectionManual(lastName, lastEnd, size);
				}
				lastEnd = sec.GetAddress() + sec.GetTotalSize();
				lastName = sec.name + "_free";
			}

			size = 0x3000000 - lastEnd;
			if (size > 0)
				CreateSectionManual(lastName, lastEnd, size);
		}

		// symbol definition functions
		public void CreateTypedef(string name, string type)
		{
			var prg = Document.Element("PROGRAM");
			prg.Element("DATATYPES").Add(Xml_CreateDatatype("TYPE_DEF", name, type));
		}
		public void CreateData(uint address, uint size, string type)
		{
			var prg = Document.Element("PROGRAM");
			prg.Element("DATA").Add(Xml_CreateData(address, size, type));
		}
		public void CreateSymbol(uint address, string name, string ovname = "")
		{
			var prg = Document.Element("PROGRAM");
			prg.Element("SYMBOL_TABLE").Add(Xml_CreateSymbol(name, address, ovname));
		}
		public void CreateFunction(uint address, string name, string ovname = "")
		{
			var prg = Document.Element("PROGRAM");
			prg.Element("FUNCTIONS").Add(Xml_CreateFunction(name, address, ovname));
		}
		public void DefineCommonDatatypes()
		{
			CreateTypedef("u8",  "byte");
			CreateTypedef("u16", "ushort");
			CreateTypedef("u32", "ulong");
			CreateTypedef("u64", "ulonglong");

			CreateTypedef("s8",  "sbyte");
			CreateTypedef("s16", "short");
			CreateTypedef("s32", "long");
			CreateTypedef("s64", "longlong");

			CreateTypedef("vu8",  "byte");
			CreateTypedef("vu16", "ushort");
			CreateTypedef("vu32", "ulong");
			CreateTypedef("vu64", "ulonglong");

			CreateTypedef("vs8",  "sbyte");
			CreateTypedef("vs16", "short");
			CreateTypedef("vs32", "long");
			CreateTypedef("vs64", "longlong");

			CreateTypedef("fx16", "short");
			CreateTypedef("fx32", "long");
			CreateTypedef("fx64", "longlong");
			CreateTypedef("fx64c","longlong");
		}
		public void DefineCommonData()
		{
			if (!Util.GenerateIORegisters)
				return;

			Dictionary<string, uint> dtSizes = new Dictionary<string, uint>()
			{
				{ "vu8",  1 },
				{ "vu16", 2 },
				{ "vu32", 4 },
				{ "vu64", 8 },
			};

			Func<string,uint,string,int> def_data = (type, address, name) => 
			{
				Status.SetStatusText(string.Format("Defining IO Register: '{0}'", name));
				CreateData(address, dtSizes[type], type);
				CreateSymbol(address, name);
				return 0; 
			};

			bool dsi = Util.Main.rom.DSiGame || Util.ForceDSiIORegisters;
			uint io = 0x04000000;

			def_data("vu32", io + 0x00000000, "reg_GX_DISPCNT");
			def_data("vu16", io + 0x00000004, "reg_GX_DISPSTAT");
			def_data("vu16", io + 0x00000006, "reg_GX_VCOUNT");
			def_data("vu32", io + 0x00000064, "reg_GX_DISPCAPCNT");
			def_data("vu32", io + 0x00000068, "reg_GX_DISP_MMEM_FIFO");
			def_data("vu16", io + 0x0000006C, "reg_GX_MASTER_BRIGHT");
			def_data("vu16", io + 0x00000070, "reg_GX_TVOUTCNT");
			def_data("vu8",  io + 0x00000240, "reg_GX_VRAMCNT_A");
			def_data("vu8",  io + 0x00000241, "reg_GX_VRAMCNT_B");
			def_data("vu8",  io + 0x00000242, "reg_GX_VRAMCNT_C");
			def_data("vu8",  io + 0x00000243, "reg_GX_VRAMCNT_D");
			def_data("vu8",  io + 0x00000244, "reg_GX_VRAMCNT_E");
			def_data("vu8",  io + 0x00000245, "reg_GX_VRAMCNT_F");
			def_data("vu8",  io + 0x00000246, "reg_GX_VRAMCNT_G");
			def_data("vu8",  io + 0x00000247, "reg_GX_VRAMCNT_WRAM");
			def_data("vu8",  io + 0x00000248, "reg_GX_VRAMCNT_H");
			def_data("vu8",  io + 0x00000249, "reg_GX_VRAMCNT_I");
			def_data("vu16", io + 0x00000304, "reg_GX_POWCNT");


			def_data("vu32", io + 0x00001000, "reg_GXS_DB_DISPCNT");
			def_data("vu16", io + 0x0000106C, "reg_GXS_DB_MASTER_BRIGHT");


			def_data("vu16", io + 0x00000060, "reg_G3X_DISP3DCNT");
			def_data("vu16", io + 0x00000320, "reg_G3X_RDLINES_COUNT");
			def_data("vu32", io + 0x00000330, "reg_G3X_EDGE_COLOR_0");
			def_data("vu32", io + 0x00000334, "reg_G3X_EDGE_COLOR_1");
			def_data("vu32", io + 0x00000338, "reg_G3X_EDGE_COLOR_2");
			def_data("vu32", io + 0x0000033C, "reg_G3X_EDGE_COLOR_3");
			def_data("vu16", io + 0x00000340, "reg_G3X_ALPHA_TEST_REF");
			def_data("vu32", io + 0x00000350, "reg_G3X_CLEAR_COLOR");
			def_data("vu16", io + 0x00000354, "reg_G3X_CLEAR_DEPTH");
			def_data("vu16", io + 0x00000356, "reg_G3X_CLRIMAGE_OFFSET");
			def_data("vu32", io + 0x00000358, "reg_G3X_FOG_COLOR");
			def_data("vu16", io + 0x0000035C, "reg_G3X_FOG_OFFSET");
			def_data("vu32", io + 0x00000360, "reg_G3X_FOG_TABLE_0");
			def_data("vu32", io + 0x00000364, "reg_G3X_FOG_TABLE_1");
			def_data("vu32", io + 0x00000368, "reg_G3X_FOG_TABLE_2");
			def_data("vu32", io + 0x0000036C, "reg_G3X_FOG_TABLE_3");
			def_data("vu32", io + 0x00000370, "reg_G3X_FOG_TABLE_4");
			def_data("vu32", io + 0x00000374, "reg_G3X_FOG_TABLE_5");
			def_data("vu32", io + 0x00000378, "reg_G3X_FOG_TABLE_6");
			def_data("vu32", io + 0x0000037C, "reg_G3X_FOG_TABLE_7");
			def_data("vu32", io + 0x00000380, "reg_G3X_TOON_TABLE_0");
			def_data("vu32", io + 0x00000384, "reg_G3X_TOON_TABLE_1");
			def_data("vu32", io + 0x00000388, "reg_G3X_TOON_TABLE_2");
			def_data("vu32", io + 0x0000038C, "reg_G3X_TOON_TABLE_3");
			def_data("vu32", io + 0x00000390, "reg_G3X_TOON_TABLE_4");
			def_data("vu32", io + 0x00000394, "reg_G3X_TOON_TABLE_5");
			def_data("vu32", io + 0x00000398, "reg_G3X_TOON_TABLE_6");
			def_data("vu32", io + 0x0000039C, "reg_G3X_TOON_TABLE_7");
			def_data("vu32", io + 0x000003A0, "reg_G3X_TOON_TABLE_8");
			def_data("vu32", io + 0x000003A4, "reg_G3X_TOON_TABLE_9");
			def_data("vu32", io + 0x000003A8, "reg_G3X_TOON_TABLE_10");
			def_data("vu32", io + 0x000003AC, "reg_G3X_TOON_TABLE_11");
			def_data("vu32", io + 0x000003B0, "reg_G3X_TOON_TABLE_12");
			def_data("vu32", io + 0x000003B4, "reg_G3X_TOON_TABLE_13");
			def_data("vu32", io + 0x000003B8, "reg_G3X_TOON_TABLE_14");
			def_data("vu32", io + 0x000003BC, "reg_G3X_TOON_TABLE_15");
			def_data("vu32", io + 0x00000400, "reg_G3X_GXFIFO");
			def_data("vu32", io + 0x00000600, "reg_G3X_GXSTAT");
			def_data("vu16", io + 0x00000604, "reg_G3X_LISTRAM_COUNT");
			def_data("vu16", io + 0x00000606, "reg_G3X_VTXRAM_COUNT");
			def_data("vu16", io + 0x00000610, "reg_G3X_DISP_1DOT_DEPTH");
			def_data("vu32", io + 0x00000620, "reg_G3X_POS_RESULT_X");
			def_data("vu32", io + 0x00000624, "reg_G3X_POS_RESULT_Y");
			def_data("vu32", io + 0x00000628, "reg_G3X_POS_RESULT_Z");
			def_data("vu32", io + 0x0000062C, "reg_G3X_POS_RESULT_W");
			def_data("vu16", io + 0x00000630, "reg_G3X_VEC_RESULT_X");
			def_data("vu16", io + 0x00000634, "reg_G3X_VEC_RESULT_Y");
			def_data("vu16", io + 0x00000638, "reg_G3X_VEC_RESULT_Z");
			def_data("vu32", io + 0x00000640, "reg_G3X_CLIPMTX_RESULT_0");
			def_data("vu32", io + 0x00000644, "reg_G3X_CLIPMTX_RESULT_1");
			def_data("vu32", io + 0x00000648, "reg_G3X_CLIPMTX_RESULT_2");
			def_data("vu32", io + 0x0000064C, "reg_G3X_CLIPMTX_RESULT_3");
			def_data("vu32", io + 0x00000650, "reg_G3X_CLIPMTX_RESULT_4");
			def_data("vu32", io + 0x00000654, "reg_G3X_CLIPMTX_RESULT_5");
			def_data("vu32", io + 0x00000658, "reg_G3X_CLIPMTX_RESULT_6");
			def_data("vu32", io + 0x0000065C, "reg_G3X_CLIPMTX_RESULT_7");
			def_data("vu32", io + 0x00000660, "reg_G3X_CLIPMTX_RESULT_8");
			def_data("vu32", io + 0x00000664, "reg_G3X_CLIPMTX_RESULT_9");
			def_data("vu32", io + 0x00000668, "reg_G3X_CLIPMTX_RESULT_10");
			def_data("vu32", io + 0x0000066C, "reg_G3X_CLIPMTX_RESULT_11");
			def_data("vu32", io + 0x00000670, "reg_G3X_CLIPMTX_RESULT_12");
			def_data("vu32", io + 0x00000674, "reg_G3X_CLIPMTX_RESULT_13");
			def_data("vu32", io + 0x00000678, "reg_G3X_CLIPMTX_RESULT_14");
			def_data("vu32", io + 0x0000067C, "reg_G3X_CLIPMTX_RESULT_15");
			def_data("vu32", io + 0x00000680, "reg_G3X_VECMTX_RESULT_0");
			def_data("vu32", io + 0x00000684, "reg_G3X_VECMTX_RESULT_1");
			def_data("vu32", io + 0x00000688, "reg_G3X_VECMTX_RESULT_2");
			def_data("vu32", io + 0x0000068C, "reg_G3X_VECMTX_RESULT_3");
			def_data("vu32", io + 0x00000690, "reg_G3X_VECMTX_RESULT_4");
			def_data("vu32", io + 0x00000694, "reg_G3X_VECMTX_RESULT_5");
			def_data("vu32", io + 0x00000698, "reg_G3X_VECMTX_RESULT_6");
			def_data("vu32", io + 0x0000069C, "reg_G3X_VECMTX_RESULT_7");
			def_data("vu32", io + 0x000006A0, "reg_G3X_VECMTX_RESULT_8");


			def_data("vu32", io + 0x00000440, "reg_G3_MTX_MODE");
			def_data("vu32", io + 0x00000444, "reg_G3_MTX_PUSH");
			def_data("vu32", io + 0x00000448, "reg_G3_MTX_POP");
			def_data("vu32", io + 0x0000044C, "reg_G3_MTX_STORE");
			def_data("vu32", io + 0x00000450, "reg_G3_MTX_RESTORE");
			def_data("vu32", io + 0x00000454, "reg_G3_MTX_IDENTITY");
			def_data("vu32", io + 0x00000458, "reg_G3_MTX_LOAD_4x4");
			def_data("vu32", io + 0x0000045C, "reg_G3_MTX_LOAD_4x3");
			def_data("vu32", io + 0x00000460, "reg_G3_MTX_MULT_4x4");
			def_data("vu32", io + 0x00000464, "reg_G3_MTX_MULT_4x3");
			def_data("vu32", io + 0x00000468, "reg_G3_MTX_MULT_3x3");
			def_data("vu32", io + 0x0000046C, "reg_G3_MTX_SCALE");
			def_data("vu32", io + 0x00000470, "reg_G3_MTX_TRANS");
			def_data("vu32", io + 0x00000480, "reg_G3_COLOR");
			def_data("vu32", io + 0x00000484, "reg_G3_NORMAL");
			def_data("vu32", io + 0x00000488, "reg_G3_TEXCOORD");
			def_data("vu32", io + 0x0000048C, "reg_G3_VTX_16");
			def_data("vu32", io + 0x00000490, "reg_G3_VTX_10");
			def_data("vu32", io + 0x00000494, "reg_G3_VTX_XY");
			def_data("vu32", io + 0x00000498, "reg_G3_VTX_XZ");
			def_data("vu32", io + 0x0000049C, "reg_G3_VTX_YZ");
			def_data("vu32", io + 0x000004A0, "reg_G3_VTX_DIFF");
			def_data("vu32", io + 0x000004A4, "reg_G3_POLYGON_ATTR");
			def_data("vu32", io + 0x000004A8, "reg_G3_TEXIMAGE_PARAM");
			def_data("vu32", io + 0x000004AC, "reg_G3_TEXPLTT_BASE");
			def_data("vu32", io + 0x000004C0, "reg_G3_DIF_AMB");
			def_data("vu32", io + 0x000004C4, "reg_G3_SPE_EMI");
			def_data("vu32", io + 0x000004C8, "reg_G3_LIGHT_VECTOR");
			def_data("vu32", io + 0x000004CC, "reg_G3_LIGHT_COLOR");
			def_data("vu32", io + 0x000004D0, "reg_G3_SHININESS");
			def_data("vu32", io + 0x00000500, "reg_G3_BEGIN_VTXS");
			def_data("vu32", io + 0x00000504, "reg_G3_END_VTXS");
			def_data("vu32", io + 0x00000540, "reg_G3_SWAP_BUFFERS");
			def_data("vu32", io + 0x00000580, "reg_G3_VIEWPORT");
			def_data("vu32", io + 0x000005C0, "reg_G3_BOX_TEST");
			def_data("vu32", io + 0x000005C4, "reg_G3_POS_TEST");
			def_data("vu32", io + 0x000005C8, "reg_G3_VEC_TEST");


			def_data("vu16", io + 0x00000008, "reg_G2_BG0CNT");
			def_data("vu16", io + 0x0000000A, "reg_G2_BG1CNT");
			def_data("vu16", io + 0x0000000C, "reg_G2_BG2CNT");
			def_data("vu16", io + 0x0000000E, "reg_G2_BG3CNT");
			def_data("vu32", io + 0x00000010, "reg_G2_BG0OFS");
			def_data("vu32", io + 0x00000014, "reg_G2_BG1OFS");
			def_data("vu32", io + 0x00000018, "reg_G2_BG2OFS");
			def_data("vu32", io + 0x0000001C, "reg_G2_BG3OFS");
			def_data("vu16", io + 0x00000020, "reg_G2_BG2PA");
			def_data("vu16", io + 0x00000022, "reg_G2_BG2PB");
			def_data("vu16", io + 0x00000024, "reg_G2_BG2PC");
			def_data("vu16", io + 0x00000026, "reg_G2_BG2PD");
			def_data("vu32", io + 0x00000028, "reg_G2_BG2X");
			def_data("vu32", io + 0x0000002C, "reg_G2_BG2Y");
			def_data("vu16", io + 0x00000030, "reg_G2_BG3PA");
			def_data("vu16", io + 0x00000032, "reg_G2_BG3PB");
			def_data("vu16", io + 0x00000034, "reg_G2_BG3PC");
			def_data("vu16", io + 0x00000036, "reg_G2_BG3PD");
			def_data("vu32", io + 0x00000038, "reg_G2_BG3X");
			def_data("vu32", io + 0x0000003C, "reg_G2_BG3Y");
			def_data("vu16", io + 0x00000040, "reg_G2_WIN0H");
			def_data("vu16", io + 0x00000042, "reg_G2_WIN1H");
			def_data("vu16", io + 0x00000044, "reg_G2_WIN0V");
			def_data("vu16", io + 0x00000046, "reg_G2_WIN1V");
			def_data("vu16", io + 0x00000048, "reg_G2_WININ");
			def_data("vu16", io + 0x0000004A, "reg_G2_WINOUT");
			def_data("vu16", io + 0x0000004C, "reg_G2_MOSAIC");
			def_data("vu16", io + 0x00000050, "reg_G2_BLDCNT");
			def_data("vu16", io + 0x00000052, "reg_G2_BLDALPHA");
			def_data("vu16", io + 0x00000054, "reg_G2_BLDY");


			def_data("vu16", io + 0x00001008, "reg_G2S_DB_BG0CNT");
			def_data("vu16", io + 0x0000100A, "reg_G2S_DB_BG1CNT");
			def_data("vu16", io + 0x0000100C, "reg_G2S_DB_BG2CNT");
			def_data("vu16", io + 0x0000100E, "reg_G2S_DB_BG3CNT");
			def_data("vu32", io + 0x00001010, "reg_G2S_DB_BG0OFS");
			def_data("vu32", io + 0x00001014, "reg_G2S_DB_BG1OFS");
			def_data("vu32", io + 0x00001018, "reg_G2S_DB_BG2OFS");
			def_data("vu32", io + 0x0000101C, "reg_G2S_DB_BG3OFS");
			def_data("vu16", io + 0x00001020, "reg_G2S_DB_BG2PA");
			def_data("vu16", io + 0x00001022, "reg_G2S_DB_BG2PB");
			def_data("vu16", io + 0x00001024, "reg_G2S_DB_BG2PC");
			def_data("vu16", io + 0x00001026, "reg_G2S_DB_BG2PD");
			def_data("vu32", io + 0x00001028, "reg_G2S_DB_BG2X");
			def_data("vu32", io + 0x0000102C, "reg_G2S_DB_BG2Y");
			def_data("vu16", io + 0x00001030, "reg_G2S_DB_BG3PA");
			def_data("vu16", io + 0x00001032, "reg_G2S_DB_BG3PB");
			def_data("vu16", io + 0x00001034, "reg_G2S_DB_BG3PC");
			def_data("vu16", io + 0x00001036, "reg_G2S_DB_BG3PD");
			def_data("vu32", io + 0x00001038, "reg_G2S_DB_BG3X");
			def_data("vu32", io + 0x0000103C, "reg_G2S_DB_BG3Y");
			def_data("vu16", io + 0x00001040, "reg_G2S_DB_WIN0H");
			def_data("vu16", io + 0x00001042, "reg_G2S_DB_WIN1H");
			def_data("vu16", io + 0x00001044, "reg_G2S_DB_WIN0V");
			def_data("vu16", io + 0x00001046, "reg_G2S_DB_WIN1V");
			def_data("vu16", io + 0x00001048, "reg_G2S_DB_WININ");
			def_data("vu16", io + 0x0000104A, "reg_G2S_DB_WINOUT");
			def_data("vu16", io + 0x0000104C, "reg_G2S_DB_MOSAIC");
			def_data("vu16", io + 0x00001050, "reg_G2S_DB_BLDCNT");
			def_data("vu16", io + 0x00001052, "reg_G2S_DB_BLDALPHA");
			def_data("vu16", io + 0x00001054, "reg_G2S_DB_BLDY");


			def_data("vu16", io + 0x00000100, "reg_OS_TM0CNT_L");
			def_data("vu16", io + 0x00000102, "reg_OS_TM0CNT_H");
			def_data("vu16", io + 0x00000104, "reg_OS_TM1CNT_L");
			def_data("vu16", io + 0x00000106, "reg_OS_TM1CNT_H");
			def_data("vu16", io + 0x00000108, "reg_OS_TM2CNT_L");
			def_data("vu16", io + 0x0000010A, "reg_OS_TM2CNT_H");
			def_data("vu16", io + 0x0000010C, "reg_OS_TM3CNT_L");
			def_data("vu16", io + 0x0000010E, "reg_OS_TM3CNT_H");
			def_data("vu16", io + 0x00000208, "reg_OS_IME");
			def_data("vu32", io + 0x00000210, "reg_OS_IE");
			def_data("vu32", io + 0x00000214, "reg_OS_IF");
			def_data("vu16", io + 0x00000300, "reg_OS_PAUSE");


			def_data("vu16", io + 0x00000180, "reg_PXI_SUBPINTF");
			def_data("vu16", io + 0x00000184, "reg_PXI_SUBP_FIFO_CNT");
			def_data("vu32", io + 0x00000188, "reg_PXI_SEND_FIFO");
			def_data("vu32", io + 0x00010000, "reg_PXI_RECV_FIFO");


			def_data("vu16", io + 0x00000130, "reg_PAD_KEYINPUT");
			def_data("vu16", io + 0x00000132, "reg_PAD_KEYCNT");


			def_data("vu16", io + 0x00000280, "reg_CP_DIVCNT");
			def_data("vu64", io + 0x00000290, "reg_CP_DIV_NUMER");
			def_data("vu64", io + 0x00000298, "reg_CP_DIV_DENOM");
			def_data("vu64", io + 0x000002A0, "reg_CP_DIV_RESULT");
			def_data("vu64", io + 0x000002A8, "reg_CP_DIVREM_RESULT");
			def_data("vu16", io + 0x000002B0, "reg_CP_SQRTCNT");
			def_data("vu32", io + 0x000002B4, "reg_CP_SQRT_RESULT");
			def_data("vu64", io + 0x000002B8, "reg_CP_SQRT_PARAM");


			def_data("vu32", io + 0x00000120, "reg_EXI_SIODATA32");
			def_data("vu16", io + 0x00000128, "reg_EXI_SIOCNT");
			def_data("vu16", io + 0x0000012C, "reg_EXI_SIOSEL");


			if (dsi)
			{
				def_data("vu16", io + 0x00004300, "reg_DSP_PDATA");
				def_data("vu16", io + 0x00004304, "reg_DSP_PADR");
				def_data("vu16", io + 0x00004308, "reg_DSP_PCFG");
				def_data("vu16", io + 0x0000430C, "reg_DSP_PSTS");
				def_data("vu16", io + 0x00004310, "reg_DSP_PSEM");
				def_data("vu16", io + 0x00004314, "reg_DSP_PMASK");
				def_data("vu16", io + 0x00004318, "reg_DSP_PCLEAR");
				def_data("vu16", io + 0x0000431C, "reg_DSP_SEM");
				def_data("vu16", io + 0x00004320, "reg_DSP_COM0");
				def_data("vu16", io + 0x00004324, "reg_DSP_REP0");
				def_data("vu16", io + 0x00004328, "reg_DSP_COM1");
				def_data("vu16", io + 0x0000432C, "reg_DSP_REP1");
				def_data("vu16", io + 0x00004330, "reg_DSP_COM2");
				def_data("vu16", io + 0x00004334, "reg_DSP_REP2");


				def_data("vu8", io + 0x00004000, "reg_SCFG_A9ROM");
				def_data("vu16", io + 0x00004004, "reg_SCFG_CLK");
				def_data("vu16", io + 0x00004006, "reg_SCFG_RST");
				def_data("vu32", io + 0x00004008, "reg_SCFG_EXT");


				def_data("vu16", io + 0x00004200, "reg_CAM_MCNT");
				def_data("vu16", io + 0x00004202, "reg_CAM_CNT");
				def_data("vu32", io + 0x00004204, "reg_CAM_DAT");
				def_data("vu32", io + 0x00004210, "reg_CAM_SOFS");
				def_data("vu32", io + 0x00004214, "reg_CAM_EOFS");
			}


			def_data("vu32", io + 0x000000B0, "reg_MI_DMA0SAD");
			def_data("vu32", io + 0x000000B4, "reg_MI_DMA0DAD");
			def_data("vu32", io + 0x000000B8, "reg_MI_DMA0CNT");
			def_data("vu32", io + 0x000000BC, "reg_MI_DMA1SAD");
			def_data("vu32", io + 0x000000C0, "reg_MI_DMA1DAD");
			def_data("vu32", io + 0x000000C4, "reg_MI_DMA1CNT");
			def_data("vu32", io + 0x000000C8, "reg_MI_DMA2SAD");
			def_data("vu32", io + 0x000000CC, "reg_MI_DMA2DAD");
			def_data("vu32", io + 0x000000D0, "reg_MI_DMA2CNT");
			def_data("vu32", io + 0x000000D4, "reg_MI_DMA3SAD");
			def_data("vu32", io + 0x000000D8, "reg_MI_DMA3DAD");
			def_data("vu32", io + 0x000000DC, "reg_MI_DMA3CNT");
			def_data("vu32", io + 0x000000E0, "reg_MI_DMA0_CLR_DATA");
			def_data("vu32", io + 0x000000E4, "reg_MI_DMA1_CLR_DATA");
			def_data("vu32", io + 0x000000E8, "reg_MI_DMA2_CLR_DATA");
			def_data("vu32", io + 0x000000EC, "reg_MI_DMA3_CLR_DATA");

			if (dsi)
			{
				def_data("vu32", io + 0x00004100, "reg_MI_NDMAGCNT");
				def_data("vu32", io + 0x00004104, "reg_MI_NDMA0SAD");
				def_data("vu32", io + 0x00004108, "reg_MI_NDMA0DAD");
				def_data("vu32", io + 0x0000410C, "reg_MI_NDMA0TCNT");
				def_data("vu32", io + 0x00004110, "reg_MI_NDMA0WCNT");
				def_data("vu32", io + 0x00004114, "reg_MI_NDMA0BCNT");
				def_data("vu32", io + 0x00004118, "reg_MI_NDMA0FDATA");
				def_data("vu32", io + 0x0000411C, "reg_MI_NDMA0CNT");
				def_data("vu32", io + 0x00004120, "reg_MI_NDMA1SAD");
				def_data("vu32", io + 0x00004124, "reg_MI_NDMA1DAD");
				def_data("vu32", io + 0x00004128, "reg_MI_NDMA1TCNT");
				def_data("vu32", io + 0x0000412C, "reg_MI_NDMA1WCNT");
				def_data("vu32", io + 0x00004130, "reg_MI_NDMA1BCNT");
				def_data("vu32", io + 0x00004134, "reg_MI_NDMA1FDATA");
				def_data("vu32", io + 0x00004138, "reg_MI_NDMA1CNT");
				def_data("vu32", io + 0x0000413C, "reg_MI_NDMA2SAD");
				def_data("vu32", io + 0x00004140, "reg_MI_NDMA2DAD");
				def_data("vu32", io + 0x00004144, "reg_MI_NDMA2TCNT");
				def_data("vu32", io + 0x00004148, "reg_MI_NDMA2WCNT");
				def_data("vu32", io + 0x0000414C, "reg_MI_NDMA2BCNT");
				def_data("vu32", io + 0x00004150, "reg_MI_NDMA2FDATA");
				def_data("vu32", io + 0x00004154, "reg_MI_NDMA2CNT");
				def_data("vu32", io + 0x00004158, "reg_MI_NDMA3SAD");
				def_data("vu32", io + 0x0000415C, "reg_MI_NDMA3DAD");
				def_data("vu32", io + 0x00004160, "reg_MI_NDMA3TCNT");
				def_data("vu32", io + 0x00004164, "reg_MI_NDMA3WCNT");
				def_data("vu32", io + 0x00004168, "reg_MI_NDMA3BCNT");
				def_data("vu32", io + 0x0000416C, "reg_MI_NDMA3FDATA");
				def_data("vu32", io + 0x00004170, "reg_MI_NDMA3CNT");

				def_data("vu16", io + 0x000001A0, "reg_MI_MCCNT0_A");
				def_data("vu16", io + 0x000001A2, "reg_MI_MCD0_A");
				def_data("vu32", io + 0x00100010, "reg_MI_MCD1_A");
				def_data("vu32", io + 0x000001A4, "reg_MI_MCCNT1_A");
				def_data("vu32", io + 0x000001A8, "reg_MI_MCCMD0_A");
				def_data("vu32", io + 0x000001AC, "reg_MI_MCCMD1_A");
				def_data("vu32", io + 0x000001B0, "reg_MI_MCSRC0_A");
				def_data("vu32", io + 0x000001B4, "reg_MI_MCSRC1_A");
				def_data("vu32", io + 0x000001B8, "reg_MI_MCSRC2_A");

				def_data("vu16", io + 0x000021A0, "reg_MI_MCCNT0_B");
				def_data("vu16", io + 0x000021A2, "reg_MI_MCD0_B");
				def_data("vu32", io + 0x00102010, "reg_MI_MCD1_B");
				def_data("vu32", io + 0x000021A4, "reg_MI_MCCNT1_B");
				def_data("vu32", io + 0x000021A8, "reg_MI_MCCMD0_B");
				def_data("vu32", io + 0x000021AC, "reg_MI_MCCMD1_B");
				def_data("vu32", io + 0x000021B0, "reg_MI_MCSRC0_B");
				def_data("vu32", io + 0x000021B4, "reg_MI_MCSRC1_B");
				def_data("vu32", io + 0x000021B8, "reg_MI_MCSRC2_B");
			}
			else
			{
				def_data("vu16", io + 0x000001A0, "reg_MI_MCCNT0");
				def_data("vu16", io + 0x000001A2, "reg_MI_MCD0");
				def_data("vu32", io + 0x00100010, "reg_MI_MCD1");
				def_data("vu32", io + 0x000001A4, "reg_MI_MCCNT1");
				def_data("vu32", io + 0x000001A8, "reg_MI_MCCMD0");
				def_data("vu32", io + 0x000001AC, "reg_MI_MCCMD1");
			}

			def_data("vu16", io + 0x00000204, "reg_MI_EXMEMCNT");

			if (dsi)
			{
				def_data("vu16", io + 0x00004010, "reg_MI_MC");
				def_data("vu32", io + 0x00004040, "reg_MI_MBK1");
				def_data("vu32", io + 0x00004044, "reg_MI_MBK2");
				def_data("vu32", io + 0x00004048, "reg_MI_MBK3");
				def_data("vu32", io + 0x0000404C, "reg_MI_MBK4");
				def_data("vu32", io + 0x00004050, "reg_MI_MBK5");
				def_data("vu32", io + 0x00004054, "reg_MI_MBK6");
				def_data("vu32", io + 0x00004058, "reg_MI_MBK7");
				def_data("vu32", io + 0x0000405C, "reg_MI_MBK8");
				def_data("vu32", io + 0x00004060, "reg_MI_MBK9");
			}
		}
		public void DefineCommonSymbols()
		{
			NDS.ROM rom = Util.Main.rom;
			CreateSymbol(rom.ARM9.Info.entryAddress, "_start");
			CreateSymbol(rom.ARM7.Info.entryAddress, "_start", "arm7_crt0");
			CreateSymbol(rom.ARM9StartModuleParamsAddress, "_start_ModuleParams");
			CreateSymbol(rom.ARM7StartModuleParamsAddress, "_start_ModuleParams", "arm7_crt0");
		}
		public void DefineCommonFunctions()
		{
			NDS.ROM rom = Util.Main.rom;
			CreateFunction(rom.ARM9.Info.entryAddress, "_start");
			CreateFunction(rom.ARM7.Info.entryAddress, "_start", "arm7_crt0");
		}
		private string FindOverlayName(uint address)
		{
			foreach (var s in ARM7Sections)
			{
				if (s.ContainsAddress(address))
					return s.name;
			}

			foreach (var s in OverlaySections)
			{
				if (s.ContainsAddress(address))
					return s.name;
			}

			return "";
		}
		public void DefineFunctionsFromFile(string path)
		{
			if (!File.Exists(path))
				return;

			bool duplicates = ParseSymbolFile(path, out Dictionary<string, string> symbols);
			string lastOverlay = "";

			// save duplicate-free symbols file 
			if (duplicates)
			{
				Util.SymbolsDir = Util.OutputDir + @"\exported_symbols.x";
				StreamWriter writer = File.CreateText(Util.SymbolsDir);

				Status.InitProgress();
				Status.SetMaxProgress(symbols.Count);
				int i = 0;

				// save symbols to file
				foreach (var v in symbols)
				{
					Status.SetStatusText(string.Format("Parsing symbols [{0:d}/{1:d}]", ++i, symbols.Count));

					string name = v.Value;
					DecodeAddressOverlayString(v.Key, out uint address, out string overlay);

					if (overlay != lastOverlay && overlay != string.Empty)
					{
						lastOverlay = overlay;

						writer.WriteLine();
						writer.WriteLine("/* {0:s} */", lastOverlay);
						writer.WriteLine();
					}

					writer.WriteLine("{0:s} = 0x{1:X8};", name, address);

					Status.IncrementProgress();
				}

				Status.FillProgress();

				writer.Close();
			}

			foreach (var v in symbols)
			{
				string name = v.Value;
				DecodeAddressOverlayString(v.Key, out uint address, out string overlay);

				if (overlay == "arm9")
					overlay = "";

				// arm7 sections are overlays, 
				// so they need to be named accordingly
				if (overlay.StartsWith("arm7"))
					overlay = FindOverlayName(address);

				// create symbols / functions
				CreateSymbol(address, name, overlay);
				if (Util.SymbolsAsFunctions)
					CreateFunction(address, name, overlay);

				Console.WriteLine("'{0:S}' => '{1:X}'", v.Value, v.Key);
			}
		}

		public static string EncodeAddressOverlayString(uint address, string overlay)
		{
			return string.Format("@{0:s}@{1:X8}", overlay, address);
		}
		public static void DecodeAddressOverlayString(string encoded, out uint address, out string overlay)
		{
			address = 0;
			overlay = "";

			if (string.IsNullOrEmpty(encoded))
				return;

			// worst case: "@a@00000000"
			if (encoded.Length > 11)
			{
				int addressPos = encoded.IndexOf('@', 1) + 1;

				overlay = encoded.Substring(1, addressPos - 2);
				string sAddress = encoded.Substring(addressPos);
				address = uint.Parse(sAddress, System.Globalization.NumberStyles.HexNumber);
			}
		}

		private bool ParseSymbolFile(string path, out Dictionary<string, string> result)
		{
			string[] lines = File.ReadLines(path).ToArray();

			//------ address symbol name(s)
			Dictionary<string, List<string>> symbols = new Dictionary<string, List<string>>();
			List<string> duplicates = new List<string>();

			Status.InitProgress();
			Status.SetMaxProgress(lines.Length);

			int i = 0;
			bool duplicatesFound = false;
			string lastOverlay = "arm9";

			foreach (string line in lines)
			{
				Status.SetStatusText(string.Format("Parsing symbols [{0:d}/{1:d}]", ++i, lines.Length));

				string l = line;

				if (l == string.Empty)
					continue;

				if (!ParseSymbolString(line, out string name, out uint address, out string overlay))
					continue;

				if (overlay != string.Empty)
				{
					lastOverlay = overlay;
					continue;
				}

				// we encode overlay and address since there could be multiple
				// symbols with the same address but in different overlays
				string encoded = EncodeAddressOverlayString(address, lastOverlay);

				// store duplicates
				if (symbols.ContainsKey(encoded))
				{
					if (!symbols[encoded].Contains(name))
					{
						symbols[encoded].Add(name);
						if(!duplicates.Contains(encoded))
							duplicates.Add(encoded);

						duplicatesFound = true;
					}
				}
				else
				{
					symbols[encoded] = new List<string>();
					symbols[encoded].Add(name);
				}

				Status.IncrementProgress();
			}

			Status.FillProgress();

			result = new Dictionary<string, string>();

			// fix duplicates
			if (duplicatesFound)
			{
				Status.SetStatusText(string.Format("Found {0:d} duplicates", duplicates.Count));

				DuplicatesFixWindow dfw = new DuplicatesFixWindow();
				dfw.Init(symbols, duplicates);
				dfw.ShowDialog(Util.Main);

				var unique = dfw.result;

				foreach (var v in symbols)
				{
					if (duplicates.Contains(v.Key))
					{
						// insert symbol chosen from duplicates window
						result[v.Key] = unique[v.Key];
					}
					else
					{
						result[v.Key] = v.Value[0];
					}
				}
			}
			else
			{
				foreach (var v in symbols)
				{
					result[v.Key] = v.Value[0];
				}
			}

			return duplicatesFound;
		}
		private bool ParseSymbolString(string input, out string output, out uint address, out string overlay)
		{
			output = "";
			address = 0;
			overlay = "";

			// remove all whitespaces
			input = input.Replace(" ", "");

			Console.WriteLine("line: '{0:s}'", input);

			if (input.Length < 2)
				return false;

			// skip comment lines
			if (input[0] == '/' && input[1] == '*')
			{
				int len = input.Length;
				if (input[len - 1] != '/' || input[len - 2] != '*')
					return false;

				len = input.Length - 4;
				string ov = input.Substring(2, len);

				if (ov.Length < 4)
					return false;

				Console.WriteLine("overlay: {0:s}", ov);

				overlay = ov;
				return true;
			}

			int i = 0;

			// read name
			for (; i < input.Length; i++)
			{
				if (input[i] == '=')
					break;
				output += input[i];
			}

			// skip other symbols ("=0x")
			i += 3;

			// read address
			string sAddress = "";
			for (; i < input.Length; i++)
			{
				if (input[i] == ';')
					break;
				sAddress += input[i];
			}

			if (sAddress.Length > 8)
			{
				Console.WriteLine("Invalid symbol: {0:s}", input);
				return false;
			}

			address = uint.Parse(sAddress, System.Globalization.NumberStyles.HexNumber);
			return true;
		}
	}
}
