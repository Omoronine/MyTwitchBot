using System;
using System.Collections.Generic;
using System.Text;

namespace Omoronine.TwitchBot.Bouyomichan
{
    class VoiceroidTalkConfig
    {
        public string Command { get; set; }
        public string UserMacro { get; set; }
        public string VoiceMacro { get; set; }
        public string SuccessMessage { get; set; }
        public string HelpMessage { get; set; }
        public string Default { get; set; }
        public string UserDataFileName { get; set; }
        public bool IsCallName { get; set; }
        public Dictionary<string ,string> VoiceList { get; set; }
    }
}
