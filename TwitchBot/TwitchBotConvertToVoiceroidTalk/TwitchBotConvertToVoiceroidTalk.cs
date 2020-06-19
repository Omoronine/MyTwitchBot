using System.Collections.Generic;
using Omoronine.TwitchBot.Bouyomichanhan;
using Omoronine.TwitchIrc;
using TwitchBotDemo;

namespace Omoronine.TwitchBot.Bouyomichan
{
    /// <summary>
    /// TwitchIrcからのテキストを棒読みちゃんのプラグインであるVoiceroidTalkで使用する形式に変換するためのbotです。
    /// このクラスの初期化には、設定ファイルのパスを要求します。
    /// </summary>
    class TwitchBotConvertToVoiceroidTalk
    {

        //botの設定や情報
        private VoiceroidTalkConfig _voiceroidTalkConfig;

        //チャット発言者が設定した情報を保存するための辞書
        private Dictionary<string, string> _userData { get; set; }

        //TwitchIrcClient
        private IrcClient _ircClient;

        //コンストラクタを使用禁止にします
        private TwitchBotConvertToVoiceroidTalk() { }

        /// <summary>
        /// このクラスは直接インスタンスを生成することを禁止している為、オブジェクトを作成するときはこのメソッドを用います。
        /// </summary>
        /// <param name="irc">TwitchIrcクライアントを指定</param>
        /// <param name="filename">設定ファイルのパスを指定</param>
        /// <returns>新しいTwitchBotConvertToVoiceroidTalkのインスタンスを返します。失敗時はnullを返します。</returns>
        public static TwitchBotConvertToVoiceroidTalk Create(in IrcClient irc, in string filename)
        {
            TwitchBotConvertToVoiceroidTalk newObject = new TwitchBotConvertToVoiceroidTalk();
            //Ircクライアントをセット
            newObject._ircClient = irc;

            //引数から設定ファイルを読み込む
            VoiceroidTalkConfigReader vtcr = new VoiceroidTalkConfigReader();
            newObject._voiceroidTalkConfig = vtcr.ReadJsonFromFile(filename);
            if (newObject._voiceroidTalkConfig == null)
            {
                //ファイル読み込みに失敗
                return null;
            }

            //読み込んだ設定ファイルからユーザーデータを読み込む
            VoiceroidTalkUserDataReader vtudr = new VoiceroidTalkUserDataReader();
            newObject._userData = vtudr.ReadJsonFromFile(
                System.AppDomain.CurrentDomain.BaseDirectory + newObject._voiceroidTalkConfig.UserDataFileName
                );

            //ユーザーデータのファイルがなければ辞書を作成
            if (newObject._userData == null)
            {
                newObject._userData = new Dictionary<string, string>();
            }
            return newObject;
        }

        /// <summary>
        /// 引数のチャットデータを元に変換、またはBOTとして動作します。
        /// BOTとして動作する場合、読み上げは不あってはならないで空の文字列が返ります。
        /// </summary>
        /// <param name="twitchIrcChatData"></param>
        /// <returns>
        /// 変換が必要な場合は返還後の文字列。変換が失敗した場合は本文がそのまま返ります。
        /// アクションに文字が含まれる場合は読み上げてはまずいので空の文字列が返ります
        /// </returns>
        public string Run( in TwitchIrcChatData twitchIrcChatData )
        {
            //アクショントリガーがある場合、BOTとして動作する
            if(twitchIrcChatData.Action == "")
            {
                return DoConvert(twitchIrcChatData);
            }
            else
            {
                DoAction(twitchIrcChatData);
            }

            return "";
        }


        /// <summary>
        /// 辞書に登録されているユーザー情報の保存を行います。
        /// </summary>
        public void SaveUserData()
        {
            VoiceroidTalkUserDataWriter vtudw = new VoiceroidTalkUserDataWriter();
            vtudw.WriteJsonFromFile(
                System.AppDomain.CurrentDomain.BaseDirectory + _voiceroidTalkConfig.UserDataFileName, _userData
                );
        }

        /// <summary>
        /// アクションのトリガーが含まれている場合、BOTとしての動作をするための本体となるメソッドです。
        /// </summary>
        /// <param name="twitchIrcChatData">調べるチャットのデータ</param>
        private void DoAction( in TwitchIrcChatData twitchIrcChatData)
        {
            string text = null;
            //コマンドが一致しているか調べbotとしてのアクションが必要かどうかを調べる
            if (twitchIrcChatData.Action.Equals(_voiceroidTalkConfig.Command))
            {
                //引数のキーを元に音声情報を探す
                if (_voiceroidTalkConfig.VoiceList.ContainsKey(twitchIrcChatData.Text)) {
                    //ユーザーが既に存在するかを調べる
                    if (_userData.ContainsKey(twitchIrcChatData.User))
                    {
                        //存在するなら音声情報を上書き
                        _userData[twitchIrcChatData.User.ToString()] = twitchIrcChatData.Text.ToString();
                    }
                    else {
                        //存在しないなら音声情報を新しく登録
                        _userData.Add(
                            twitchIrcChatData.User.ToString(),
                            twitchIrcChatData.Text.ToString()
                        );
                    }
                    //登録成功メッセージをチャットに送信
                    text = _voiceroidTalkConfig.SuccessMessage.ToString();
                    text = text.Replace(_voiceroidTalkConfig.UserMacro, twitchIrcChatData.User);
                    text = text.Replace(_voiceroidTalkConfig.VoiceMacro, twitchIrcChatData.Text);
                    _ircClient.SendPublicChatMessage(text);
                }
                else
                {
                    //ヘルプメッセージを作成
                    text = _voiceroidTalkConfig.HelpMessage.ToString();
                    text += "【 ";
                    foreach(string keys in _voiceroidTalkConfig.VoiceList.Keys)
                    {
                        text += keys + " ";
                    }
                    text += "】";
                    //存在しない場合はヘルプメッセージをチャットに送信
                    _ircClient.SendPublicChatMessage(text);
                }
            }
        }

        /// <summary>
        /// アクショントリガーが含まれていない場合の、コンバーターとして動作する本体となるメソッドです。
        /// </summary>
        /// <param name="twitchIrcChatData">調べるチャットのデータ</param>
        /// <returns>変換された文字列を返します。変換が不可能だった場合、本文がそのまま返ります。</returns>
        private string DoConvert(in TwitchIrcChatData twitchIrcChatData)
        {
            string text = "";
            //発言者が辞書に登録されているかを調べます
            if (_userData.ContainsKey(twitchIrcChatData.User))
            {
                //現在その情報が使用できるかを調べます
                if (_voiceroidTalkConfig.VoiceList.ContainsKey(_userData[twitchIrcChatData.User]))
                {
                    //設定を使用可能なのでそのユーザー専用の設定で変換します。
                    text += _voiceroidTalkConfig.VoiceList[_userData[twitchIrcChatData.User]];
                }
            }

            //設定が使用されなかった場合、デフォルトが使用されます
            if(text == "")
            {
                //デフォルトが使用できるかを調べます
                if (_voiceroidTalkConfig.VoiceList.ContainsKey(_voiceroidTalkConfig.Default))
                {
                    //使用可能なのでデフォルトを使用します。
                    text += _voiceroidTalkConfig.VoiceList[_voiceroidTalkConfig.Default];
                }
            }

            //名前を読み上げるなら名前を追加
            if (_voiceroidTalkConfig.IsCallName == true)
            {
                text += twitchIrcChatData.User.ToString() + " ";
            }
            //本文をセットし返します
            text += twitchIrcChatData.Text.ToString();
            return text;
        }

    }
}