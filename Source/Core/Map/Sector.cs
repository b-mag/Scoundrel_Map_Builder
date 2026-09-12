
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;
using CodeImp.DoomBuilder.Rendering;
using System.Collections.ObjectModel;
using SlimDX.Direct3D9;
using SlimDX;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public sealed class Sector : SelectableElement
    {
        #region ================== Constants

        public const int NUM_COLORS = 5;    // villsa 9/14/11 (builder64)

        #endregion

        #region ================== Variables

        // Map
        private MapSet map;

        // List items
        private LinkedListNode<Sector> selecteditem;

        // Sidedefs
        private LinkedList<Sidedef> sidedefs;

        // Properties
        private int fixedindex;
        private int floorheight;
        private int ceilheight;
        private string floortexname;
        private string ceiltexname;
        private long longfloortexname;
        private long longceiltexname;
        private int effect;
        private int tag;
        private int brightness;
        private Dictionary<string, bool> flags; // villsa
        private Lights ceilColor;   // villsa
        private Lights flrColor;    // villsa
        private Lights thingColor;  // villsa
        private Lights topColor;    // villsa
        private Lights lwrColor;    // villsa
        private uint hashfloortexname;  // villsa
        private uint hashceilingtexname;    // villsa

        // Carcosa sidecar (CARCOSA lump — not part of the 24-byte SECTORS struct)
        private int carcosaWeather;
        private int carcosaMood;
        private float carcosaFogDensity;
        private float carcosaFogStart;
        private float carcosaFogMax;
        private int carcosaFogR = 128;
        private int carcosaFogG = 128;
        private int carcosaFogB = 128;
        private int carcosaAmbient;
        private float carcosaEmissive;
        private bool carcosaOutdoors = true;
        private bool carcosaFacadeOnly;
        private string carcosaPlaceName = "";

        // Cloning
        private Sector clone;
        private int serializedindex;

        // Triangulation
        private bool updateneeded;
        private bool triangulationneeded;
        private RectangleF bbox;
        private Triangulation triangles;
        private FlatVertex[] flatvertices;
        private ReadOnlyCollection<LabelPositionInfo> labels;
        private SurfaceEntryCollection surfaceentries;

        #endregion

        #region ================== Properties

        public MapSet Map { get { return map; } }
        public ICollection<Sidedef> Sidedefs { get { return sidedefs; } }

        /// <summary>
        /// An unique index that does not change when other sectors are removed.
        /// </summary>
        public int FixedIndex { get { return fixedindex; } }
        public int FloorHeight { get { return floorheight; } set { BeforePropsChange(); floorheight = value; } }
        public int CeilHeight { get { return ceilheight; } set { BeforePropsChange(); ceilheight = value; } }
        public string FloorTexture { get { return floortexname; } }
        public string CeilTexture { get { return ceiltexname; } }

        // villsa
        public uint HashFloor { get { return hashfloortexname; } set { hashfloortexname = value; } }
        public uint HashCeiling { get { return hashceilingtexname; } set { hashceilingtexname = value; } }

        public long LongFloorTexture { get { return longfloortexname; } }
        public long LongCeilTexture { get { return longceiltexname; } }
        public int Effect { get { return effect; } set { BeforePropsChange(); effect = value; } }
        public int Tag { get { return tag; } set { BeforePropsChange(); tag = value; if ((tag < General.Map.FormatInterface.MinTag) || (tag > General.Map.FormatInterface.MaxTag)) throw new ArgumentOutOfRangeException("Tag", "Invalid tag number"); } }
        public int Brightness { get { return brightness; } set { BeforePropsChange(); brightness = value; updateneeded = true; } }
        public bool UpdateNeeded { get { return updateneeded; } set { updateneeded |= value; triangulationneeded |= value; } }
        public RectangleF BBox { get { return bbox; } }
        internal Sector Clone { get { return clone; } set { clone = value; } }
        internal int SerializedIndex { get { return serializedindex; } set { serializedindex = value; } }
        public Triangulation Triangles { get { return triangles; } }
        public FlatVertex[] FlatVertices { get { return flatvertices; } }
        public ReadOnlyCollection<LabelPositionInfo> Labels { get { return labels; } }
        internal Dictionary<string, bool> Flags { get { return flags; } } // villsa

        public int CarcosaWeather { get { return carcosaWeather; } set { BeforePropsChange(); carcosaWeather = value; } }
        public int CarcosaMood { get { return carcosaMood; } set { BeforePropsChange(); carcosaMood = value; } }
        public float CarcosaFogDensity { get { return carcosaFogDensity; } set { BeforePropsChange(); carcosaFogDensity = value; } }
        public float CarcosaFogStart { get { return carcosaFogStart; } set { BeforePropsChange(); carcosaFogStart = value; } }
        public float CarcosaFogMax { get { return carcosaFogMax; } set { BeforePropsChange(); carcosaFogMax = value; } }
        public int CarcosaFogR { get { return carcosaFogR; } set { BeforePropsChange(); carcosaFogR = value; } }
        public int CarcosaFogG { get { return carcosaFogG; } set { BeforePropsChange(); carcosaFogG = value; } }
        public int CarcosaFogB { get { return carcosaFogB; } set { BeforePropsChange(); carcosaFogB = value; } }
        public int CarcosaAmbient { get { return carcosaAmbient; } set { BeforePropsChange(); carcosaAmbient = value; } }
        public float CarcosaEmissive { get { return carcosaEmissive; } set { BeforePropsChange(); carcosaEmissive = value; } }
        public bool CarcosaOutdoors { get { return carcosaOutdoors; } set { BeforePropsChange(); carcosaOutdoors = value; } }
        public bool CarcosaFacadeOnly { get { return carcosaFacadeOnly; } set { BeforePropsChange(); carcosaFacadeOnly = value; } }
        public string CarcosaPlaceName { get { return carcosaPlaceName ?? ""; } set { BeforePropsChange(); carcosaPlaceName = value ?? ""; } }

        public bool HasCarcosaExtra()
        {
            return (carcosaWeather != 0) || (carcosaMood != 0) || (carcosaFogDensity != 0f) ||
                (carcosaFogStart != 0f) || (carcosaFogMax != 0f) ||
                (carcosaFogR != 128) || (carcosaFogG != 128) || (carcosaFogB != 128) ||
                (carcosaAmbient != 0) || (carcosaEmissive != 0f) ||
                !carcosaOutdoors || carcosaFacadeOnly ||
                ((carcosaPlaceName != null) && (carcosaPlaceName.Length > 0));
        }

        // Lump load: assign without undo (opening a map must not mark it dirty).
        internal void ApplyCarcosaFromLump(int weather, int mood, float fogDensity, float fogStart, float fogMax,
            int fogR, int fogG, int fogB, int ambient, float emissive, bool outdoors, bool facadeOnly, string placeName)
        {
            carcosaWeather = weather;
            carcosaMood = mood;
            carcosaFogDensity = fogDensity;
            carcosaFogStart = fogStart;
            carcosaFogMax = fogMax;
            carcosaFogR = fogR;
            carcosaFogG = fogG;
            carcosaFogB = fogB;
            carcosaAmbient = ambient;
            carcosaEmissive = emissive;
            carcosaOutdoors = outdoors;
            carcosaFacadeOnly = facadeOnly;
            carcosaPlaceName = placeName ?? "";
        }
        public Lights CeilColor { get { return ceilColor; } set { BeforePropsChange(); ceilColor = value; } } // villsa
        public Lights FloorColor { get { return flrColor; } set { BeforePropsChange(); flrColor = value; } } // villsa
        public Lights ThingColor { get { return thingColor; } set { BeforePropsChange(); thingColor = value; } } // villsa
        public Lights TopColor { get { return topColor; } set { BeforePropsChange(); topColor = value; } } // villsa
        public Lights LowerColor { get { return lwrColor; } set { BeforePropsChange(); lwrColor = value; } } // villsa

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal Sector(MapSet map, int listindex, int index)
        {
            // Initialize
            this.map = map;
            this.listindex = listindex;
            this.sidedefs = new LinkedList<Sidedef>();
            this.fixedindex = index;
            this.floortexname = "-";
            this.ceiltexname = "-";
            this.longfloortexname = MapSet.EmptyLongName;
            this.longceiltexname = MapSet.EmptyLongName;
            this.updateneeded = true;
            this.triangulationneeded = true;
            this.surfaceentries = new SurfaceEntryCollection();
            this.flags = new Dictionary<string, bool>(); // villsa
            this.ceilColor = new Lights(128, 128, 128, 0); // villsa
            this.flrColor = new Lights(128, 128, 128, 0); // villsa
            this.thingColor = new Lights(128, 128, 128, 0); // villsa
            this.topColor = new Lights(128, 128, 128, 0); // villsa
            this.lwrColor = new Lights(128, 128, 128, 0); // villsa
            this.carcosaOutdoors = true;
            this.carcosaFogR = 128;
            this.carcosaFogG = 128;
            this.carcosaFogB = 128;
            this.carcosaPlaceName = "";

            if (map == General.Map.Map)
                General.Map.UndoRedo.RecAddSector(this);

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public override void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                // Already set isdisposed so that changes can be prohibited
                isdisposed = true;

                // Dispose the sidedefs that are attached to this sector
                // because a sidedef cannot exist without reference to its sector.
                if (map.AutoRemove)
                    foreach (Sidedef sd in sidedefs) sd.Dispose();
                else
                    foreach (Sidedef sd in sidedefs) sd.SetSectorP(null);

                if (map == General.Map.Map)
                    General.Map.UndoRedo.RecRemSector(this);

                // Remove from main list
                map.RemoveSector(listindex);

                // Register the index as free
                map.AddSectorIndexHole(fixedindex);

                // Free surface entry
                General.Map.CRenderer2D.Surfaces.FreeSurfaces(surfaceentries);

                // Clean up
                sidedefs = null;
                map = null;

                // Dispose base
                base.Dispose();
            }
        }

        #endregion

        #region ================== Management

        // Call this before changing properties
        protected override void BeforePropsChange()
        {
            if (map == General.Map.Map)
                General.Map.UndoRedo.RecPrpSector(this);
        }

        // Serialize / deserialize (passive: this doesn't record)
        internal void ReadWrite(IReadWriteStream s)
        {
            if (!s.IsWriting)
            {
                BeforePropsChange();
                updateneeded = true;
            }

            base.ReadWrite(s);

            // villsa
            if (s.IsWriting)
            {
                s.wInt(flags.Count);

                foreach (KeyValuePair<string, bool> f in flags)
                {
                    s.wString(f.Key);
                    s.wBool(f.Value);
                }
            }
            else
            {
                int c; s.rInt(out c);

                flags = new Dictionary<string, bool>(c);
                for (int i = 0; i < c; i++)
                {
                    string t; s.rString(out t);
                    bool b; s.rBool(out b);
                    flags.Add(t, b);
                }
            }

            s.rwInt(ref fixedindex);
            s.rwInt(ref floorheight);
            s.rwInt(ref ceilheight);
            s.rwString(ref floortexname);
            s.rwString(ref ceiltexname);
            s.rwLong(ref longfloortexname);
            s.rwLong(ref longceiltexname);
            s.rwInt(ref effect);
            s.rwInt(ref tag);
            s.rwInt(ref brightness);
            // villsa
            if (General.Map.FormatInterface.InDoom64Mode)
            {
                s.rwLight(ref ceilColor);
                s.rwLight(ref flrColor);
                s.rwLight(ref thingColor);
                s.rwLight(ref topColor);
                s.rwLight(ref lwrColor);
            }

            s.rwInt(ref carcosaWeather);
            s.rwInt(ref carcosaMood);
            s.rwFloat(ref carcosaFogDensity);
            s.rwFloat(ref carcosaFogStart);
            s.rwFloat(ref carcosaFogMax);
            s.rwInt(ref carcosaFogR);
            s.rwInt(ref carcosaFogG);
            s.rwInt(ref carcosaFogB);
            s.rwInt(ref carcosaAmbient);
            s.rwFloat(ref carcosaEmissive);
            s.rwBool(ref carcosaOutdoors);
            s.rwBool(ref carcosaFacadeOnly);
            s.rwString(ref carcosaPlaceName);
        }

        // After deserialization
        internal void PostDeserialize(MapSet map)
        {
            triangles.PostDeserialize(map);
            updateneeded = true;
            triangulationneeded = true;
        }

        // This copies all properties to another sector
        public void CopyPropertiesTo(Sector s)
        {
            s.BeforePropsChange();

            // Copy properties
            s.ceilheight = ceilheight;
            s.ceiltexname = ceiltexname;
            s.longceiltexname = longceiltexname;
            s.floorheight = floorheight;
            s.floortexname = floortexname;
            s.longfloortexname = longfloortexname;
            s.effect = effect;
            s.tag = tag;
            s.flags = new Dictionary<string, bool>(flags);  // villsa
            s.brightness = brightness;
            s.updateneeded = true;
            s.ceilColor = ceilColor;    // villsa
            s.flrColor = flrColor;    // villsa
            s.thingColor = thingColor;    // villsa
            s.topColor = topColor;    // villsa
            s.lwrColor = lwrColor;    // villsa
            s.carcosaWeather = carcosaWeather;
            s.carcosaMood = carcosaMood;
            s.carcosaFogDensity = carcosaFogDensity;
            s.carcosaFogStart = carcosaFogStart;
            s.carcosaFogMax = carcosaFogMax;
            s.carcosaFogR = carcosaFogR;
            s.carcosaFogG = carcosaFogG;
            s.carcosaFogB = carcosaFogB;
            s.carcosaAmbient = carcosaAmbient;
            s.carcosaEmissive = carcosaEmissive;
            s.carcosaOutdoors = carcosaOutdoors;
            s.carcosaFacadeOnly = carcosaFacadeOnly;
            s.carcosaPlaceName = carcosaPlaceName;
            base.CopyPropertiesTo(s);
        }

        // This attaches a sidedef and returns the listitem
        internal LinkedListNode<Sidedef> AttachSidedefP(Sidedef sd)
        {
            updateneeded = true;
            triangulationneeded = true;
            return sidedefs.AddLast(sd);
        }

        // This detaches a sidedef
        internal void DetachSidedefP(LinkedListNode<Sidedef> l)
        {
            // Not disposing?
            if (!isdisposed)
            {
                // Remove sidedef
                updateneeded = true;
                triangulationneeded = true;
                sidedefs.Remove(l);

                // No more sidedefs left?
                if ((sidedefs.Count == 0) && map.AutoRemove)
                {
                    // This sector is now useless, dispose it
                    this.Dispose();
                }
            }
        }

        // This triangulates the sector geometry
        internal void Triangulate()
        {
            if (updateneeded)
            {
                // Triangulate again?
                if (triangulationneeded || (triangles == null))
                {
                    // Triangulate sector
                    triangles = Triangulation.Create(this);
                    triangulationneeded = false;
                    updateneeded = true;

                    // Make label positions
                    labels = Array.AsReadOnly<LabelPositionInfo>(Tools.FindLabelPositions(this).ToArray());

                    // Number of vertices changed?
                    if (triangles.Vertices.Count != surfaceentries.totalvertices)
                        General.Map.CRenderer2D.Surfaces.FreeSurfaces(surfaceentries);
                }
            }
        }

        // This makes new vertices as well as floor and ceiling surfaces
        internal void CreateSurfaces()
        {
            if (updateneeded)
            {
                // Brightness color
                int brightint;

                // villsa
                switch (General.Map.Renderer2D.ViewMode)
                {
                    case ViewMode.FloorColor:
                        brightint = this.flrColor.color.ToInt();
                        break;
                    case ViewMode.CeilingColor:
                        brightint = this.ceilColor.color.ToInt();
                        break;
                    case ViewMode.ThingColor:
                        brightint = this.thingColor.color.ToInt();
                        break;
                    case ViewMode.FloorTextures:
                        if (Renderer.FullBrightness)
                        {
                            brightint = General.Map.Renderer2D.CalculateBrightness(brightness);
                        }
                        else
                        {
                            brightint = this.flrColor.color.ToInt();
                        }
                        break;
                    case ViewMode.CeilingTextures:
                        if (Renderer.FullBrightness)
                        {
                            brightint = General.Map.Renderer2D.CalculateBrightness(brightness);
                        }
                        else
                        {
                            brightint = this.ceilColor.color.ToInt();
                        }
                        break;
                    default:
                        brightint = this.flrColor.color.ToInt();
                        break;
                }

                // Make vertices
                flatvertices = new FlatVertex[triangles.Vertices.Count];
                for (int i = 0; i < triangles.Vertices.Count; i++)
                {
                    flatvertices[i].x = triangles.Vertices[i].x;
                    flatvertices[i].y = triangles.Vertices[i].y;
                    flatvertices[i].z = 1.0f;
                    flatvertices[i].c = brightint;
                    flatvertices[i].u = triangles.Vertices[i].x;
                    flatvertices[i].v = triangles.Vertices[i].y;
                }

                // Create bounding box
                bbox = CreateBBox();

                // Make update info (this lets the plugin fill in texture coordinates and such)
                SurfaceUpdate updateinfo = new SurfaceUpdate(flatvertices.Length, true, true);
                flatvertices.CopyTo(updateinfo.floorvertices, 0);
                General.Plugins.OnSectorFloorSurfaceUpdate(this, ref updateinfo.floorvertices);
                flatvertices.CopyTo(updateinfo.ceilvertices, 0);
                General.Plugins.OnSectorCeilingSurfaceUpdate(this, ref updateinfo.ceilvertices);
                updateinfo.floortexture = longfloortexname;
                updateinfo.ceiltexture = longceiltexname;
                
                // Update surfaces
                General.Map.CRenderer2D.Surfaces.UpdateSurfaces(surfaceentries, updateinfo);

                // Updated
                updateneeded = false;
            }
        }

        // This updates the floor surface
        public void UpdateFloorSurface()
        {
            if (flatvertices == null) return;

            // Create floor vertices
            SurfaceUpdate updateinfo = new SurfaceUpdate(flatvertices.Length, true, false);
            flatvertices.CopyTo(updateinfo.floorvertices, 0);
            General.Plugins.OnSectorFloorSurfaceUpdate(this, ref updateinfo.floorvertices);
            updateinfo.floortexture = longfloortexname;

            // Update entry
            General.Map.CRenderer2D.Surfaces.UpdateSurfaces(surfaceentries, updateinfo);
            General.Map.CRenderer2D.Surfaces.UnlockBuffers();
        }

        // This updates the ceiling surface
        public void UpdateCeilingSurface()
        {
            if (flatvertices == null) return;

            // Create ceiling vertices
            SurfaceUpdate updateinfo = new SurfaceUpdate(flatvertices.Length, false, true);
            flatvertices.CopyTo(updateinfo.ceilvertices, 0);
            General.Plugins.OnSectorCeilingSurfaceUpdate(this, ref updateinfo.ceilvertices);
            updateinfo.ceiltexture = longceiltexname;

            // Update entry
            General.Map.CRenderer2D.Surfaces.UpdateSurfaces(surfaceentries, updateinfo);
            General.Map.CRenderer2D.Surfaces.UnlockBuffers();
        }

        // This updates the sector when changes have been made
        public void UpdateCache()
        {
            // Update if needed
            if (updateneeded)
            {
                Triangulate();

                CreateSurfaces();

                General.Map.CRenderer2D.Surfaces.UnlockBuffers();
            }
        }

        // Selected
        protected override void DoSelect()
        {
            base.DoSelect();
            selecteditem = map.SelectedSectors.AddLast(this);
        }

        // Deselect
        protected override void DoUnselect()
        {
            base.DoUnselect();
            if (selecteditem.List != null) selecteditem.List.Remove(selecteditem);
            selecteditem = null;
        }

        #endregion

        #region ================== Methods

        // This checks if the given point is inside the sector polygon
        public bool Intersect(Vector2D p)
        {
            uint c = 0;

            // Go for all sidedefs
            foreach (Sidedef sd in sidedefs)
            {
                // Get vertices
                Vector2D v1 = sd.Line.Start.Position;
                Vector2D v2 = sd.Line.End.Position;

                // Determine min/max values
                float miny = Math.Min(v1.y, v2.y);
                float maxy = Math.Max(v1.y, v2.y);
                float maxx = Math.Max(v1.x, v2.x);

                // Check for intersection
                if ((p.y > miny) && (p.y <= maxy))
                {
                    if (p.x <= maxx)
                    {
                        if (v1.y != v2.y)
                        {
                            float xint = (p.y - v1.y) * (v2.x - v1.x) / (v2.y - v1.y) + v1.x;
                            if ((v1.x == v2.x) || (p.x <= xint)) c++;
                        }
                    }
                }
            }

            // Inside this polygon?
            return ((c & 0x00000001UL) != 0);
        }

        // This creates a bounding box rectangle
        // This requires the sector triangulation to be up-to-date!
        private RectangleF CreateBBox()
        {
            // Setup
            float left = float.MaxValue;
            float top = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MinValue;

            // Go for vertices
            foreach (Vector2D v in triangles.Vertices)
            {
                // Update rect
                if (v.x < left) left = v.x;
                if (v.y < top) top = v.y;
                if (v.x > right) right = v.x;
                if (v.y > bottom) bottom = v.y;
            }

            // Return rectangle
            return new RectangleF(left, top, right - left, bottom - top);
        }

        // This joins the sector with another sector
        // This sector will be disposed
        public void Join(Sector other)
        {
            // Any sidedefs to move?
            if (sidedefs.Count > 0)
            {
                // Change secter reference on my sidedefs
                // This automatically disposes this sector
                while (sidedefs != null)
                    sidedefs.First.Value.SetSector(other);
            }
            else
            {
                // No sidedefs attached
                // Dispose manually
                this.Dispose();
            }

            General.Map.IsChanged = true;
        }

        // String representation
        public override string ToString()
        {
            return "Sector " + listindex;
        }

        #endregion

        #region ================== Changes

        // This updates all properties
        public void Update(int hfloor, int hceil, string tfloor, string tceil, int effect, int tag, int brightness)
        {
            BeforePropsChange();

            // Apply changes
            this.floorheight = hfloor;
            this.ceilheight = hceil;
            SetFloorTexture(tfloor);
            SetCeilTexture(tceil);
            this.effect = effect;
            this.tag = tag;
            this.brightness = brightness;
            updateneeded = true;
        }

        // villsa - new overload method
        private Lights GetLight(int cindex, Lights[] light)
        {
            Lights color;

            if (cindex >= 256)
            {
                cindex -= 256;
                color = light[cindex];
            }
            else
            {
                byte c = (byte)cindex;

                color = new Lights(c, c, c, 0);
            }

            color.color.a = 255;

            return color;
        }

        private Lights GetLight(int color)
        {
            PixelColor c;

            c = PixelColor.FromInt(color);
            return new Lights(c.r, c.g, c.b, 0);
        }

        // villsa TODO - too many fucking overloads for this. Need to simplify the way lighting is handled...
        public void Update(Dictionary<string, bool> flags, int hfloor, int hceil,
            string tfloor, string tceil, int effect, int tag, Lights[] light, int[] cindex)
        {
            BeforePropsChange();

            // Apply changes
            this.flags = new Dictionary<string, bool>(flags);
            this.floorheight = hfloor;
            this.ceilheight = hceil;
            SetFloorTexture(tfloor);
            SetCeilTexture(tceil);
            this.effect = effect;
            this.tag = tag;
            this.ceilColor = GetLight(cindex[1], light);
            this.flrColor = GetLight(cindex[0], light);
            this.thingColor = GetLight(cindex[2], light);
            this.topColor = GetLight(cindex[3], light);
            this.lwrColor = GetLight(cindex[4], light);
            this.brightness = 255;
            updateneeded = true;
        }

        public void Update(Dictionary<string, bool> flags, int hfloor, int hceil,
            string tfloor, string tceil, int effect, int tag, int[] colors)
        {
            BeforePropsChange();

            // Apply changes
            this.flags = new Dictionary<string, bool>(flags);
            this.floorheight = hfloor;
            this.ceilheight = hceil;
            SetFloorTexture(tfloor);
            SetCeilTexture(tceil);
            this.effect = effect;
            this.tag = tag;
            this.brightness = 255;
            this.flrColor = GetLight(colors[0]);
            this.ceilColor = GetLight(colors[1]);
            this.thingColor = GetLight(colors[2]);
            this.topColor = GetLight(colors[3]);
            this.lwrColor = GetLight(colors[4]);
            updateneeded = true;
        }

        // [villsa start]
        // This checks and returns a flag without creating it
        public bool IsFlagSet(string flagname)
        {
            if (flags.ContainsKey(flagname))
                return flags[flagname];
            else
                return false;
        }

        // This sets a flag
        public void SetFlag(string flagname, bool value)
        {
            if (!flags.ContainsKey(flagname) || (IsFlagSet(flagname) != value))
            {
                BeforePropsChange();
                flags[flagname] = value;
            }
        }

        // This returns a copy of the flags dictionary
        public Dictionary<string, bool> GetFlags()
        {
            return new Dictionary<string, bool>(flags);
        }

        // This clears all flags
        public void ClearFlags()
        {
            flags.Clear();
        }

        // [villsa end]

        // This sets texture
        public void SetFloorTexture(string name)
        {
            BeforePropsChange();

            floortexname = name;
            longfloortexname = Lump.MakeLongName(name);
            updateneeded = true;
            General.Map.IsChanged = true;
        }

        // This sets texture
        public void SetCeilTexture(string name)
        {
            BeforePropsChange();

            ceiltexname = name;
            longceiltexname = Lump.MakeLongName(name);
            updateneeded = true;
            General.Map.IsChanged = true;
        }

        #endregion
    }
}
