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

		/// <summary>
		/// Opens the output file for an exported entry.
		/// </summary>
		/// <remarks>
		/// File.Create, not File.OpenWrite: OpenWrite keeps whatever is already in the file, so
		/// exporting a shorter version of an entry over an existing one left the old tail in place and
		/// produced a file that was the new data followed by the end of the old.
		/// </remarks>
		public void create_file(string path)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path2 = Path.Combine(directoryName, Path.GetDirectoryName(path));
			string path3 = Path.Combine(directoryName, path);
			Directory.CreateDirectory(path2);
			this.outFile = new BinaryWriter(File.Create(path3));
		}

		/// <summary>
		/// Reads and decrypts one entry's data.
		/// </summary>
		/// <remarks>
		/// The bytes past the last whole four-byte block use the key as it stands after the block
		/// loop, shifted eight bits per byte <b>within the tail</b>. This used the absolute byte index
		/// instead, which happened to give the right answer only because a C# int shift is masked to
		/// five bits and the block start is always a multiple of four. Written out here so the rule is
		/// visible rather than an accident of the masking.
		/// </remarks>
		/// <param name="offset">Where the entry's data starts.</param>
		/// <param name="size">How many bytes it holds.</param>
		/// <param name="datakey">The entry's key.</param>
		/// <returns>The decrypted bytes.</returns>
		public byte[] ReadData(long offset, long size, int datakey)
		{
			if (size <= 0 || size > int.MaxValue)
			{
				return new byte[0];
			}

			this.inFile.BaseStream.Seek(offset, SeekOrigin.Begin);
			this.data = this.inFile.ReadBytes((int)size);

			// A truncated archive reads short instead of throwing, and the loops below walk the
			// length they were promised rather than the length they got.
			int length = this.data.Length;
			int num = length / 4;
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
			while (num2 < length)
			{
				byte[] expr_82_cp_0 = this.data;
				int expr_82_cp_1 = num2;
				expr_82_cp_0[expr_82_cp_1] ^= (byte)(datakey >> 8 * (num2 - i * 4));
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
			if (this.inFile == null)
			{
				return;
			}

			this.inFile.Close();
		}

		public abstract void ParseFile();

        public void Dispose()
        {
			CloseFile();
        }
    }
}
