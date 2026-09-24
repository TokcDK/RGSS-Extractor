using System;
using System.IO;

namespace RGSS_Extractor
{
    public class RGSSAD_Parser : Parser
	{
		public RGSSAD_Parser(BinaryReader file) : base(file)
		{
		}

		/// <remarks>
		/// The length comes out of the archive, so the loop walks what was actually read: ReadBytes
		/// returns fewer bytes than asked for at the end of a truncated file.
		/// </remarks>
		public string read_filename(int len)
		{
			byte[] array = this.inFile.ReadBytes(len);
			for (int i = 0; i < array.Length; i++)
			{
				byte[] expr_18_cp_0 = array;
				int expr_18_cp_1 = i;
				expr_18_cp_0[expr_18_cp_1] ^= (byte)this.magickey;
				this.magickey = this.magickey * 7 + 3;
			}
			return base.GetString(array);
		}

		/// <remarks>
		/// Bounded by "before the end of the stream" rather than "not exactly at the end": the old
		/// condition only stopped on an exact landing, so a table that ended between two reads - or a
		/// length that ran past the end - kept the loop going instead of stopping it.
		/// </remarks>
		public void parse_table()
		{
			while (this.inFile.BaseStream.Position < this.inFile.BaseStream.Length)
			{
				int num = this.inFile.ReadInt32();
				num ^= this.magickey;
				this.magickey = this.magickey * 7 + 3;
				string name = this.read_filename(num);
				long num2 = (long)this.inFile.ReadInt32();
				num2 ^= (long)this.magickey;
				this.magickey = this.magickey * 7 + 3;
				long position = this.inFile.BaseStream.Position;
				this.inFile.BaseStream.Seek(num2, SeekOrigin.Current);
				Entry entry = new Entry();
				entry.Name = name;
				entry.Offset = position;
				entry.Size = num2;
				entry.Datakey = this.magickey;
				this.entries.Add(entry);
			}
		}

		public override void ParseFile()
		{
			uint magickey = 3735931646u;
			this.magickey = (int)magickey;
			this.parse_table();
		}
	}
}
