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
		private Dictionary<string, List<string>> symbols;
		private List<string> duplicates;

		private List<string> lSymbolsFixed;
		private Dictionary<string, string> result;

		private int symbolsFixed;
		private int currentSymbol;

		public DuplicatesFixWindow()
		{
			InitializeComponent();
			lSymbolsFixed = new List<string>();
			result = new Dictionary<string, string>();
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

		private void UpdateButtons()
		{
			Text = string.Format("Fixing duplicate symbols [{0:d}/{1:d}]", currentSymbol+1, duplicates.Count);

			PreviousButton.Enabled = true;
			PreviousButton.Image = Properties.Resources.arrow_left_e;
			NextButton.Enabled = true;
			NextButton.Image = Properties.Resources.arrow_right_e;
			if (currentSymbol == 0)
			{
				PreviousButton.Enabled = false;
				PreviousButton.Image = Properties.Resources.arrow_left;
			}
			else if (currentSymbol == duplicates.Count - 1)
			{
				NextButton.Enabled = false;
				NextButton.Image = Properties.Resources.arrow_right;
			}
		}
		private void UpdateTree()
		{
			if (lSymbolsFixed.Contains(duplicates[currentSymbol]))
			{
				DuplicatesTree.BeginUpdate();

				DuplicatesTree.Nodes.Clear();
				TreeNode node = new TreeNode();
				node.Text = string.Format("0x{0:s} => {1:s}", duplicates[currentSymbol], result[duplicates[currentSymbol]]);
				node.Tag = (string)duplicates[currentSymbol]; 
				DuplicatesTree.Nodes.Add(node);

				HintLabel.Text = "Double-click to edit";

				DuplicatesTree.EndUpdate();
				return;
			}

			HintLabel.Text = "";

			DuplicatesTree.BeginUpdate();

			DuplicatesTree.Nodes.Clear();
			DuplicatesTree.Nodes.Add(string.Format("0x{0:X}", duplicates[currentSymbol]));

			foreach (var s in symbols[duplicates[currentSymbol]])
			{
				DuplicatesTree.Nodes[0].Nodes.Add(s);
			}

			DuplicatesTree.Nodes[0].ExpandAll();

			DuplicatesTree.EndUpdate();
		}

		public void Init(Dictionary<string, List<string>> symbols, List<string> duplicates)
		{
			this.symbols = symbols;
			this.duplicates = duplicates;

			if (symbols.Count == 0 || duplicates.Count == 0)
			{
				Close();
				return;
			}

			symbolsFixed = 0;
			result.Clear();

			UpdateEmptyEntries();
			UpdateButtons();
			UpdateTree();
		}

		public Dictionary<string, string> GetResult()
		{
			return result;
		}

		private int freeLeft;
		private int freeRight;

		private void UpdateEmptyEntries()
		{
			freeLeft = 1;
			freeRight = 1;

			// check for empty entries to the left
			if (currentSymbol > 0)
			{
				for (int i = currentSymbol; i > 0; i--)
				{
					if (!lSymbolsFixed.Contains(duplicates[i]))
						break;
					else
						freeLeft++;
				}
			}

			// check for empty entries to the right
			if (currentSymbol < duplicates.Count)
			{
				for (int i = currentSymbol; i < duplicates.Count; i++)
				{
					if (!lSymbolsFixed.Contains(duplicates[i]))
						break;
					else
						freeRight++;
				}
			}
		}
		private void PreviousButton_Click(object sender, EventArgs e)
		{
			currentSymbol -= 1;
			//UpdateEmptyEntries();
			UpdateButtons();
			UpdateTree();
		}
		private void NextButton_Click(object sender, EventArgs e)
		{
			currentSymbol += 1;
			//UpdateEmptyEntries();
			UpdateButtons();
			UpdateTree();
		}

		private void DuplicatesTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Parent == null)
			{
				lSymbolsFixed.Remove((string)e.Node.Tag);
				result.Remove((string)e.Node.Tag);
				UpdateTree();
				return;
			}

			result[duplicates[currentSymbol]] = e.Node.Text;

			symbolsFixed++;
			lSymbolsFixed.Add(duplicates[currentSymbol]);

			if (symbolsFixed >= duplicates.Count)
			{
				Close();
				return;
			}

			UpdateEmptyEntries();

			if (currentSymbol < duplicates.Count - 1)
				currentSymbol++;

			UpdateButtons();
			UpdateTree();
		}
	}
}
