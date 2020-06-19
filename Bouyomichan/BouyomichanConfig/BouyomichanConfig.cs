namespace Omoronine.Bouyomichan
{
    
    /// <summary>
    /// 棒読みちゃんの設定をセットするクラスです。
    /// このクラスは棒読みちゃんに与えられるそれぞれの設定に対応したプロパティを持ちます。
    /// 値の詳細については棒読みちゃんのドキュメントをご覧になってください。
    /// </summary>
    public class BouyomichanConfig
    {
        public string Host{ get; set; }
        public int? Port { get; set; }
        public int? ByteCode { get; set; }
        public int? Voice { get; set; }
        public int? Volume { get; set; }
        public int? Speed { get; set; }
        public int? Tone { get; set; }

        public BouyomichanConfig()
        {
            Host = null;
            Port = null;
            ByteCode = null;
            Voice = null;
            Volume = null;
            Speed = null;
            Tone = null;
        }

        public BouyomichanConfig( 
            string i_host , int i_port , int i_byteCode , int i_voice , int i_volume , int i_speed , int i_tone)
        {
            Host = i_host;
            Port = i_port;
            ByteCode = i_byteCode;
            Voice = i_voice;
            Volume = i_volume;
            Speed = i_speed;
            Tone = i_tone;
        }

        public BouyomichanConfig(BouyomichanConfig i_copy)
        {
            Host = i_copy.Host;
            Port = i_copy.Port;
            ByteCode = i_copy.ByteCode;
            Voice = i_copy.Voice;
            Volume = i_copy.Volume;
            Speed = i_copy.Speed;
            Tone = i_copy.Tone;
        }

    }
}
