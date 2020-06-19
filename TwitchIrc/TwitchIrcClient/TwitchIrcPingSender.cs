using System.Threading;
using System.Threading.Tasks;
using TwitchBotDemo;

namespace Omoronine
{
    namespace TwitchIrc
    {
        /// <summary>
        /// TwitchIRCでは定期的に接続を確認します。
        /// 一定時間接続への確認に応答がない場合切断されてしまうため6分毎に接続していることを伝えるためのクラスです。
        /// </summary>
        class TwitchIrcPingSender
        {
            private IrcClient _irc;

            //マルチタスク関連
            private Task _task;
            private CancellationTokenSource _tokenSource;
            private CancellationToken _cancelToken;
            private bool _isRun;

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="irc">接続が可能なIrcClientクラスを指定してください。</param>
            public TwitchIrcPingSender(in IrcClient irc)
            {
                _irc = irc;
                _isRun = false;
            }

            /// <summary>
            /// タスクを開始し、5分毎にTwitchに接続を伝えます。
            /// </summary>
            public void Start()
            {
                //既に監視しているタスクが動作しているなら処理をしない
                if (_isRun == true) return;

                //キャンセルトークンを作成
                _tokenSource = new CancellationTokenSource();
                _cancelToken = _tokenSource.Token;

                //タスクを開始
                _task = Task.Run(() => Run(_cancelToken));
                _isRun = true;
            }

            private async Task Run(CancellationToken token)
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    _irc.SendIrcMessage("PING irc.twitch.tv");
                    await Task.Delay(300000).ConfigureAwait(false); // 5 minutes
                }
            }

            /// <summary>
            /// 現在監視しているタスクが動作しているかどうかを返します
            /// </summary>
            /// <returns>タスクが動作しているならtrue。それ以外はfalse。</returns>
            public bool IsTaskRunning()
            {
                if (_task == null) return false;
                if (_task.IsCompleted) return false;
                return true;
            }

            /// <summary>
            /// このメソッドを呼ぶことでタスクを強制終了させます
            /// </summary>
            public void End()
            {
                if (IsTaskRunning()) 
                { 
                    _tokenSource.Cancel();
                    _isRun = false;
                }
            }
        }
    }
}