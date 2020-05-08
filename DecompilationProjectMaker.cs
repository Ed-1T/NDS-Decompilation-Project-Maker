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
		}
		public MemorySection(uint startAddress, uint size, uint bssSize, string name, byte[] data)
		{
			info.startAddress = startAddress;
			info.size = size;
			info.bssSize = bssSize;
			this.name = name;
			this.data = data;
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
			if (Util.AutoNameSections)
			{
				if (section.info.startAddress < Util.Main.rom.ARM9.Info.ramAddress)
					name = "arm9_itcm";
				else if (section.info.startAddress == Util.Main.rom.ARM9.Info.ramAddress)
					name = "arm9_crt0";
				else if (section.info.startAddress == Util.Main.rom.ARM7.Info.ramAddress)
					name = "arm7_crt0";
			}

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
				var sec = Xml_CreateMemorySection(name + ".bss", section.GetBssAddress(), section.GetBssSize(), overlay, ovname);
				var frg = Xml_CreateFragment(name + ".bss", section.GetBssAddress(), section.GetBssSize(), overlay, ovname);
				
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
			foreach (MemorySection sec in sections)
				CreateMemorySection(sec, false);
		}
		public void CreateOverlayMemorySections(MemorySection[] sections)
		{
			foreach (MemorySection sec in sections)
				CreateMemorySection(sec, true, sec.name);
		}
		public void CreateARM7MemorySections(MemorySection[] sections)
		{
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
		public void DefineFunctionsFromFile(string path)
		{
			if (!File.Exists(path))
				return;

			bool duplicates = ParseSymbolFile(path, out Dictionary<uint, string> symbols);

			// save duplicate free symbols file 
			if (duplicates)
			{
				Util.SymbolsPath = Util.OutputPath + @"\exported_symbols.x";
				StreamWriter writer = File.AppendText(Util.SymbolsPath);

				// save symbols to file
				foreach (var v in symbols)
				{
					string name = v.Value;
					uint address = v.Key;

					writer.WriteLine("{0:s} = 0x{1:X8};", name, address);
					Console.WriteLine("writing: '{0:S}' => '{1:X}'", v.Value, v.Key);
				}

				writer.Close();
			}

			foreach (var v in symbols)
			{
				string name = v.Value;
				uint address = v.Key;

				// create symbols / functions
				CreateSymbol(address, name);
				if (Util.SymbolsAsFunctions)
					CreateFunction(address, name);

				Console.WriteLine("'{0:S}' => '{1:X}'", v.Value, v.Key);
			}
		}
		private bool ParseSymbolFile(string path, out Dictionary<uint, string> result)
		{
			string[] lines = File.ReadLines(path).ToArray();

			//------ address symbol name(s)
			Dictionary<uint, List<string>> symbols = new Dictionary<uint, List<string>>();
			List<uint> duplicates = new List<uint>();

			Status.InitProgress();
			Status.DivideProgress(lines.Length);

			int i = 0;
			bool duplicatesFound = false;

			foreach (string line in lines)
			{
				Status.SetStatusText(string.Format("Parsing symbols [{0:d}/{1:d}]", ++i, lines.Length));

				string l = line;

				if (l == string.Empty)
					continue;

				if (!ParseSymbolString(line, out string name, out uint address))
					continue;

				// store duplicates
				if (symbols.ContainsKey(address))
				{
					if (!symbols[address].Contains(name))
					{
						symbols[address].Add(name);
						if(!duplicates.Contains(address))
							duplicates.Add(address);
					}

					duplicatesFound = true;
				}
				else
				{
					symbols[address] = new List<string>();
					symbols[address].Add(name);
				}

				Status.IncrementProgress();
			}

			Status.FillProgress();

			result = new Dictionary<uint, string>();

			// fix duplicates
			if (duplicatesFound)
			{
				Status.SetStatusText(string.Format("Found {0:d} duplicates", duplicates.Count));

				DuplicatesFixWindow dfw = new DuplicatesFixWindow();
				dfw.Init(symbols, duplicates);
				dfw.ShowDialog(Util.Main);

				Dictionary<uint, string> unique = dfw.GetResult();

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
		private bool ParseSymbolString(string input, out string output, out uint address)
		{
			output = "";
			address = 0;

			// remove all whitespaces
			input = input.Replace(" ", "");

			// skip comment lines
			if (input[0] == '/' && input[1] == '*')
				return false;

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
