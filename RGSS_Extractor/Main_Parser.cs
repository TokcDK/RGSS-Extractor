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

		public List<Entry> ParseFile(string path)
		{
			BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
			string @string = Encoding.UTF8.GetString(binaryReader.ReadBytes(6));
			if (@string != "RGSSAD")
			{
				return null;
			}
			binaryReader.ReadByte();
			int version = (int)binaryReader.ReadByte();
			this.parser = this.GetParser(version, binaryReader);
			if (this.parser == null)
			{
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

		public void CloseFile()
		{
			this.parser.CloseFile();
		}

        public void Dispose()
        {
            CloseFile();
        }
    }
}
