using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RGSS_Extractor
{
    public abstract class Parser : IDisposable
	{
		protected BinaryReader inFile;

		protected BinaryWriter outFile;

		protected int magickey;

		public List<Entry> entries = new List<Entry>();

		protected byte[] data;

		public Parser(BinaryReader file)
		{
			this.inFile = file;
		}

		public string GetString(byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}

		public void create_file(string path)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path2 = Path.Combine(directoryName, Path.GetDirectoryName(path));
			string path3 = Path.Combine(directoryName, path);
			Directory.CreateDirectory(path2);
			this.outFile = new BinaryWriter(File.OpenWrite(path3));
		}

		public byte[] ReadData(long offset, long size, int datakey)
		{
			this.inFile.BaseStream.Seek(offset, SeekOrigin.Begin);
			this.data = this.inFile.ReadBytes((int)size);
			int num = (int)size / 4;
			int i;
			for (i = 0; i < num; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					byte[] expr_43_cp_0 = this.data;
					int expr_43_cp_1 = i * 4 + j;
					expr_43_cp_0[expr_43_cp_1] ^= (byte)(datakey >> 8 * j);
				}
				datakey = datakey * 7 + 3;
			}
			int num2 = i * 4;
			while ((long)num2 < size)
			{
				byte[] expr_82_cp_0 = this.data;
				int expr_82_cp_1 = num2;
				expr_82_cp_0[expr_82_cp_1] ^= (byte)(datakey >> 8 * num2);
				num2++;
			}
			return this.data;
		}

		public void WriteFile(Entry e)
		{
			this.create_file(e.Name);
			this.data = this.ReadData(e.Offset, e.Size, e.Datakey);
			this.outFile.Write(this.data);
			this.outFile.Close();
			Console.WriteLine("{0} wrote out successfully", e.Name);
		}

		public void write_entries()
		{
			for (int i = 0; i < this.entries.Count; i++)
			{
				this.WriteFile(this.entries[i]);
			}
		}

		public void CloseFile()
		{
			this.inFile.Close();
		}

		public abstract void ParseFile();

        public void Dispose()
        {
			CloseFile();
        }
    }
}
