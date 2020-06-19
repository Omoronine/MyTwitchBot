using System;
using System.Collections.Generic;
using System.Text;

namespace Omoronine 
{
    namespace TwitchIrc
    {
        /// <summary>
        /// Twitchの発言は様々な情報が1行の文章にまとめて表現されております。
        /// このクラスはそれらの情報を分割したものを保存するクラスです。
        /// このクラスはTwitchIrcSliceMesaageクラスにより生成され、使用者に渡されます。
        /// </summary>
        public class TwitchIrcChatData
        {
            /// <summary>
            /// 配信者のアカウント名
            /// </summary>
            public string Broadcaster { get; }

            /// <summary>
            /// 発言者のアカウント名
            /// </summary>
            public string User { get; }

            /// <summary>
            /// Botやチャットのコマンド
            /// </summary>
            public string Action { get; }

            /// <summary>
            /// チャットの発言の本文
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// このクラスはコンストラクタでのみ値をセット可能です。
            /// TwitchIrcSliceMessageクラスから生成されることのみを想定しております。
            /// </summary>
            /// <param name="broadcaster">配信者のアカウント名</param>
            /// <param name="user">発言者のアカウント名</param>
            /// <param name="action">Botを呼び出すのコマンド</param>
            /// <param name="text">チャットの本文</param>
            public TwitchIrcChatData( in string broadcaster , in string user, in  string action, in string text)
            {
                Broadcaster = broadcaster;
                User = user;
                Action = action;
                Text = text;
            }
        }
    }
}
