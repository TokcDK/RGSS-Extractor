using System;
using System.IO;

namespace RGSS_Extractor
{
    public class RGSS3AParser : Parser
	{
		public RGSS3AParser(BinaryReader file) : base(file)
		{
		}

		/// <summary>
		/// Reads a name of <paramref name="len"/> bytes and decrypts it with the archive's key.
		/// </summary>
		/// <remarks>
		/// The length comes out of the archive, so it is checked. A corrupt or truncated table can name
		/// a length far past the end of the file, and ReadBytes then returns fewer bytes than were
		/// asked for - which the loop used to index straight past the end of.
		/// </remarks>
		public string ReadFilename(int len)
		{
			if (len <= 0)
			{
				return string.Empty;
			}

			byte[] array = this.inFile.ReadBytes(len);
			for (int i = 0; i < array.Length; i++)
			{
				byte[] expr_18_cp_0 = array;
				int expr_18_cp_1 = i;
				expr_18_cp_0[expr_18_cp_1] ^= (byte)(this.magickey >> 8 * (i % 4));
			}
			return base.GetString(array);
		}

		/// <summary>
		/// Reads the entry table. Stops at the zero offset that terminates it.
		/// </summary>
		/// <remarks>
		/// The loop also stops at the end of the stream. It used to rely on the terminator alone, so an
		/// archive whose table was cut short - or whose terminator was missing - read past the end
		/// instead of stopping, and every entry after the damage was built from whatever followed.
		/// </remarks>
		public void ParseTable()
		{
			Stream stream = this.inFile.BaseStream;
			while (stream.Position + 4 <= stream.Length)
			{
				long num = (long)this.inFile.ReadInt32();
				num ^= (long)this.magickey;
				if (num == 0L)
				{
					break;
				}
				if (stream.Position + 12 > stream.Length)
				{
					break;
				}
				long num2 = (long)this.inFile.ReadInt32();
				int num3 = this.inFile.ReadInt32();
				int num4 = this.inFile.ReadInt32();
				num2 ^= (long)this.magickey;
				num3 ^= this.magickey;
				num4 ^= this.magickey;
				string name = this.ReadFilename(num4);
                Entry entry = new Entry
                {
                    Offset = num,
                    Name = name,
                    Size = num2,
                    Datakey = num3
                };
                this.entries.Add(entry);
			}
		}

		public override void ParseFile()
		{
			this.magickey = this.inFile.ReadInt32() * 9 + 3;
			this.ParseTable();
		}
	}
}
