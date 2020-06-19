using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace Omoronine.BouyomichanConnecter
{
    /// <summary>
    /// 棒読みちゃんに送るリクエスト中で、戻り値が必要なリクエストの返信を待つクラス。
    /// インスタンスが生成された時点で、Taskクラスによる別スレッドで指定したコマンドを送信し、戻り値が返ってくるのを待ちます。
    /// このクラスはBouyomichanSenderクラスによって生成されることを想定しているため、直接インスタンスを生成し使用することを考慮してません。
    /// </summary>
    public class BouyomichanReader
    {
        //使用できるコマンドの種類です。
        public enum Command {

            GetPause = 0x0110,      //一時停止状態かどうかを取得
            GetNowPlaying = 0x0120, //現在再生状態かどうかを取得
            GetTaskCount = 0x0130,  //現時点の残りのタスクを取得
        }

        //返答がなかった場合待機する回数(10回で約2秒)
        private readonly int MAX_WAIT_COUNT = 10;

        private Int16 _command;

        private string _host;
        private int _port;

        //スレッド関連
        private Task<int> _task;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _cancelToken;


        /// <summary>
        /// コンストラクタです。
        /// 変数を初期し、コマンドを送信するタスクを作成します。
        /// </summary>
        /// <param name="command"> 送信するコマンド </param>
        /// <param name="host"> 接続先のホスト </param>
        /// <param name="port"> 接続先のポート番号 </param>
        public BouyomichanReader(in Int16 command , in string host , in int port)
        {
            _command = command;
            _host = host;
            _port = port;

            //キャンセルトークンを作成
            _tokenSource = new CancellationTokenSource();
            _cancelToken = _tokenSource.Token;
            //タスクを開始
            _task = Task.Run(() => Run(_cancelToken));
        }

        /// <summary>
        /// コンストラクタで指定したコマンドを送信し、返信を待つタスクです。
        /// 返信があった場合、タイムアウトした場合、例外が発生した場合にこのタスクを完了します。
        /// </summary>
        /// <param name="i_cancelToken"> このタスクのキャンセルトークンです。 </param>
        /// <returns> 
        /// タスクの状態を得るためのTask<int>クラス
        /// 中身のint型変数はタスク成功時、返信で取得した値が格納されます。
        /// 返信がなかった場合と、エラーにより失敗した場合はint型変数に-1が返ります。
        /// </returns>
        private async Task<int> Run(CancellationToken i_cancelToken)
        {
            int waitTime = 1;
            int waitCount = 0;
            byte[] getData = new byte[1];
            List<byte> bytestream = new List<byte>();

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
                using BinaryReader binaryReader = new BinaryReader(streamSync);

                //リクエストを送信
                binaryWriter.Write((Int16)_command); 

                while (true)
                {
                    //途中終了が呼ばれたらタスクを破壊
                    if (i_cancelToken.IsCancellationRequested) break;

                    while (binaryReader.Read(getData, 0, getData.Length) > 0)
                    {
                        //データをリストに追加していく
                        foreach (byte data in getData)
                        {
                            bytestream.Add(data);
                        }
                    }
                    
                    //データの返信があったらループを破壊
                    if( bytestream.Count > 0)
                    {
                        break;
                    }

                    //返答がなかった場合待機
                    await Task.Delay(waitTime).ConfigureAwait(false); 
                    //返答がなかった場合、待機時間を倍にします
                    waitTime *= 2;//1ms,2ms,4ms,8ms,16ms,32ms,64ms,128ms,,,
                    if(waitCount <= MAX_WAIT_COUNT)
                    {
                        waitCount++;
                    }
                    else
                    {
                        //返答一定回数なかったのでTCPをクローズし-1を返す
                        tcpClient.Close();
                        Console.WriteLine("指定時間以内に棒読みちゃんから応答がありませんでした。接続を終了します。");
                        return -1;
                    }

                }

                //バイトストリームによっては配列の反転が必要（棒読みちゃんの場合は必要）
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytestream.ToArray());
                }

                switch (bytestream.Count)
                {
                    case 1:
                        //1バイトならそのままintにキャストして返す
                        return (int)bytestream[0];
                    case 2:
                        //2バイト以上なら配列を結合してintに変換
                        return (int)BitConverter.ToInt16(bytestream.ToArray(), 0);
                    case 4:
                        return BitConverter.ToInt32(bytestream.ToArray(), 0);
                }
                //終了したらタスクを閉じる
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            //最期まで値を返せなければ-1を返す
            return -1;
        }
        /// <summary>
        /// このインスタンスのタスクが完了しているかどうかを返します。
        /// </summary>
        /// <returns>タスクが完了しているならtrue。そうでないならfalse。</returns>
        public bool IsCompleted()
        {
            return _task.IsCompleted;
        }

        /// <summary>
        /// このインスタンスが使用中のタスクを強制終了させます。
        /// </summary>
        public void End()
        {
            _tokenSource.Cancel();
        }

        /// <summary>
        /// このインスタンスがリクエストしたコマンドを返します。
        /// </summary>
        /// <returns> リクエストしたコマンド </returns>
        public Command GetCommand()
        {
            return (Command)_command;
        }

        /// <summary>
        /// リクエストに対する戻り値を返します。
        /// </summary>
        /// <returns> リクエストに対する返信 </returns>
        public int GetResult()
        {
            return _task.Result;
        }
    }
}
