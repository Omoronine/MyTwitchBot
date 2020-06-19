using System;
using TwitchBotDemo;
using Omoronine.TwitchIrc;
using Omoronine.Bouyomichan;
using Omoronine.BouyomichanConnecter;
using Omoronine.TwitchBot.Bouyomichan;
using System.Runtime.InteropServices;

namespace Omoronine.MyTwitchBot
{
    // ハンドラ・ルーチンに渡される定数の定義
    public enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    class MytwitchBot
    {
        //各種ファイルの保存場所
        private readonly string TWITCH_IRC_CONFIG_FILE = AppDomain.CurrentDomain.BaseDirectory + @"\\config\twitch_irc_config.json";
        private readonly string BOUYOMICHAN_CONFIG_FILE = AppDomain.CurrentDomain.BaseDirectory + @"\\config\bouyomichan_config.json";
        private readonly string VOICEROID_TALK_CONFIG_FILE = AppDomain.CurrentDomain.BaseDirectory + @"\\config\\voiceroid_talk_config.json";

        //自身のインスタンス
        private static MytwitchBot _mytwitchBot = new MytwitchBot();

        //TwitchIrc関連
        private IrcClient _irc;
        private TwitchIrcPingSender _ping;

        //チャットのメッセージをを発言者、アクショントリガー、発言内容に分割するクラス
        private TwitchIrcSliceMessage _twitchIrcSliceMessage;
        
        //分割したテキストから棒読みちゃんに送る文字列を生成するBOT
        private TwitchBotConvertToVoiceroidTalk _twitchBotConvertToVoiceroidTalk;

        //棒読みちゃんに接続するためのオブジェクト
        private BouyomichanSender _bouyomichanWriter;

        // Win32 APIであるSetConsoleCtrlHandler関数の宣言
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // SetConsoleCtrlHandler関数にメソッド（ハンドラ・ルーチン）を渡すためのデリゲート
        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        static void Main(string[] args)
        {
            //終了時の処理を登録するための処理
            HandlerRoutine myHandlerDele = new HandlerRoutine(_mytwitchBot.MyHandler);
            SetConsoleCtrlHandler(myHandlerDele, true);

            if (!_mytwitchBot.Init()) return;
            _mytwitchBot.MainLoop();
        }

        private bool Init()
        {
            //TwitchIrcのコンフィグをファイルから読み込む
            TwitchIrcConfigReader ticr = new TwitchIrcConfigReader();
            TwitchIrcConfig twitchIrcConfig = ticr.ReadJsonFromFile(TWITCH_IRC_CONFIG_FILE);
            if (twitchIrcConfig == null)
            {
                Console.WriteLine("ファイル読み込みに失敗しました。");
                return false;
            }

            //指定したブロードキャストに接続
            _irc = new IrcClient(
                twitchIrcConfig.NetworkConfig.Server.ToString(),
                (int)twitchIrcConfig.NetworkConfig.Port,
                twitchIrcConfig.BotConfig.BotName.ToString(),
                twitchIrcConfig.BotConfig.Oauth.ToString(),
                twitchIrcConfig.BotConfig.BroadcasterName.ToString()
            );

            //チャット内容分割用のクラスを作成
            _twitchIrcSliceMessage = new TwitchIrcSliceMessage(twitchIrcConfig.ActionTriggers);

            //ログイン維持用のタスクを作成し開始
            _ping = new TwitchIrcPingSender(_irc);
            _ping.Start();

            //棒読みちゃん接続設定をファイルから読み込む
            BouyomichanConfigReader brc = new BouyomichanConfigReader();
            BouyomichanConfig bouyomichanConfig = brc.ReadJsonFromFile(BOUYOMICHAN_CONFIG_FILE);
            //読み込みに失敗した
            if (bouyomichanConfig == null)
            {
                Console.WriteLine("ファイル読み込みに失敗しました。");
                return false;
            }
            //棒読みちゃんに接続する設定をセット
            _bouyomichanWriter = new BouyomichanSender(
                bouyomichanConfig.Host,
                (int)bouyomichanConfig.Port,
                (int)bouyomichanConfig.ByteCode,
                (int)bouyomichanConfig.Voice,
                (int)bouyomichanConfig.Volume,
                (int)bouyomichanConfig.Speed,
                (int)bouyomichanConfig.Tone
            );

            //**************** ここからBotの設定 *******************

            //ボイスロイド読み上げbotの初期化をファイルから行う
            _twitchBotConvertToVoiceroidTalk = TwitchBotConvertToVoiceroidTalk.Create(_irc, VOICEROID_TALK_CONFIG_FILE);
            //設定の読み込みに失敗したら失敗
            if (_twitchBotConvertToVoiceroidTalk == null)
            {
                Console.WriteLine("ファイル読み込みに失敗しました。");
                return false;
            }

            //******************************************************

            //成功
            return true;
        }

        private void MainLoop()
        {
            string message;
            TwitchIrcChatData ticd;
            string bouyomiText;
            while (true)
            {
                //TwitchIrcからメッセージを受け取ります
                message = _irc.ReadMessage();
                //受け取ったメッセージがチャット文かどうかを調べる
                if (message.Contains("PRIVMSG"))
                {
                    //チャット文なら文章を分割します
                    ticd = _twitchIrcSliceMessage.SliceMessage(message);
                    //整形したチャットをコンソールに出力
                    Console.WriteLine("[user : " + ticd.User + " ][action : " + ticd.Action + " ]\r\n-> " + ticd.Text);

                    //************** Botにチャットデータを送信 ****************

                    //チャット内容から、棒読みちゃんに送る文字列の設定や生成をします
                    bouyomiText = _twitchBotConvertToVoiceroidTalk.Run(ticd);

                    //*********************** ここまで ************************

                    try
                    {
                        //棒読みちゃんに読み上げさせる文字列を送信します
                        _bouyomichanWriter.Talk(bouyomiText);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine(message);
                }
            }
        }

        // 終了時の処理を行うためのハンドラルーチン
        private bool MyHandler(CtrlTypes ctrlType)
        {
            _mytwitchBot.End();
            Console.WriteLine("アプリケーションを終了します。");
            return false;
        }

        private void End()
        {
            //ユーザー情報をセーブ
            _twitchBotConvertToVoiceroidTalk.SaveUserData();
            //ログイン維持を中断
            _ping.End();
        }

        ~MytwitchBot()
        {
            _mytwitchBot.End();
        }
    }
}
