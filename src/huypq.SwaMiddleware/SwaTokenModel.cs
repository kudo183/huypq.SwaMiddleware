using System;

namespace huypq.SwaMiddleware
{
    public class SwaTokenModel
    {
        /// <summary>
        /// Name of the logged in user
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Id of the logged in user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Token craetion time (int Utc)
        /// </summary>
        public DateTime CreateTime { get; set; }

        public SwaTokenModel()
        {
            CreateTime = DateTime.UtcNow;
        }

        public string ToBase64()
        {
            using (var ms = new System.IO.MemoryStream())
            using (var bw = new System.IO.BinaryWriter(ms))
            {
                bw.Write(UserId);
                bw.Write(User);
                bw.Write(CreateTime.ToBinary());
                bw.Flush();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static SwaTokenModel FromBase64(string str)
        {
            var result = new SwaTokenModel();

            using (var ms = new System.IO.MemoryStream(Convert.FromBase64String(str)))
            using (var br = new System.IO.BinaryReader(ms))
            {
                result.UserId = br.ReadInt32();
                result.User = br.ReadString();
                result.CreateTime = DateTime.FromBinary(br.ReadInt64());

                return result;
            }
        }
    }
}
