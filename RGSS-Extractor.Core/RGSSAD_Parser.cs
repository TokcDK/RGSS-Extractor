using System;
using System.IO;

namespace RGSS_Extractor.Core
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
			byte[] array = inFile.ReadBytes(len);
			for (int i = 0; i < array.Length; i++)
			{
				byte[] expr_18_cp_0 = array;
				int expr_18_cp_1 = i;
				expr_18_cp_0[expr_18_cp_1] ^= (byte)magickey;
                magickey = magickey * 7 + 3;
			}
			return GetString(array);
		}

		/// <remarks>
		/// Bounded by "before the end of the stream" rather than "not exactly at the end": the old
		/// condition only stopped on an exact landing, so a table that ended between two reads - or a
		/// length that ran past the end - kept the loop going instead of stopping it.
		/// </remarks>
		public void parse_table()
		{
			while (inFile.BaseStream.Position < inFile.BaseStream.Length)
			{
				int num = inFile.ReadInt32();
				num ^= magickey;
                magickey = magickey * 7 + 3;
				string name = read_filename(num);
				long num2 = inFile.ReadInt32();
				num2 ^= magickey;
                magickey = magickey * 7 + 3;
				long position = inFile.BaseStream.Position;
                inFile.BaseStream.Seek(num2, SeekOrigin.Current);
				Entry entry = new Entry();
				entry.Name = name;
				entry.Offset = position;
				entry.Size = num2;
				entry.Datakey = magickey;
                entries.Add(entry);
			}
		}

		public override void ParseFile()
		{
			uint magickey = 3735931646u;
			this.magickey = (int)magickey;
            parse_table();
		}
	}
}
