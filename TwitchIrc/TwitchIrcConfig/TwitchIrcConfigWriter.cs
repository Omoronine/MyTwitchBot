using System;
using System.IO;
using System.Text.Json;

namespace Omoronine.TwitchIrc
{
    /// <summary>
    /// TwitchIrcの設定をファイルに保存します。
    /// 個人用なので、json形式のみの対応となります。
    /// </summary>
    class TwitchIrcConfigWriter
    {
        //ファイル書き込みに必要なフィールド
        public TwitchIrcConfig TwitchIrcConfig { get; set; }

        /// <summary>
        /// TwitchIrcの設定をjson形式で保存します。
        /// </summary>
        /// <param name="fileName">保存するファイルパス</param>
        /// <param name="TwitchIrcConfig">保存する設定を含むTwitchIrcConfigクラス</param>
        public void WriteJsonFromFile(in string fileName, in TwitchIrcConfig twitchIrcConfig)
        {
            TwitchIrcConfig = twitchIrcConfig;
            //指定した場所にディレクトリがあるかを調べる
            string directoryName = Path.GetDirectoryName(fileName);
            //ディレクトリを調べ、なければ作成
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            try
            {
                //ファイルを上書きして保存。ファイルがない場合は作成。
                using (FileStream fs = File.Create(fileName))
                {
                    JsonSerializer.SerializeAsync<TwitchIrcConfig>(fs, TwitchIrcConfig);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}