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
			Document.Save(Util.OutputPath + @"\out.xml");
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
						new XAttribute( "FILE_NAME",  (name + ".bytes")),
						new XAttribute( "FILE_OFFSET",  string.Format("0x{0:X}",  0))
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
				var frg = Xml_CreateFragment(name, section.GetAddress(), section.GetSize(), overlay, ovname);
				
				if (section.data != null)
				{
					sec.Add(Xml_CreateMemoryContents(name));

					string output = Util.OutputPath + @"\" + name + ".bytes";
					BinaryWriter bwr = new BinaryWriter(File.Open(output, FileMode.Create));
					bwr.Write(section.data, 0, (int)section.GetSize());
					bwr.Close();
				}

				mmap.Add(sec);
				tree.Add(frg);
			}

			if (section.info.bssSize != 0)
			{
				var sec = Xml_CreateMemorySection(name + "_bss", section.GetBssAddress(), section.GetBssSize(), overlay, ovname);
				var frg = Xml_CreateFragment(name + "_bss", section.GetBssAddress(), section.GetBssSize(), overlay, ovname);
				
				if (Util.FillBSS)
				{
					sec.Add(Xml_CreateMemoryContents(name));

					byte[] buffer = new byte[section.GetBssSize()];

					for (int i = 0; i < section.GetBssSize(); i++)
					{
						buffer[i] = Util.FillValue;
					}

					string output = Util.OutputPath + @"\" + name + "_bss.bytes";
					BinaryWriter bwr = new BinaryWriter(File.Open(output, FileMode.Create));
					bwr.Write(buffer, 0, (int)section.GetBssSize());
					bwr.Close();
				}

				mmap.Add(sec);
				tree.Add(frg);
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
			var frg = Xml_CreateFragment(name, address, size);

			mmap.Add(sec);
			tree.Add(frg);
		}
		public void FillMemoryGaps()
		{
			// the program currently creates 
			// sections like this:
			// ---------------------------------------
			// code section 1
			// bss section 1
			//					<----
			// code section 2
			// bss section 2
			// ...				<----
			// this function fills the gaps that might 
			// appear between bss and code sections

			ARM9MemoryGaps = new List<MemorySection>();
			ARM9MemoryGaps.AddRange((MemorySection[])ARM9Sections.Clone());
			ARM9MemoryGaps = ARM9MemoryGaps.OrderBy(o => o.GetAddress()).ToList();

			uint lastEnd = uint.MaxValue;
			string lastName = "";

			foreach (var sec in ARM9MemoryGaps)
			{
				if (sec.GetAddress() >= 0x27E0000)
					break;

				if (lastEnd != uint.MaxValue)
				{
					CreateSectionManual(lastName, lastEnd, sec.GetAddress() - lastEnd);
				}
				lastEnd = sec.GetAddress() + sec.GetSize();
				lastName = sec.name + "_free";
			}

			CreateSectionManual(lastName, lastEnd, 0x27E0000 - lastEnd);
		}

		// symbol definition functions
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
		public void DefineCommonSymbols()
		{
			NDS.ROM rom = Util.Main.rom;
			CreateSymbol(rom.ARM9.Info.entryAddress, "_start");
			CreateSymbol(rom.ARM7.Info.entryAddress, "_start_arm7", "arm7_crt0");
			CreateSymbol(rom.ARM9StartModuleParamsAddress, "_start_ModuleParams");
			CreateSymbol(rom.ARM7StartModuleParamsAddress, "_start_arm7_ModuleParams", "arm7_crt0");
		}
		public void DefineCommonFunctions()
		{
			NDS.ROM rom = Util.Main.rom;
			CreateFunction(rom.ARM9.Info.entryAddress, "_start");
			CreateFunction(rom.ARM7.Info.entryAddress, "_start_arm7", "arm7_crt0");
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

			// save duplicate free symbols file 
			if (duplicates)
			{
				Util.SymbolsPath = Util.OutputPath + @"\exported_symbols.x";
				StreamWriter writer = File.AppendText(Util.SymbolsPath);

				// save symbols to file
				foreach (var v in symbols)
				{
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
					Console.WriteLine("writing: '{0:S}' => '{1:X}'", v.Value, v.Key);
				}

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

		private string EncodeAddressOverlayString(uint address, string overlay)
		{
			return string.Format("@{0:s}@{1:X8}", overlay, address);
		}
		private void DecodeAddressOverlayString(string encoded, out uint address, out string overlay)
		{
			address = 0;
			overlay = "";

			// worst case: "@@00000000"
			if (encoded.Length > 10)
			{
				int addressPos = encoded.IndexOf('@', 1) + 1;

				overlay = encoded.Substring(1, addressPos - 2);
				string sAddress = encoded.Substring(addressPos);
				address = uint.Parse(sAddress, System.Globalization.NumberStyles.HexNumber);

				Console.WriteLine("encoded: {0:s}", encoded);
				Console.WriteLine("address at: {0:D}", addressPos);
				Console.WriteLine("overlay: {0:s}", overlay);
				Console.WriteLine("address: {0:X}", address);
			}
		}

		private bool ParseSymbolFile(string path, out Dictionary<string, string> result)
		{
			string[] lines = File.ReadLines(path).ToArray();

			//------ address symbol name(s)
			Dictionary<string, List<string>> symbols = new Dictionary<string, List<string>>();
			List<string> duplicates = new List<string>();

			Status.InitProgress();
			Status.DivideProgress(lines.Length);

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

				if (overlay != lastOverlay && overlay != string.Empty)
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
					}

					duplicatesFound = true;
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

				Dictionary<string, string> unique = dfw.GetResult();

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
