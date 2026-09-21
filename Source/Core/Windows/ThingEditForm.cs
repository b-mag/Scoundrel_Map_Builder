
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
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Controls;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
    /// <summary>
    /// Dialog window that allows viewing and editing of Thing properties.
    /// </summary>
    public partial class ThingEditForm : DelayedForm
    {
        #region ================== Variables

        private ICollection<Thing> things;
        private List<TreeNode> nodes;
        private ThingTypeInfo thinginfo;

        private NumericUpDown carcosaArchetype;
        private NumericUpDown carcosaState;
        private NumericUpDown carcosaFlags;
        private NumericUpDown carcosaDialogue;
        private NumericUpDown carcosaQuest;
        private NumericUpDown carcosaPx0;
        private NumericUpDown carcosaPy0;
        private NumericUpDown carcosaPx1;
        private NumericUpDown carcosaPy1;
        private Button carcosaEditLua;
        private bool carcosaPanelReady;

        #endregion

        #region ================== Properties

        #endregion

        #region ================== Constructor

        // Constructor
        public ThingEditForm()
        {
            // Initialize
            InitializeComponent();

            // Fill flags list
            foreach (KeyValuePair<string, string> tf in General.Map.Config.ThingFlags)
                flags.Add(tf.Value, tf.Key);

            // villsa - hide thing tag if specified false
            if (!General.Map.FormatInterface.HasThingTag)
                groupBox3.Hide();

            // Thing height?
            height.Visible = General.Map.FormatInterface.HasThingHeight;
            heightlabel.Visible = General.Map.FormatInterface.HasThingHeight;

            // Setup types list
            thingtype.Setup();

            if (General.Map.FormatInterface.InDoom64Mode)
                BuildCarcosaPanel();
        }

        private static NumericUpDown MakeNud(int x, int y, int max, int decimals)
        {
            NumericUpDown n = new NumericUpDown();
            n.Location = new Point(x, y);
            n.Size = new Size(70, 22);
            n.Minimum = decimals > 0 ? -32000 : 0;
            n.Maximum = max;
            n.DecimalPlaces = decimals;
            n.Increment = decimals > 0 ? 1 : 1;
            return n;
        }

        private void BuildCarcosaPanel()
        {
            foreach (Control c in thingproperties.Controls)
            {
                GroupBox gb = c as GroupBox;
                if (gb != null && gb.Left < 20 && gb.Text != null && gb.Text.IndexOf("Thing") >= 0)
                {
                    gb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    break;
                }
            }

            int bottom = 0;
            foreach (Control c in thingproperties.Controls)
                if (c.Bottom > bottom) bottom = c.Bottom;

            GroupBox box = new GroupBox();
            box.Text = " Script / RPG ";
            box.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            box.Location = new Point(6, bottom + 8);
            box.Size = new Size(thingproperties.Width - 18, 228);
            box.TabIndex = 30;

            Label la = new Label();
            la.Text = "Archetype:";
            la.Location = new Point(12, 28);
            la.AutoSize = true;
            carcosaArchetype = MakeNud(80, 24, 65535, 0);

            Label ls = new Label();
            ls.Text = "State:";
            ls.Location = new Point(160, 28);
            ls.AutoSize = true;
            carcosaState = MakeNud(200, 24, 255, 0);

            Label lf = new Label();
            lf.Text = "Flags:";
            lf.Location = new Point(280, 28);
            lf.AutoSize = true;
            carcosaFlags = MakeNud(320, 24, 255, 0);

            Label ld = new Label();
            ld.Text = "Dialogue:";
            ld.Location = new Point(12, 60);
            ld.AutoSize = true;
            carcosaDialogue = MakeNud(80, 56, 65535, 0);

            Label lq = new Label();
            lq.Text = "Quest flag:";
            lq.Location = new Point(160, 60);
            lq.AutoSize = true;
            carcosaQuest = MakeNud(230, 56, 65535, 0);

            Label lp = new Label();
            lp.Text = "Patrol x0 y0 x1 y1:";
            lp.Location = new Point(12, 96);
            lp.AutoSize = true;
            carcosaPx0 = MakeNud(130, 92, 32000, 1);
            carcosaPy0 = MakeNud(206, 92, 32000, 1);
            carcosaPx1 = MakeNud(282, 92, 32000, 1);
            carcosaPy1 = MakeNud(358, 92, 32000, 1);

            carcosaEditLua = new Button();
            carcosaEditLua.Text = "Edit Lua for this thing";
            carcosaEditLua.Location = new Point(12, 128);
            carcosaEditLua.Size = new Size(180, 26);
            carcosaEditLua.Click += carcosaEditLua_Click;

            Label hint = new Label();
            hint.Text = "Runtime script id is enkai / fountain_N / key_thingIndex — match thing_type in the map CARCLUA lump.\r\n" +
                "Flag-gated doors: set a linedef Tag, then carcosa.open_tag(n). Writes CARCOSA thing extras.";
            hint.Location = new Point(12, 162);
            hint.Size = new Size(620, 52);
            hint.ForeColor = Color.DimGray;

            box.Controls.Add(la);
            box.Controls.Add(carcosaArchetype);
            box.Controls.Add(ls);
            box.Controls.Add(carcosaState);
            box.Controls.Add(lf);
            box.Controls.Add(carcosaFlags);
            box.Controls.Add(ld);
            box.Controls.Add(carcosaDialogue);
            box.Controls.Add(lq);
            box.Controls.Add(carcosaQuest);
            box.Controls.Add(lp);
            box.Controls.Add(carcosaPx0);
            box.Controls.Add(carcosaPy0);
            box.Controls.Add(carcosaPx1);
            box.Controls.Add(carcosaPy1);
            box.Controls.Add(carcosaEditLua);
            box.Controls.Add(hint);

            thingproperties.Controls.Add(box);
            thingproperties.Height = box.Bottom + 10;
            this.ClientSize = new Size(this.ClientSize.Width, thingproperties.Bottom + 56);
            carcosaPanelReady = true;
        }

        private void carcosaEditLua_Click(object sender, EventArgs e)
        {
            if (things == null || things.Count == 0) return;
            Thing ft = General.GetByIndex(things, 0);
            int type = thingtype.GetResult(ft.Type);
            General.Map.EditCarcosaLuaForThing(type);
        }

        // This sets up the form to edit the given things
        public void Setup(ICollection<Thing> things)
        {
            Thing ft;

            // Keep this list
            this.things = things;
            if (things.Count > 1) this.Text = "Edit Things (" + things.Count + ")";

            ////////////////////////////////////////////////////////////////////////
            // Set all options to the first thing properties
            ////////////////////////////////////////////////////////////////////////

            ft = General.GetByIndex(things, 0);

            // Set type
            thingtype.SelectType(ft.Type);

            // Flags
            foreach (CheckBox c in flags.Checkboxes)
                if (ft.Flags.ContainsKey(c.Tag.ToString())) c.Checked = ft.Flags[c.Tag.ToString()];

            // Coordination
            angle.Text = ft.AngleDoom.ToString();
            height.Text = ((int)ft.Position.z).ToString();

            // Tag
            tag.Text = ft.Tag.ToString();

            if (carcosaPanelReady)
            {
                carcosaArchetype.Value = ClampNud(carcosaArchetype, ft.CarcosaArchetypeId);
                carcosaState.Value = ClampNud(carcosaState, ft.CarcosaInitialState);
                carcosaFlags.Value = ClampNud(carcosaFlags, ft.CarcosaFlags);
                carcosaDialogue.Value = ClampNud(carcosaDialogue, ft.CarcosaDialogue);
                carcosaQuest.Value = ClampNud(carcosaQuest, ft.CarcosaQuestFlag);
                carcosaPx0.Value = ClampNud(carcosaPx0, ft.CarcosaPatrolX0);
                carcosaPy0.Value = ClampNud(carcosaPy0, ft.CarcosaPatrolY0);
                carcosaPx1.Value = ClampNud(carcosaPx1, ft.CarcosaPatrolX1);
                carcosaPy1.Value = ClampNud(carcosaPy1, ft.CarcosaPatrolY1);
            }

            ////////////////////////////////////////////////////////////////////////
            // Now go for all lines and change the options when a setting is different
            ////////////////////////////////////////////////////////////////////////

            // Go for all things
            foreach (Thing t in things)
            {
                // Type does not match?
                if ((thingtype.GetSelectedInfo() != null) &&
                   (thingtype.GetSelectedInfo().Index != t.Type))
                    thingtype.ClearSelectedType();

                // Flags
                foreach (CheckBox c in flags.Checkboxes)
                {
                    if (t.Flags.ContainsKey(c.Tag.ToString()))
                    {
                        if (t.Flags[c.Tag.ToString()] != c.Checked)
                        {
                            c.ThreeState = true;
                            c.CheckState = CheckState.Indeterminate;
                        }
                    }
                }

                // Coordination
                if (t.AngleDoom.ToString() != angle.Text) angle.Text = "";
                if (((int)t.Position.z).ToString() != height.Text) height.Text = "";

                // Tag
                if (t.Tag.ToString() != tag.Text) tag.Text = "";
            }
        }

        #endregion

        #region ================== Interface

        // This finds a new (unused) tag
        private void newtag_Click(object sender, EventArgs e)
        {
            tag.Text = General.Map.Map.GetNewTag().ToString();
        }

        // Selected type changes
        private void thingtype_OnTypeChanged(ThingTypeInfo value)
        {
            thinginfo = value;

            // Update preview image
            if (thinginfo != null)
            {
                if (thinginfo.Title == "Camera") // villsa 9/11/11
                {
                    General.DisplayZoomedImage(spritetex, General.Map.Data.ThingCamera.GetBitmap());
                }
                else if (thinginfo.Title == "Trigger") // villsa 9/11/11
                {
                    General.DisplayZoomedImage(spritetex, General.Map.Data.ThingTrigger.GetBitmap());
                }
                else if (thinginfo.Sprite.ToLowerInvariant().StartsWith(DataManager.INTERNAL_PREFIX) &&
                   (thinginfo.Sprite.Length > DataManager.INTERNAL_PREFIX.Length))
                {
                    General.DisplayZoomedImage(spritetex, General.Map.Data.GetSpriteImage(thinginfo.Sprite).GetBitmap());
                }
                else if ((thinginfo.Sprite.Length <= 8) && (thinginfo.Sprite.Length > 0))
                {
                    General.DisplayZoomedImage(spritetex, General.Map.Data.GetSpriteImage(thinginfo.Sprite).GetPreview());
                }
                else
                {
                    spritetex.BackgroundImage = null;
                }
            }
            else
            {
                spritetex.BackgroundImage = null;
            }
        }

        // Angle text changes
        private void angle_TextChanged(object sender, EventArgs e)
        {
            anglecontrol.Value = angle.GetResult(int.MinValue);
        }

        // Angle control clicked
        private void anglecontrol_ButtonClicked(object sender, EventArgs e)
        {
            angle.Text = anglecontrol.Value.ToString();
        }

        // Apply clicked
        private void apply_Click(object sender, EventArgs e)
        {
            List<string> defaultflags = new List<string>();
            string undodesc = "thing";

            // Verify the tag
            if (General.Map.FormatInterface.HasThingTag && ((tag.GetResult(0) < General.Map.FormatInterface.MinTag) || (tag.GetResult(0) > General.Map.FormatInterface.MaxTag)))
            {
                General.ShowWarningMessage("Thing tag must be between " + General.Map.FormatInterface.MinTag + " and " + General.Map.FormatInterface.MaxTag + ".", MessageBoxButtons.OK);
                return;
            }

            // Verify the type
            if (((thingtype.GetResult(0) < General.Map.FormatInterface.MinThingType) || (thingtype.GetResult(0) > General.Map.FormatInterface.MaxThingType)))
            {
                General.ShowWarningMessage("Thing type must be between " + General.Map.FormatInterface.MinThingType + " and " + General.Map.FormatInterface.MaxThingType + ".", MessageBoxButtons.OK);
                return;
            }

            // Make undo
            if (things.Count > 1) undodesc = things.Count + " things";
            General.Map.UndoRedo.CreateUndo("Edit " + undodesc);

            // Go for all the things
            foreach (Thing t in things)
            {
                // Thing type index
                t.Type = General.Clamp(thingtype.GetResult(t.Type), General.Map.FormatInterface.MinThingType, General.Map.FormatInterface.MaxThingType);

                // Coordination
                t.Rotate(angle.GetResult(t.AngleDoom));
                t.Move(t.Position.x, t.Position.y, (float)height.GetResult((int)t.Position.z));

                // Apply all flags
                foreach (CheckBox c in flags.Checkboxes)
                {
                    if (c.CheckState == CheckState.Checked) t.SetFlag(c.Tag.ToString(), true);
                    else if (c.CheckState == CheckState.Unchecked) t.SetFlag(c.Tag.ToString(), false);
                }

                // Tag
                t.Tag = tag.GetResult(t.Tag);

                if (carcosaPanelReady)
                {
                    t.CarcosaArchetypeId = (int)carcosaArchetype.Value;
                    t.CarcosaInitialState = (int)carcosaState.Value;
                    t.CarcosaFlags = (int)carcosaFlags.Value;
                    t.CarcosaDialogue = (int)carcosaDialogue.Value;
                    t.CarcosaQuestFlag = (int)carcosaQuest.Value;
                    t.CarcosaPatrolX0 = (float)carcosaPx0.Value;
                    t.CarcosaPatrolY0 = (float)carcosaPy0.Value;
                    t.CarcosaPatrolX1 = (float)carcosaPx1.Value;
                    t.CarcosaPatrolY1 = (float)carcosaPy1.Value;
                }

                // Update settings
                t.UpdateConfiguration();
            }

            // Set as defaults
            foreach (CheckBox c in flags.Checkboxes)
                if (c.CheckState == CheckState.Checked) defaultflags.Add(c.Tag.ToString());
            General.Settings.DefaultThingType = thingtype.GetResult(General.Settings.DefaultThingType);
            General.Settings.DefaultThingAngle = Angle2D.DegToRad((float)angle.GetResult((int)Angle2D.RadToDeg(General.Settings.DefaultThingAngle) - 90) + 90);
            General.Settings.SetDefaultThingFlags(defaultflags);

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

        // Help
        private void ThingEditForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            General.ShowHelp("w_thingeditor.html");
            hlpevent.Handled = true;
        }

        private static decimal ClampNud(NumericUpDown n, int value)
        {
            decimal v = value;
            if (v < n.Minimum) v = n.Minimum;
            if (v > n.Maximum) v = n.Maximum;
            return v;
        }

        private static decimal ClampNud(NumericUpDown n, float value)
        {
            decimal v = (decimal)value;
            if (v < n.Minimum) v = n.Minimum;
            if (v > n.Maximum) v = n.Maximum;
            return v;
        }

        #endregion
    }
}
