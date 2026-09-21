
#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	/// <summary>
	/// Packs one or more single-map PWADs into a multi-map PWAD (Builder UI stand-in for
	/// Scoundrel :game:mergeCarcosaWad). Preserves each map's sibling lumps; rewrites markers.
	/// </summary>
	internal static class CarcosaMultiMapPacker
	{
		private static readonly HashSet<string> MapSiblingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"THINGS", "LINEDEFS", "SIDEDEFS", "VERTEXES", "SEGS", "SSECTORS", "NODES",
			"SECTORS", "REJECT", "BLOCKMAP", "BEHAVIOR", "SCRIPTS", "LIGHTS", "MACROS",
			"LEAFS", "GL_VERT", "GL_SEGS", "GL_SSECT", "GL_NODES", "GL_PVS",
			"CARCOSA", "CARCLUA", "CARCWLD",
		};

		private static readonly Regex MapMarker = new Regex(@"^MAP\d{2}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>One source map: path to a WAD and the MAP## name to write.</summary>
		public sealed class MapEntry
		{
			public string SourcePath;
			public string MapName;
		}

		/// <summary>
		/// Merge [maps] into [outputPath]. Optional [worldLua] is written as CARCWLD on the first map
		/// (replacing any existing CARCWLD in that map group).
		/// </summary>
		public static void Pack(IList<MapEntry> maps, string outputPath, string worldLua)
		{
			if (maps == null || maps.Count == 0)
				throw new ArgumentException("At least one source map is required.");
			if (string.IsNullOrEmpty(outputPath))
				throw new ArgumentException("Output path is required.");

			string tempPath = outputPath + ".packtmp";
			if (File.Exists(tempPath)) File.Delete(tempPath);

			WAD target = null;
			try
			{
				target = new WAD(tempPath, false);
				for (int i = 0; i < maps.Count; i++)
				{
					MapEntry entry = maps[i];
					if (string.IsNullOrEmpty(entry.SourcePath) || !File.Exists(entry.SourcePath))
						throw new FileNotFoundException("Source map not found: " + entry.SourcePath);
					string marker = NormalizeMapName(entry.MapName, i);
					AppendMap(target, entry.SourcePath, marker, (i == 0) ? worldLua : null);
				}
				target.Flush();
				target.Dispose();
				target = null;

				if (File.Exists(outputPath)) File.Delete(outputPath);
				File.Move(tempPath, outputPath);
			}
			catch
			{
				if (target != null)
				{
					try { target.Dispose(); } catch { }
				}
				if (File.Exists(tempPath))
				{
					try { File.Delete(tempPath); } catch { }
				}
				throw;
			}
		}

		private static string NormalizeMapName(string name, int index)
		{
			string n = (name ?? "").Trim().ToUpperInvariant();
			if (MapMarker.IsMatch(n)) return n;
			return "MAP" + (index + 1).ToString("00");
		}

		private static void AppendMap(WAD target, string sourcePath, string mapName, string worldLua)
		{
			WAD source = null;
			try
			{
				source = new WAD(sourcePath, true);
				int header = FindMapHeaderIndex(source);
				if (header < 0)
					throw new IOException("No MAP## header found in " + sourcePath);

				List<Lump> group = CollectMapGroup(source, header);
				bool wroteCarcwld = false;

				target.Insert(mapName, target.Lumps.Count, 0);

				for (int i = 1; i < group.Count; i++)
				{
					Lump src = group[i];
					string name = src.Name.ToUpperInvariant();
					if (name == "CARCWLD" && worldLua != null)
					{
						WriteTextLump(target, "CARCWLD", worldLua);
						wroteCarcwld = true;
						continue;
					}
					Lump dst = target.Insert(src.Name, target.Lumps.Count, src.Length);
					src.CopyTo(dst);
				}

				if (worldLua != null && !wroteCarcwld)
					WriteTextLump(target, "CARCWLD", worldLua);
			}
			finally
			{
				if (source != null) source.Dispose();
			}
		}

		private static void WriteTextLump(WAD target, string name, string text)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text ?? "");
			Lump lump = target.Insert(name, target.Lumps.Count, bytes.Length);
			if (bytes.Length > 0)
			{
				lump.Stream.Seek(0, SeekOrigin.Begin);
				lump.Stream.Write(bytes, 0, bytes.Length);
			}
		}

		private static int FindMapHeaderIndex(WAD wad)
		{
			for (int i = 0; i < wad.Lumps.Count; i++)
			{
				if (MapMarker.IsMatch(wad.Lumps[i].Name)) return i;
			}
			return -1;
		}

		private static List<Lump> CollectMapGroup(WAD wad, int headerIndex)
		{
			List<Lump> group = new List<Lump>();
			group.Add(wad.Lumps[headerIndex]);
			for (int i = headerIndex + 1; i < wad.Lumps.Count; i++)
			{
				string name = wad.Lumps[i].Name;
				if (MapMarker.IsMatch(name)) break;
				if (!MapSiblingNames.Contains(name)) break;
				group.Add(wad.Lumps[i]);
			}
			return group;
		}
	}
}
