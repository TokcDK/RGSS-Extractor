using System;
using System.IO;

namespace RGSS_Extractor
{
    public class RGSS3AParser : Parser
	{
		public RGSS3AParser(BinaryReader file) : base(file)
		{
		}

		public string ReadFilename(int len)
		{
			byte[] array = this.inFile.ReadBytes(len);
			for (int i = 0; i < len; i++)
			{
				byte[] expr_18_cp_0 = array;
				int expr_18_cp_1 = i;
				expr_18_cp_0[expr_18_cp_1] ^= (byte)(this.magickey >> 8 * (i % 4));
			}
			return base.GetString(array);
		}

		public void ParseTable()
		{
			while (true)
			{
				long num = (long)this.inFile.ReadInt32();
				num ^= (long)this.magickey;
				if (num == 0L)
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
