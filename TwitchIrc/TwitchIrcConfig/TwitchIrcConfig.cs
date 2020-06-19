using System.Collections.Generic;

namespace Omoronine
{
    namespace TwitchIrc
    {
        /// <summary>
        /// TwitchIrc接続の為の設定クラスです。
        /// </summary>
        public class TwitchIrcConfig
        {
            // Network settings
            public _NetworkConfig NetworkConfig { get; set; }

            // Bot settings
            public _BotConfig BotConfig { get; set; }

            // Action trigger
            public List<string> ActionTriggers { get; set; }

            public class _NetworkConfig
            {
                public string Server { get; set; }
                public int Port { get; set; }
            }

            public class _BotConfig
            {
                public string BotName { get; set; }
                public string BroadcasterName { get; set; }
                public string Oauth { get; set; }
            }
        }
    }
}
