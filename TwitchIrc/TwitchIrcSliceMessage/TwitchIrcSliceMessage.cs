using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Omoronine
{
    namespace TwitchIrc
    {
        /// <summary>
        /// Twitchのチャット情報は様々なデータを1行のテキストにまとめております。
        /// このクラスは、それらのまとまったテイル情報を扱いやすいように分割するためのクラスです。
        /// </summary>
        class TwitchIrcSliceMessage
        {

            //チャット情報のメモ
            //:user_name!user_name @user_name.tmi.twitch.tv PRIVMSG #broadcaster_name :TEXTTEXTTEXT
            //チャット情報の位置について。今後も変わることはないと思われるため、現在は定数として定義してます。
            private const string USER_START = "@";
            private const string USER_END = ".tmi.twitch.tv";
            private const string BROADCASTER_START = "#";
            private const string BROADCASTER_END = " :";
            private const string TEXT_START = " :";
            private const string ACTION_END = " ";

            //config.jsonファイルに定義されたアクションのトリガー
            private List<string> _actiontTriggers;

            /// <summary>
            /// コンストラクタでは、Bot等のアクションのトリガーとなる任意テキストのリストをセットしてください。
            /// トリガーが不要であればnullを指定してください。
            /// ※トリガーとなるテキストとは...
            ///     例えばTwitchのデフォルトのチャットコマンドは「/action」の形式となっており、この時のトリガーは"/"で始まる文字列から成ります。
            ///     この場合、トリガーとなるテキストは"/"となります。
            ///     他に、twitchのBotの多くは、「!action」の形式を取っており、トリガーは"!"で始まる文字列から成ります。
            ///     この場合、トリガーとなるテキストは"!"となります。
            /// </summary>
            /// <param name="actiontTriggers">Bot等のアクションのトリガーになるテキストを含むリスト</param>
            public TwitchIrcSliceMessage( in List<string> actiontTriggers )
            {
                _actiontTriggers = actiontTriggers;
            }

            /// <summary>
            /// Twitchから受け取ったメッセージを情報毎に分割し、TwitchIrcChatDataクラスにセットし返します。
            /// </summary>
            /// <param name="message">Twitchから受け取ったメッセージをセットしてください。</param>
            /// <returns>分割したチャットの情報をTwitchIrcChatDataクラスにセットし返します。</returns>
            public TwitchIrcChatData SliceMessage( in string message )
            {
                //名前を取得
                string user = message.Substring(
                    message.IndexOf(USER_START) + USER_START.Length , 
                    message.IndexOf(USER_END) - message.IndexOf(USER_START) - USER_START.Length
                    );
                //配信者を取得
                string broadcaster = message.Substring(
                    message.IndexOf(BROADCASTER_START) + BROADCASTER_START.Length,
                    message.IndexOf(BROADCASTER_END) - message.IndexOf(BROADCASTER_START) - BROADCASTER_START.Length
                    );
                //本文を取得
                string text = message.Substring(
                    message.IndexOf(TEXT_START) + TEXT_START.Length ,
                    message.Length - message.IndexOf(TEXT_START) - TEXT_START.Length
                    );
                //アクションを取得
                string action = "";
                if (_actiontTriggers != null)
                {
                    foreach (string actionTrigger in _actiontTriggers.ToArray())
                    {
                        //アクションのトリガーが本文と前方から見て一致した場合
                        if (text.StartsWith(actionTrigger))
                        {
                            //その上でさらに空白がある場合
                            if (text.Contains(" "))
                            {
                                //アクショントリガーの内容を格納
                                action = text.Substring(0, text.IndexOf(ACTION_END));
                                //本文からアクショントリガーを消す
                                text = text.Substring(
                                    text.IndexOf(ACTION_END) + ACTION_END.Length,
                                    text.Length - text.IndexOf(ACTION_END) - ACTION_END.Length
                                    );
                            }
                            else
                            {
                                action = text;
                                text = "";
                            }
                        }
                    }
                }
                //取得した文字列から返すオブジェクトを作成
                return new TwitchIrcChatData( broadcaster , user , action, text );
            }

        }
    }
}
