#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeImp.DoomBuilder;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.IO
{
    /// <summary>
    /// Reads and writes the additive CARCOSA sidecar lump (see scoundrel docs/carcosa/CARCOSA_LUMP.md).
    /// Does not touch the 24-byte SECTORS struct.
    /// </summary>
    internal static class CarcosaLumpIO
    {
        public const string LUMP_NAME = "CARCOSA";
        public const int VERSION = 1;
        public const int HEADER_SIZE = 16;
        public const int SECTOR_EXTRA_SIZE = 48;
        public const int THING_EXTRA_SIZE = 32;

        public static void ReadFrom(WAD wad, string mapheader, MapSet map, IDictionary maplumps)
        {
            int headerindex = wad.FindLumpIndex(mapheader);
            if (headerindex < 0) return;
            int lumpindex = MapManager.FindSpecificLump(wad, LUMP_NAME, headerindex, mapheader, maplumps);
            if (lumpindex < 0) return;

            Lump lump = wad.Lumps[lumpindex];
            if (lump.Length < HEADER_SIZE) return;

            lump.Stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(lump.Stream, Encoding.ASCII);

            byte[] magic = reader.ReadBytes(8);
            string mag = Encoding.ASCII.GetString(magic).TrimEnd('\0');
            if (mag != "CARCOSA") return;

            int version = reader.ReadUInt16();
            int sectorCount = reader.ReadUInt16();
            int thingCount = reader.ReadUInt16();
            reader.ReadUInt16(); // flags reserved
            if (version < 1) return;

            List<Sector> sectors = new List<Sector>(map.Sectors);
            for (int i = 0; i < sectorCount; i++)
            {
                if (lump.Stream.Position + SECTOR_EXTRA_SIZE > lump.Length) break;
                int sectorIndex = reader.ReadUInt16();
                int weather = reader.ReadByte();
                int mood = reader.ReadByte();
                float fogDensity = reader.ReadSingle();
                float fogStart = reader.ReadSingle();
                float fogMax = reader.ReadSingle();
                int fogR = reader.ReadByte();
                int fogG = reader.ReadByte();
                int fogB = reader.ReadByte();
                int ambient = reader.ReadByte();
                float emissive = reader.ReadSingle();
                bool outdoors = reader.ReadByte() != 0;
                bool facadeOnly = reader.ReadByte() != 0;
                byte[] nameBytes = reader.ReadBytes(22);
                string placeName = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

                if (sectorIndex < 0 || sectorIndex >= sectors.Count) continue;
                sectors[sectorIndex].ApplyCarcosaFromLump(weather, mood, fogDensity, fogStart, fogMax,
                    fogR, fogG, fogB, ambient, emissive, outdoors, facadeOnly, placeName);
            }

            List<Thing> things = new List<Thing>(map.Things);
            for (int i = 0; i < thingCount; i++)
            {
                if (lump.Stream.Position + THING_EXTRA_SIZE > lump.Length) break;
                int thingIndex = reader.ReadUInt16();
                int archetype = reader.ReadUInt16();
                int initialState = reader.ReadByte();
                int flags = reader.ReadByte();
                int dialogue = reader.ReadUInt16();
                float px0 = reader.ReadSingle();
                float py0 = reader.ReadSingle();
                float px1 = reader.ReadSingle();
                float py1 = reader.ReadSingle();
                int quest = reader.ReadUInt16();
                reader.ReadBytes(6);

                if (thingIndex < 0 || thingIndex >= things.Count) continue;
                things[thingIndex].ApplyCarcosaFromLump(archetype, initialState, flags, dialogue,
                    px0, py0, px1, py1, quest);
            }
        }

        public static void WriteTo(WAD wad, string mapheader, MapSet map, IDictionary maplumps)
        {
            int headerindex = wad.FindLumpIndex(mapheader);
            if (headerindex < 0) headerindex = 0;

            List<Sector> sectors = new List<Sector>(map.Sectors);
            List<Thing> things = new List<Thing>(map.Things);
            List<int> sectorIdx = new List<int>();
            List<int> thingIdx = new List<int>();
            for (int i = 0; i < sectors.Count; i++)
            {
                if (sectors[i].HasCarcosaExtra()) sectorIdx.Add(i);
            }
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i].HasCarcosaExtra()) thingIdx.Add(i);
            }

            int insertpos = MapManager.RemoveSpecificLump(wad, LUMP_NAME, headerindex, mapheader, maplumps);
            if (sectorIdx.Count == 0 && thingIdx.Count == 0) return;

            if (insertpos < 0)
            {
                insertpos = headerindex + 1;
                int macros = MapManager.FindSpecificLump(wad, "MACROS", headerindex, mapheader, maplumps);
                if (macros >= 0) insertpos = macros + 1;
            }
            if (insertpos > wad.Lumps.Count) insertpos = wad.Lumps.Count;

            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem, Encoding.ASCII);

            byte[] magic = new byte[8];
            byte[] magstr = Encoding.ASCII.GetBytes("CARCOSA");
            Array.Copy(magstr, magic, Math.Min(7, magstr.Length));
            writer.Write(magic);
            writer.Write((ushort)VERSION);
            writer.Write((ushort)sectorIdx.Count);
            writer.Write((ushort)thingIdx.Count);
            writer.Write((ushort)0);

            foreach (int i in sectorIdx)
            {
                Sector s = sectors[i];
                writer.Write((ushort)i);
                writer.Write((byte)s.CarcosaWeather);
                writer.Write((byte)s.CarcosaMood);
                writer.Write(s.CarcosaFogDensity);
                writer.Write(s.CarcosaFogStart);
                writer.Write(s.CarcosaFogMax);
                writer.Write((byte)s.CarcosaFogR);
                writer.Write((byte)s.CarcosaFogG);
                writer.Write((byte)s.CarcosaFogB);
                writer.Write((byte)s.CarcosaAmbient);
                writer.Write(s.CarcosaEmissive);
                writer.Write((byte)(s.CarcosaOutdoors ? 1 : 0));
                writer.Write((byte)(s.CarcosaFacadeOnly ? 1 : 0));
                byte[] name = new byte[22];
                string place = s.CarcosaPlaceName ?? "";
                byte[] pbytes = Encoding.ASCII.GetBytes(place);
                Array.Copy(pbytes, name, Math.Min(21, pbytes.Length));
                writer.Write(name);
            }

            foreach (int i in thingIdx)
            {
                Thing t = things[i];
                writer.Write((ushort)i);
                writer.Write((ushort)t.CarcosaArchetypeId);
                writer.Write((byte)t.CarcosaInitialState);
                writer.Write((byte)t.CarcosaFlags);
                writer.Write((ushort)t.CarcosaDialogue);
                writer.Write(t.CarcosaPatrolX0);
                writer.Write(t.CarcosaPatrolY0);
                writer.Write(t.CarcosaPatrolX1);
                writer.Write(t.CarcosaPatrolY1);
                writer.Write((ushort)t.CarcosaQuestFlag);
                writer.Write(new byte[6]);
            }

            writer.Flush();
            Lump lump = wad.Insert(LUMP_NAME, insertpos, (int)mem.Length);
            lump.Stream.Seek(0, SeekOrigin.Begin);
            mem.WriteTo(lump.Stream);
        }
    }
}
