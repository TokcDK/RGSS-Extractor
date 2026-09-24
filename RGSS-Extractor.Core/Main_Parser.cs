using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RGSS_Extractor
{
	public class Main_Parser : IDisposable
	{
		private Parser parser;

		private Parser GetParser(int version, BinaryReader inFile)
		{
			if (version == 1)
			{
				return new RGSSAD_Parser(inFile);
			}
			if (version == 3)
			{
				return new RGSS3AParser(inFile);
			}
			return null;
		}

		/// <summary>
		/// Reads the archive's entry table.
		/// </summary>
		/// <remarks>
		/// Returns null - rather than throwing - when the file is not an RGSSAD archive or its version
		/// is unknown. The reader is disposed on those paths: it owns an open FileStream, and leaving
		/// it behind held the archive open for the rest of the process, which on Windows also blocks
		/// writing it back.
		/// </remarks>
		/// <param name="path">The archive to read.</param>
		/// <returns>The entries, or null when the file is not a supported archive.</returns>
		public List<Entry> ParseFile(string path)
		{
			BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
			string @string = Encoding.UTF8.GetString(binaryReader.ReadBytes(6));
			if (@string != "RGSSAD")
			{
				binaryReader.Dispose();
				return null;
			}
			binaryReader.ReadByte();
			int version = (int)binaryReader.ReadByte();
			this.parser = this.GetParser(version, binaryReader);
			if (this.parser == null)
			{
				binaryReader.Dispose();
				return null;
			}
			this.parser.ParseFile();

            return parser.entries;
		}

		public byte[] GetFiledata(Entry e)
		{
			return this.parser.ReadData(e.Offset, e.Size, e.Datakey);
		}

		public void ExportFile(Entry e)
		{
			this.parser.WriteFile(e);
		}

		public void ExportArchive()
		{
			if (this.parser == null)
			{
				return;
			}
			this.parser.write_entries();
		}

		/// <summary>
		/// Closes the archive. Safe to call after a <see cref="ParseFile"/> that returned null.
		/// </summary>
		/// <remarks>
		/// The inner parser is left null on every path that returned null above, and it used to be
		/// dereferenced here - so disposing an archive that was not one threw NullReferenceException
		/// from Dispose instead of reporting "not an archive".
		/// </remarks>
		public void CloseFile()
		{
			if (this.parser == null)
			{
				return;
			}

			this.parser.CloseFile();
			this.parser = null;
		}

        public void Dispose()
        {
            CloseFile();
        }
    }
}
