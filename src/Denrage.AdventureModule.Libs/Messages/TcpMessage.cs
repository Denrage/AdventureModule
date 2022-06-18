namespace Denrage.AdventureModule.Libs.Messages
{
    public class TcpMessage
    {
        public string TypeIdentifier { get; set; }

        public byte[] Data { get; set; }
    }
}