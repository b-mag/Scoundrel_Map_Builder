
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.IO;
using System.IO;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Rendering; // villsa

#endregion

namespace CodeImp.DoomBuilder.Windows
{
    internal partial class SectorEditForm : DelayedForm
    {
        // Variables
        private ICollection<Sector> sectors;
        private PixelColor color;
        private PixelColor[] initialcolor;
        private const float LIGHTINCVALUE = 0.235f;
        private const float LIGHTDECVALUE = -0.1825f;

        private ComboBox carcosaWeather;
        private ComboBox carcosaMood;
        private NumericUpDown carcosaDensity;
        private NumericUpDown carcosaFogStart;
        private NumericUpDown carcosaFogMax;
        private Button carcosaFogColor;
        private CheckBox carcosaOutdoors;
        private TextBox carcosaPlace;
        private bool carcosaPanelReady;

        // Constructor
        public SectorEditForm()
        {
            // Initialize
            InitializeComponent();

            color = new PixelColor(255, 255, 255, 255);
            initialcolor = new PixelColor[Sector.NUM_COLORS];

            // Fill effects list
            effect.AddInfo(General.Map.Config.SortedSectorEffects.ToArray());

            // villsa
            if (General.Map.FormatInterface.InDoom64Mode)
            {
                // Fill flags list
                foreach (KeyValuePair<string, string> lf in General.Map.Config.SectorFlags)
                    flags.Add(lf.Value, lf.Key);

                groupeffect.Height = 64;
                groupaction.Top = groupeffect.Bottom + groupeffect.Margin.Bottom + groupaction.Margin.Top;
                settingsgroup.Top = groupaction.Bottom + groupaction.Margin.Bottom + settingsgroup.Margin.Top;
                this.Height = settingsgroup.Bottom + settingsgroup.Margin.Bottom + 120;
            }

            // Initialize image selectors
            floortex.Initialize();
            ceilingtex.Initialize();
            this.Height = heightpanel3.Height;

            if (General.Map.FormatInterface.InDoom64Mode)
                BuildCarcosaPanel();
        }

        private void BuildCarcosaPanel()
        {
            GroupBox box = new GroupBox();
            box.Text = " Carcosa atmosphere ";
            box.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            box.Location = new Point(9, settingsgroup.Bottom + 8);
            box.Size = new Size(545, 205);
            box.TabIndex = 20;

            Label lw = new Label();
            lw.Text = "Weather:";
            lw.Location = new Point(12, 28);
            lw.AutoSize = true;
            carcosaWeather = new ComboBox();
            carcosaWeather.DropDownStyle = ComboBoxStyle.DropDownList;
            carcosaWeather.Location = new Point(90, 24);
            carcosaWeather.Size = new Size(150, 24);
            carcosaWeather.Items.AddRange(new object[] { "Clear", "Rain", "Fog", "Mist", "Storm" });
            carcosaWeather.SelectedIndex = 0;

            Label lm = new Label();
            lm.Text = "Mood:";
            lm.Location = new Point(260, 28);
            lm.AutoSize = true;
            carcosaMood = new ComboBox();
            carcosaMood.DropDownStyle = ComboBoxStyle.DropDownList;
            carcosaMood.Location = new Point(310, 24);
            carcosaMood.Size = new Size(210, 24);
            carcosaMood.Items.AddRange(new object[] {
                "Default", "Shore", "Cemetery", "Fog bank", "Forest", "Indoor", "Lava", "Nitre", "Blood"
            });
            carcosaMood.SelectedIndex = 0;

            Label ld = new Label();
            ld.Text = "Fog dens:";
            ld.Location = new Point(12, 64);
            ld.AutoSize = true;
            carcosaDensity = new NumericUpDown();
            carcosaDensity.DecimalPlaces = 2;
            carcosaDensity.Increment = 0.01M;
            carcosaDensity.Minimum = 0;
            carcosaDensity.Maximum = 0.35M;
            carcosaDensity.Location = new Point(90, 60);
            carcosaDensity.Size = new Size(70, 24);

            Label ls = new Label();
            ls.Text = "Start:";
            ls.Location = new Point(170, 64);
            ls.AutoSize = true;
            carcosaFogStart = new NumericUpDown();
            carcosaFogStart.Maximum = 4000;
            carcosaFogStart.Location = new Point(210, 60);
            carcosaFogStart.Size = new Size(70, 24);

            Label lx = new Label();
            lx.Text = "Max:";
            lx.Location = new Point(290, 64);
            lx.AutoSize = true;
            carcosaFogMax = new NumericUpDown();
            carcosaFogMax.Maximum = 4000;
            carcosaFogMax.Location = new Point(325, 60);
            carcosaFogMax.Size = new Size(70, 24);

            Label lc = new Label();
            lc.Text = "Fog RGB:";
            lc.Location = new Point(12, 100);
            lc.AutoSize = true;
            carcosaFogColor = new Button();
            carcosaFogColor.Location = new Point(90, 94);
            carcosaFogColor.Size = new Size(70, 28);
            carcosaFogColor.BackColor = Color.FromArgb(140, 132, 122);
            carcosaFogColor.Click += carcosaFogColor_Click;

            carcosaOutdoors = new CheckBox();
            carcosaOutdoors.Text = "Outdoors";
            carcosaOutdoors.Location = new Point(180, 98);
            carcosaOutdoors.AutoSize = true;
            carcosaOutdoors.Checked = true;

            Label lp = new Label();
            lp.Text = "Place:";
            lp.Location = new Point(12, 140);
            lp.AutoSize = true;
            carcosaPlace = new TextBox();
            carcosaPlace.Location = new Point(90, 136);
            carcosaPlace.Size = new Size(430, 24);
            carcosaPlace.MaxLength = 21;

            Label hint = new Label();
            hint.Text = "Writes the CARCOSA lump. Stock maps without extras stay unchanged.";
            hint.Location = new Point(12, 170);
            hint.AutoSize = true;
            hint.ForeColor = Color.DimGray;

            box.Controls.Add(lw);
            box.Controls.Add(carcosaWeather);
            box.Controls.Add(lm);
            box.Controls.Add(carcosaMood);
            box.Controls.Add(ld);
            box.Controls.Add(carcosaDensity);
            box.Controls.Add(ls);
            box.Controls.Add(carcosaFogStart);
            box.Controls.Add(lx);
            box.Controls.Add(carcosaFogMax);
            box.Controls.Add(lc);
            box.Controls.Add(carcosaFogColor);
            box.Controls.Add(carcosaOutdoors);
            box.Controls.Add(lp);
            box.Controls.Add(carcosaPlace);
            box.Controls.Add(hint);

            sectorproperties.Controls.Add(box);
            sectorproperties.Height = box.Bottom + 12;
            this.ClientSize = new Size(this.ClientSize.Width, sectorproperties.Bottom + 56);
            carcosaPanelReady = true;
        }

        private void carcosaFogColor_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = carcosaFogColor.BackColor;
            dlg.FullOpen = true;
            if (dlg.ShowDialog(this) == DialogResult.OK)
                carcosaFogColor.BackColor = dlg.Color;
        }

        // This sets up the form to edit the given sectors
        public void Setup(ICollection<Sector> sectors)
        {
            Sector sc;

            // Keep this list
            this.sectors = sectors;
            if (sectors.Count > 1) this.Text = "Edit Sectors (" + sectors.Count + ")";

            ////////////////////////////////////////////////////////////////////////
            // Set all options to the first sector properties
            ////////////////////////////////////////////////////////////////////////

            // Get first sector
            sc = General.GetByIndex(sectors, 0);

            if (General.Map.FormatInterface.InDoom64Mode)
            {
                // villsa - Flags
                foreach (CheckBox c in flags.Checkboxes)
                    if (sc.Flags.ContainsKey(c.Tag.ToString())) c.Checked = sc.Flags[c.Tag.ToString()];

                ceilingcolor.Color = initialcolor[0] = sc.CeilColor.color;
                topcolor.Color = initialcolor[1] = sc.TopColor.color;
                thingcolor.Color = initialcolor[2] = sc.ThingColor.color;
                lowercolor.Color = initialcolor[3] = sc.LowerColor.color;
                floorcolor.Color = initialcolor[4] = sc.FloorColor.color;
            }

            // Effects
            effect.Value = sc.Effect;

            // Floor/ceiling
            floorheight.Text = sc.FloorHeight.ToString();
            ceilingheight.Text = sc.CeilHeight.ToString();
            floortex.TextureName = sc.FloorTexture;
            ceilingtex.TextureName = sc.CeilTexture;

            // Action
            tag.Text = sc.Tag.ToString();

            if (carcosaPanelReady)
            {
                carcosaWeather.SelectedIndex = Math.Max(0, Math.Min(4, sc.CarcosaWeather));
                carcosaMood.SelectedIndex = Math.Max(0, Math.Min(8, sc.CarcosaMood));
                decimal dens = (decimal)sc.CarcosaFogDensity;
                if (dens < carcosaDensity.Minimum) dens = carcosaDensity.Minimum;
                if (dens > carcosaDensity.Maximum) dens = carcosaDensity.Maximum;
                carcosaDensity.Value = dens;
                decimal st = (decimal)sc.CarcosaFogStart;
                if (st < carcosaFogStart.Minimum) st = carcosaFogStart.Minimum;
                if (st > carcosaFogStart.Maximum) st = carcosaFogStart.Maximum;
                carcosaFogStart.Value = st;
                decimal mx = (decimal)sc.CarcosaFogMax;
                if (mx < carcosaFogMax.Minimum) mx = carcosaFogMax.Minimum;
                if (mx > carcosaFogMax.Maximum) mx = carcosaFogMax.Maximum;
                carcosaFogMax.Value = mx;
                carcosaFogColor.BackColor = Color.FromArgb(
                    Math.Max(0, Math.Min(255, sc.CarcosaFogR)),
                    Math.Max(0, Math.Min(255, sc.CarcosaFogG)),
                    Math.Max(0, Math.Min(255, sc.CarcosaFogB)));
                carcosaOutdoors.Checked = sc.CarcosaOutdoors;
                carcosaPlace.Text = sc.CarcosaPlaceName;
            }

            ////////////////////////////////////////////////////////////////////////
            // Now go for all sectors and change the options when a setting is different
            ////////////////////////////////////////////////////////////////////////

            // Go for all sectors
            foreach (Sector s in sectors)
            {
                // Flags
                foreach (CheckBox c in flags.Checkboxes)
                {
                    if (s.Flags.ContainsKey(c.Tag.ToString()))
                    {
                        if (s.Flags[c.Tag.ToString()] != c.Checked)
                        {
                            c.ThreeState = true;
                            c.CheckState = CheckState.Indeterminate;
                        }
                    }
                }

                // Effects
                if (s.Effect != effect.Value) effect.Empty = true;

                // Floor/Ceiling
                if (s.FloorHeight.ToString() != floorheight.Text) floorheight.Text = "";
                if (s.CeilHeight.ToString() != ceilingheight.Text) ceilingheight.Text = "";
                if (s.FloorTexture != floortex.TextureName) floortex.TextureName = "";
                if (s.CeilTexture != ceilingtex.TextureName) ceilingtex.TextureName = "";

                // Action
                if (s.Tag.ToString() != tag.Text) tag.Text = "";
            }

            // Show sector height
            UpdateSectorHeight();
        }

        // This updates the sector height field
        private void UpdateSectorHeight()
        {
            bool showheight = true;
            int delta = 0;
            Sector first = null;

            // Check all selected sectors
            foreach (Sector s in sectors)
            {
                if (first == null)
                {
                    // First sector in list
                    delta = s.CeilHeight - s.FloorHeight;
                    showheight = true;
                    first = s;
                }
                else
                {
                    if (delta != (s.CeilHeight - s.FloorHeight))
                    {
                        // We can't show heights because the delta
                        // heights for the sectors is different
                        showheight = false;
                        break;
                    }
                }
            }

            if (showheight)
            {
                int fh = floorheight.GetResult(first.FloorHeight);
                int ch = ceilingheight.GetResult(first.CeilHeight);
                int height = ch - fh;
                sectorheight.Text = height.ToString();
                sectorheight.Visible = true;
                sectorheightlabel.Visible = true;
            }
            else
            {
                sectorheight.Visible = false;
                sectorheightlabel.Visible = false;
            }
        }

        // OK clicked
        private void apply_Click(object sender, EventArgs e)
        {
            string undodesc = "sector";

            // Verify the tag
            if ((tag.GetResult(0) < General.Map.FormatInterface.MinTag) || (tag.GetResult(0) > General.Map.FormatInterface.MaxTag))
            {
                General.ShowWarningMessage("Sector tag must be between " + General.Map.FormatInterface.MinTag + " and " + General.Map.FormatInterface.MaxTag + ".", MessageBoxButtons.OK);
                return;
            }

            // Verify the effect
            if ((effect.Value < General.Map.FormatInterface.MinEffect) || (effect.Value > General.Map.FormatInterface.MaxEffect))
            {
                General.ShowWarningMessage("Sector effect must be between " + General.Map.FormatInterface.MinEffect + " and " + General.Map.FormatInterface.MaxEffect + ".", MessageBoxButtons.OK);
                return;
            }

            // Make undo
            if (sectors.Count > 1) undodesc = sectors.Count + " sectors";
            General.Map.UndoRedo.CreateUndo("Edit " + undodesc);

            // Go for all sectors
            foreach (Sector s in sectors)
            {
                // villsa - Apply all flags
                if (General.Map.FormatInterface.InDoom64Mode)
                {
                    Lights light = new Lights();

                    // flags
                    foreach (CheckBox c in flags.Checkboxes)
                    {
                        if (c.CheckState == CheckState.Checked) s.SetFlag(c.Tag.ToString(), true);
                        else if (c.CheckState == CheckState.Unchecked) s.SetFlag(c.Tag.ToString(), false);
                    }

                    //
                    // color lights
                    //

                    if (initialcolor[0].ToColor() != ceilingcolor.Color.ToColor())
                    {
                        light.color = ceilingcolor.Color;
                        s.CeilColor = light;
                    }

                    if (initialcolor[1].ToColor() != topcolor.Color.ToColor())
                    {
                        light.color = topcolor.Color;
                        s.TopColor = light;
                    }

                    if (initialcolor[2].ToColor() != thingcolor.Color.ToColor())
                    {
                        light.color = thingcolor.Color;
                        s.ThingColor = light;
                    }

                    if (initialcolor[3].ToColor() != lowercolor.Color.ToColor())
                    {
                        light.color = lowercolor.Color;
                        s.LowerColor = light;
                    }

                    if (initialcolor[4].ToColor() != floorcolor.Color.ToColor())
                    {
                        light.color = floorcolor.Color;
                        s.FloorColor = light;
                    }
                }

                // Effects
                if (!effect.Empty) s.Effect = effect.Value;

                // Floor/Ceiling
                s.FloorHeight = floorheight.GetResult(s.FloorHeight);
                s.CeilHeight = ceilingheight.GetResult(s.CeilHeight);
                s.SetFloorTexture(floortex.GetResult(s.FloorTexture));
                s.SetCeilTexture(ceilingtex.GetResult(s.CeilTexture));

                // Action
                s.Tag = General.Clamp(tag.GetResult(s.Tag), General.Map.FormatInterface.MinTag, General.Map.FormatInterface.MaxTag);

                if (carcosaPanelReady)
                {
                    s.CarcosaWeather = carcosaWeather.SelectedIndex < 0 ? 0 : carcosaWeather.SelectedIndex;
                    s.CarcosaMood = carcosaMood.SelectedIndex < 0 ? 0 : carcosaMood.SelectedIndex;
                    s.CarcosaFogDensity = (float)carcosaDensity.Value;
                    s.CarcosaFogStart = (float)carcosaFogStart.Value;
                    s.CarcosaFogMax = (float)carcosaFogMax.Value;
                    s.CarcosaFogR = carcosaFogColor.BackColor.R;
                    s.CarcosaFogG = carcosaFogColor.BackColor.G;
                    s.CarcosaFogB = carcosaFogColor.BackColor.B;
                    s.CarcosaOutdoors = carcosaOutdoors.Checked;
                    s.CarcosaPlaceName = carcosaPlace.Text;
                }
            }

            // Update the used textures
            General.Map.Data.UpdateUsedTextures();

            // Done
            General.Map.IsChanged = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Cancel clicked
        private void cancel_Click(object sender, EventArgs e)
        {
            // Be gone
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // This finds a new (unused) tag
        private void newtag_Click(object sender, EventArgs e)
        {
            tag.Text = General.Map.Map.GetNewTag().ToString();
        }

        // Browse Effect clicked
        private void browseeffect_Click(object sender, EventArgs e)
        {
            effect.Value = EffectBrowserForm.BrowseEffect(this, effect.Value);
        }

        // Ceiling height changes
        private void ceilingheight_TextChanged(object sender, EventArgs e)
        {
            UpdateSectorHeight();
        }

        // Floor height changes
        private void floorheight_TextChanged(object sender, EventArgs e)
        {
            UpdateSectorHeight();
        }

        // Help
        private void SectorEditForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            General.ShowHelp("w_sectoredit.html");
            hlpevent.Handled = true;
        }

        private void CeilingLightIncrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(ceilingcolor.Color.r, ceilingcolor.Color.g, ceilingcolor.Color.b, 0);
            light.SetIntensity(LIGHTINCVALUE);
            ceilingcolor.Color = light.color;
        }

        private void TopWallLightIncrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(topcolor.Color.r, topcolor.Color.g, topcolor.Color.b, 0);
            light.SetIntensity(LIGHTINCVALUE);
            topcolor.Color = light.color;
        }

        private void ThingLightIncrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(thingcolor.Color.r, thingcolor.Color.g, thingcolor.Color.b, 0);
            light.SetIntensity(LIGHTINCVALUE);
            thingcolor.Color = light.color;
        }

        private void BottomWallLightIncrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(lowercolor.Color.r, lowercolor.Color.g, lowercolor.Color.b, 0);
            light.SetIntensity(LIGHTINCVALUE);
            lowercolor.Color = light.color;
        }

        private void FloorLightIncrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(floorcolor.Color.r, floorcolor.Color.g, floorcolor.Color.b, 0);
            light.SetIntensity(LIGHTINCVALUE);
            floorcolor.Color = light.color;
        }

        private void CeilingLightDecrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(ceilingcolor.Color.r, ceilingcolor.Color.g, ceilingcolor.Color.b, 0);
            light.SetIntensity(LIGHTDECVALUE);
            ceilingcolor.Color = light.color;
        }

        private void TopWallLightDecrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(topcolor.Color.r, topcolor.Color.g, topcolor.Color.b, 0);
            light.SetIntensity(LIGHTDECVALUE);
            topcolor.Color = light.color;
        }

        private void ThingLightDecrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(thingcolor.Color.r, thingcolor.Color.g, thingcolor.Color.b, 0);
            light.SetIntensity(LIGHTDECVALUE);
            thingcolor.Color = light.color;
        }

        private void BottomWallLightDecrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(lowercolor.Color.r, lowercolor.Color.g, lowercolor.Color.b, 0);
            light.SetIntensity(LIGHTDECVALUE);
            lowercolor.Color = light.color;
        }

        private void FloorLightDecrease_Click(object sender, EventArgs e)
        {
            Lights light = new Lights(floorcolor.Color.r, floorcolor.Color.g, floorcolor.Color.b, 0);
            light.SetIntensity(LIGHTDECVALUE);
            floorcolor.Color = light.color;
        }
    }
}
