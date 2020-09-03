using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NDSDecompilationProjectMaker
{
	public partial class DuplicatesFixWindow : Form
	{
		public struct SymbolState
		{
			public string address;
			public string curSymbol;

			public string[] symbols;
			public bool fix;
		}

		public SymbolState[] symbols;
		private int numSymbolsFixed;
		private int curSymbolIndex;

		public Dictionary<string, string> result
		{
			get
			{
				Dictionary<string, string> res = new Dictionary<string, string>();
				foreach (var s in symbols)
					res.Add(s.address, s.curSymbol);
				return res;
			}
		}

		public DuplicatesFixWindow()
		{
			InitializeComponent();
			HintLabel.Text = "";
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | 0x200;
				return myCp;
			}
		}

		private	string GetAddressString(string adr)
		{
			DecompilationProjectMaker.DecodeAddressOverlayString(adr, out uint address, out string overlay);
			if (overlay != "")
				overlay += "::";

			return string.Format("{0:s}0x{1:X08}", overlay, address);
		}

		private void UpdateUI()
		{
			Text = string.Format("Fixing duplicate symbols [{0:d}/{1:d}]", curSymbolIndex+1, symbols.Length);

			PreviousButton.Enabled = true;
			PreviousButton.Image = Properties.Resources.arrow_left_e;
			NextButton.Enabled = true;
			NextButton.Image = Properties.Resources.arrow_right_e;
			if (curSymbolIndex == 0)
			{
				PreviousButton.Enabled = false;
				PreviousButton.Image = Properties.Resources.arrow_left;
			}
			else if (curSymbolIndex == symbols.Length - 1)
			{
				NextButton.Enabled = false;
				NextButton.Image = Properties.Resources.arrow_right;
			}
		}
		private void UpdateTree()
		{
			if (symbols[curSymbolIndex].fix)
			{
				DuplicatesTree.BeginUpdate();

				DuplicatesTree.Nodes.Clear();
				TreeNode node = new TreeNode();

				node.Text = string.Format("{0} => {1}", GetAddressString(symbols[curSymbolIndex].address), symbols[curSymbolIndex].curSymbol);
				node.Tag = (string)symbols[curSymbolIndex].address; 
				DuplicatesTree.Nodes.Add(node);

				HintLabel.Text = "Double-click to edit";

				DuplicatesTree.EndUpdate();
				return;
			}

			HintLabel.Text = "";

			DuplicatesTree.BeginUpdate();

			DuplicatesTree.Nodes.Clear();
			DuplicatesTree.Nodes.Add(GetAddressString(symbols[curSymbolIndex].address));

			foreach (var s in symbols[curSymbolIndex].symbols)
			{
				DuplicatesTree.Nodes[0].Nodes.Add(s);
			}

			DuplicatesTree.Nodes[0].ExpandAll();

			DuplicatesTree.EndUpdate();
		}

		public void Init(Dictionary<string, List<string>> inSymbols, List<string> inDuplicates)
		{
			if (inSymbols.Count == 0 || inDuplicates.Count == 0)
			{
				Close();
				return;
			}

			symbols = new SymbolState[inDuplicates.Count];

			int i = 0;
			foreach(var adr in inDuplicates)
			{
				symbols[i] = new SymbolState();
				symbols[i].address = adr;
				symbols[i].curSymbol = "";
				symbols[i].symbols = inSymbols[adr].ToArray();
				symbols[i].fix = false;
				i++;
			}

			numSymbolsFixed = 0;

			UpdateUI();
			UpdateTree();
		}


		private void PreviousButton_Click(object sender, EventArgs e)
		{
			curSymbolIndex -= 1;

			UpdateUI();
			UpdateTree();
		}
		private void NextButton_Click(object sender, EventArgs e)
		{
			curSymbolIndex += 1;

			UpdateUI();
			UpdateTree();
		}

		private void DuplicatesTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Parent == null)
			{
				if (symbols[curSymbolIndex].fix)
				{
					symbols[curSymbolIndex].fix = false;
					numSymbolsFixed--;
					UpdateTree();
				}
				return;
			}

			symbols[curSymbolIndex].curSymbol = e.Node.Text;
			symbols[curSymbolIndex].fix = true;
			numSymbolsFixed++;

			if (numSymbolsFixed >= symbols.Length)
			{
				Close();
				return;
			}

			if (curSymbolIndex < symbols.Length - 1)
				curSymbolIndex++;

			UpdateUI();
			UpdateTree();
		}
	}
}
