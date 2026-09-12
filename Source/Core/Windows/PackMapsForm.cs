
#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	/// <summary>
	/// Tools → Pack maps into WAD… — merge single-map PWADs into a multi-map megawad.
	/// </summary>
	internal class PackMapsForm : DelayedForm
	{
		private ListView list;
		private TextBox outputBox;
		private TextBox worldLuaBox;
		private Button ok;
		private Button cancel;

		public PackMapsForm()
		{
			this.Text = "Pack maps into WAD";
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.StartPosition = FormStartPosition.CenterParent;
			this.ClientSize = new Size(560, 420);

			Label hint = new Label();
			hint.Text = "Combine single-map .wad / .mapwad files into one multi-map PWAD (MAP01, MAP02, …).\r\n" +
				"Day-to-day Carcosa editing: open carcosa.wad and Save Map instead. This dialog replaces Gradle merge for authors.";
			hint.Location = new Point(12, 10);
			hint.Size = new Size(536, 40);
			hint.ForeColor = Color.DimGray;

			list = new ListView();
			list.Location = new Point(12, 56);
			list.Size = new Size(536, 200);
			list.View = View.Details;
			list.FullRowSelect = true;
			list.GridLines = true;
			list.Columns.Add("MAP##", 70);
			list.Columns.Add("Source file", 440);

			Button addFiles = new Button();
			addFiles.Text = "Add files…";
			addFiles.Location = new Point(12, 264);
			addFiles.Size = new Size(100, 26);
			addFiles.Click += addFiles_Click;

			Button addFolder = new Button();
			addFolder.Text = "Add folder…";
			addFolder.Location = new Point(118, 264);
			addFolder.Size = new Size(100, 26);
			addFolder.Click += addFolder_Click;

			Button remove = new Button();
			remove.Text = "Remove";
			remove.Location = new Point(224, 264);
			remove.Size = new Size(80, 26);
			remove.Click += remove_Click;

			Button moveUp = new Button();
			moveUp.Text = "Up";
			moveUp.Location = new Point(310, 264);
			moveUp.Size = new Size(50, 26);
			moveUp.Click += delegate { MoveSelected(-1); };

			Button moveDown = new Button();
			moveDown.Text = "Down";
			moveDown.Location = new Point(366, 264);
			moveDown.Size = new Size(50, 26);
			moveDown.Click += delegate { MoveSelected(1); };

			Label lo = new Label();
			lo.Text = "Output:";
			lo.Location = new Point(12, 302);
			lo.AutoSize = true;
			outputBox = new TextBox();
			outputBox.Location = new Point(70, 298);
			outputBox.Size = new Size(390, 24);
			outputBox.Text = "carcosa.wad";
			Button browseOut = new Button();
			browseOut.Text = "…";
			browseOut.Location = new Point(466, 296);
			browseOut.Size = new Size(40, 26);
			browseOut.Click += browseOut_Click;

			Label lw = new Label();
			lw.Text = "World Lua (CARCWLD on first map, optional):";
			lw.Location = new Point(12, 334);
			lw.AutoSize = true;
			worldLuaBox = new TextBox();
			worldLuaBox.Location = new Point(12, 354);
			worldLuaBox.Size = new Size(448, 24);
			Button browseLua = new Button();
			browseLua.Text = "…";
			browseLua.Location = new Point(466, 352);
			browseLua.Size = new Size(40, 26);
			browseLua.Click += browseLua_Click;

			ok = new Button();
			ok.Text = "Pack";
			ok.Location = new Point(372, 386);
			ok.Size = new Size(90, 26);
			ok.Click += ok_Click;
			cancel = new Button();
			cancel.Text = "Cancel";
			cancel.Location = new Point(468, 386);
			cancel.Size = new Size(80, 26);
			cancel.DialogResult = DialogResult.Cancel;

			this.Controls.Add(hint);
			this.Controls.Add(list);
			this.Controls.Add(addFiles);
			this.Controls.Add(addFolder);
			this.Controls.Add(remove);
			this.Controls.Add(moveUp);
			this.Controls.Add(moveDown);
			this.Controls.Add(lo);
			this.Controls.Add(outputBox);
			this.Controls.Add(browseOut);
			this.Controls.Add(lw);
			this.Controls.Add(worldLuaBox);
			this.Controls.Add(browseLua);
			this.Controls.Add(ok);
			this.Controls.Add(cancel);
			this.AcceptButton = ok;
			this.CancelButton = cancel;
			this.ClientSize = new Size(560, 424);
		}

		private void addFiles_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Doom WAD maps|*.wad;*.mapwad|All files|*.*";
			dlg.Multiselect = true;
			dlg.Title = "Add map WADs";
			if (dlg.ShowDialog(this) != DialogResult.OK) return;
			foreach (string path in dlg.FileNames)
				AddPath(path);
			RenumberMarkers();
		}

		private void addFolder_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.Description = "Folder of .wad / .mapwad files";
			if (dlg.ShowDialog(this) != DialogResult.OK) return;
			string[] files = Directory.GetFiles(dlg.SelectedPath, "*.*");
			Array.Sort(files, StringComparer.OrdinalIgnoreCase);
			foreach (string path in files)
			{
				string ext = Path.GetExtension(path);
				if (string.Equals(ext, ".wad", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(ext, ".mapwad", StringComparison.OrdinalIgnoreCase))
					AddPath(path);
			}
			RenumberMarkers();
		}

		private void AddPath(string path)
		{
			foreach (ListViewItem existing in list.Items)
			{
				if (string.Equals(existing.SubItems[1].Text, path, StringComparison.OrdinalIgnoreCase))
					return;
			}
			ListViewItem item = new ListViewItem("MAP01");
			item.SubItems.Add(path);
			list.Items.Add(item);
		}

		private void remove_Click(object sender, EventArgs e)
		{
			while (list.SelectedItems.Count > 0)
				list.Items.Remove(list.SelectedItems[0]);
			RenumberMarkers();
		}

		private void MoveSelected(int delta)
		{
			if (list.SelectedItems.Count != 1) return;
			int index = list.SelectedItems[0].Index;
			int target = index + delta;
			if (target < 0 || target >= list.Items.Count) return;
			ListViewItem item = list.Items[index];
			list.Items.RemoveAt(index);
			list.Items.Insert(target, item);
			item.Selected = true;
			RenumberMarkers();
		}

		private void RenumberMarkers()
		{
			for (int i = 0; i < list.Items.Count; i++)
				list.Items[i].Text = "MAP" + (i + 1).ToString("00");
		}

		private void browseOut_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "PWAD|*.wad|All files|*.*";
			dlg.FileName = Path.GetFileName(outputBox.Text);
			dlg.Title = "Output megawad";
			if (dlg.ShowDialog(this) == DialogResult.OK)
				outputBox.Text = dlg.FileName;
		}

		private void browseLua_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Lua|*.lua|All files|*.*";
			dlg.Title = "World script (CARCWLD)";
			if (dlg.ShowDialog(this) == DialogResult.OK)
				worldLuaBox.Text = dlg.FileName;
		}

		private void ok_Click(object sender, EventArgs e)
		{
			if (list.Items.Count == 0)
			{
				MessageBox.Show(this, "Add at least one source map.", Application.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			string outPath = outputBox.Text.Trim();
			if (outPath.Length == 0)
			{
				MessageBox.Show(this, "Choose an output path.", Application.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (!Path.IsPathRooted(outPath))
				outPath = Path.GetFullPath(outPath);

			List<CarcosaMultiMapPacker.MapEntry> entries = new List<CarcosaMultiMapPacker.MapEntry>();
			foreach (ListViewItem item in list.Items)
			{
				CarcosaMultiMapPacker.MapEntry entry = new CarcosaMultiMapPacker.MapEntry();
				entry.MapName = item.Text;
				entry.SourcePath = item.SubItems[1].Text;
				entries.Add(entry);
			}

			string worldLua = null;
			string luaPath = worldLuaBox.Text.Trim();
			if (luaPath.Length > 0)
			{
				if (!File.Exists(luaPath))
				{
					MessageBox.Show(this, "World Lua file not found:\n" + luaPath, Application.ProductName,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				worldLua = File.ReadAllText(luaPath, Encoding.UTF8);
			}

			try
			{
				string dir = Path.GetDirectoryName(outPath);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				CarcosaMultiMapPacker.Pack(entries, outPath, worldLua);
				MessageBox.Show(this,
					"Wrote " + outPath + "\n(" + entries.Count + " maps)",
					Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Pack failed:\n" + ex.Message, Application.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
