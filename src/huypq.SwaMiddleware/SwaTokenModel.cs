using System;

namespace huypq.SwaMiddleware
{
    public class SwaTokenModel
    {
        public string User { get; set; }
        public DateTime CreateTime { get; set; }

        public SwaTokenModel()
        {
            CreateTime = DateTime.UtcNow;
        }

        public string ToBase64()
        {
            var ms = new System.IO.MemoryStream();
            var bw = new System.IO.BinaryWriter(ms);
            bw.Write(User);
            bw.Write(CreateTime.ToBinary());
            bw.Flush();
            return Convert.ToBase64String(ms.ToArray());
        }

        public static SwaTokenModel FromBase64(string str)
        {
            var result = new SwaTokenModel();

            var ms = new System.IO.MemoryStream(Convert.FromBase64String(str));
            var br = new System.IO.BinaryReader(ms);
            result.User = br.ReadString();
            result.CreateTime = DateTime.FromBinary(br.ReadInt64());

            return result;
        }
    }
}
