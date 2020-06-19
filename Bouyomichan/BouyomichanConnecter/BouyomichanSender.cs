using System;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace Omoronine.BouyomichanConnecter
{
    /// <summary>
    /// 起動している棒読みちゃんと接続し、棒読みちゃん側で設定された各種のリクエストを送信するクラスです。
    /// 戻り値が必要なリクエストに関しても送信可能ですが、対象のメソッドはBouyomichanReaderクラスを返すので必要であればそちらも参照してください。
    /// </summary>
    class BouyomichanSender
    {
        //コマンドのリスト
        private enum Command
        {
            Talk = 0x0001,          //読み上げ
            Pause = 0x0010,         //一時停止
            Resume = 0x0020,        //一時停止解除
            Skip = 0x0030,          //現在の行をスキップ
            Clear = 0x0040,         //全てのタスクをキャンセル
            GetPause = 0x0110,      //一時停止状態かどうかを取得
            GetNowPlaying = 0x0120, //現在再生状態かどうかを取得
            GetTaskCount = 0x0130,  //現時点の残りのタスクを取得
        }

        private readonly string _host;
        private readonly int _port;

        private byte _byteCode;
        private Int16 _voice;
        private Int16 _volume;
        private Int16 _speed;
        private Int16 _tone;

        /// <summary>
        /// コンストラクタです。
        /// 初期化に関しては、コンストラクタ内での初期化のみの実装となります。
        /// </summary>
        /// <param name="host"> 棒読みちゃんの接続先ホスト </param>
        /// <param name="port"> 接続するポート番号 </param>
        /// <param name="byteCode"> 送信する文字列のバイトコード番号 </param>
        /// <param name="voice"> 使用するボイス番号 </param>
        /// <param name="volume"> 読み上げの音量 </param>
        /// <param name="speed"> 読み上げの速度 </param>
        /// <param name="tone"> 読み上げの音程 </param>
        public BouyomichanSender( in string host , in int port, in int byteCode , in int voice , in int volume , in int speed , in int tone)
        {
            _host = host;
            _port = port;
            _byteCode = (byte)byteCode;
            _voice = (Int16)voice;
            _volume = (Int16)volume;
            _speed = (Int16)speed;
            _tone = (Int16)tone;

        }

        /// <summary>
        /// 棒読みちゃんに読み上げをさせるリクエストを送信します。
        /// </summary>
        /// <param name="text"> 読み上げ内容の文字列 </param>
        public void Talk(in string text)
        {
            Talk(text, _byteCode, _voice , _volume , _speed , _tone);
        }

        /// <summary>
        /// 棒読みちゃんに読み上げさせるリクエストを送信します。
        /// こちらはコンストラクタでの設定を使わず、細かく読み上げの指定を行えます。
        /// </summary>
        /// <param name="text"> 読み上げ内容の文字列 </param>
        /// <param name="byteCode"> 送信する文字列のバイトコード番号 </param>
        /// <param name="voice"> 使用するボイス番号 </param>
        /// <param name="volume"> 読み上げの音量 </param>
        /// <param name="speed"> 読み上げの速度 </param>
        /// <param name="tone"> 読み上げの音程 </param>
        public void Talk(in string text , in int byteCode , in int voice , in int volume , in int speed , in int tone  )
        {
            //読み上げが不要の場合即終了
            if (text == null) return;
            if (text == "") return;

            try
            {

                //TCPクライアントを接続
                using TcpClient tcpClient = new TcpClient(_host, _port);
                //ネットワークアクセス用のデータストリームを作成
                using NetworkStream ns = tcpClient.GetStream();
                //データストリームをスレッドセーフにラッピング
                using Stream streamSync = Stream.Synchronized(ns);
                //バイナリのライターとリーダーを作成
                using BinaryWriter binaryWriter = new BinaryWriter(streamSync);

                byte[] message = Encoding.UTF8.GetBytes(text);
                Int32 length = message.Length;

                binaryWriter.Write((Int16)Command.Talk);    //コマンド（ 0:メッセージ読み上げ）
                binaryWriter.Write((Int16)speed);           //速度    （-1:棒読みちゃん画面上の設定）
                binaryWriter.Write((Int16)tone);            //音程    （-1:棒読みちゃん画面上の設定）
                binaryWriter.Write((Int16)volume);          //音量    （-1:棒読みちゃん画面上の設定）
                binaryWriter.Write((Int16)voice);           //声質    （ 0:棒読みちゃん画面上の設定、1:女性1、2:女性2、3:男性1、4:男性2、5:中性、6:ロボット、7:機械1、8:機械2、10001～:SAPI5）
                binaryWriter.Write((byte)byteCode);         //文字列のbyte配列の文字コード(0:UTF-8, 1:Unicode, 2:Shift-JIS)
                binaryWriter.Write(length);                 //文字列のbyte配列の長さ
                binaryWriter.Write(message);                //文字列のbyte配列

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Talkメソッド以外の、コマンド以外の引数も戻り値のないコマンドを送信するメソッドです。
        /// </summary>
        /// <param name="command"> 送信するコマンド </param>
        private void SendSimpleCommand(in Int16 command )
        {
            try
            {                
                //TCPクライアントを接続
                using TcpClient tcpClient = new TcpClient(_host, _port);
                //ネットワークアクセス用のデータストリームを作成
                using NetworkStream ns = tcpClient.GetStream();
                //データストリームをスレッドセーフにラッピング
                using Stream streamSync = Stream.Synchronized(ns);
                //バイナリのライターとリーダーを作成
                using BinaryWriter binaryWriter = new BinaryWriter(streamSync);

                binaryWriter.Write(command);

                tcpClient.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 戻り値必要なコマンドを送信します。
        /// BouyomichanReaderを生成し、そのクラスにコマンドの送信、及び、戻り値を取得させます。
        /// </summary>
        /// <param name="command">送信するコマンド</param>
        /// <returns> 別タスクで走る送信したリクエストの返答を待つBouyomichanReaderクラス。 </returns>
        private BouyomichanReader SendRefCommand(in Int16 command)
        {
            return new BouyomichanReader(command, _host, _port);
        }

        /// <summary>
        /// 棒読みちゃんを読み上げの一時停止するリクエストを送信します。
        /// </summary>
        public void Pause()
        {
            SendSimpleCommand((Int16)Command.Pause);
        }
        
        /// <summary>
        /// 棒読みちゃんの読み上げの一時停止を解除するリクエストを送信します。
        /// </summary>
        public void Resume()
        {
            SendSimpleCommand((Int16)Command.Resume);
        }

        /// <summary>
        /// 棒読みちゃんの現在の読み上げを中断し、次の読み上げに移動するリクエストを送信します。
        /// </summary>
        public void Skip()
        {
            SendSimpleCommand((Int16)Command.Skip);
        }

        /// <summary>
        /// 棒読みちゃんが現在持っている読み上げタスクをすべて削除するリクエストを送信します。
        /// </summary>
        public void Clear()
        {
            SendSimpleCommand((Int16)Command.Clear);
        }

        /// <summary>
        /// 棒読みちゃんが一時停止中かどうかを取得するリクエストを送信します。
        /// </summary>
        /// <returns> 別タスクで走る送信したリクエストの返答を待つBouyomichanReaderクラス。</returns>
        public BouyomichanReader GetPause()
        {
            return SendRefCommand((Int16)Command.GetPause);
        }

        /// <summary>
        /// 棒読みちゃんの残りのタスクを取得するリクエストを送信します。
        /// </summary>
        /// <returns> 別タスクで走る送信したリクエストの返答を待つBouyomichanReaderクラス。 </returns>
        public BouyomichanReader GetTaskCount()
        {
            return SendRefCommand((Int16)Command.GetTaskCount);
        }

        /// <summary>
        /// 棒読みちゃんが読み上げ中かどうかを取得するリクエストを送信します。
        /// </summary>
        /// <returns> 別タスクで走る送信したリクエストの返答を待つBouyomichanReaderクラス。</returns>
        public BouyomichanReader GetNowPlaying()
        {
            return SendRefCommand((Int16)Command.GetNowPlaying);
        }

    }
}