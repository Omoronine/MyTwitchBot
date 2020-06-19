using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Omoronine.TwitchBot.Bouyomichanhan
{
    class VoiceroidTalkUserDataWriter
    {
        //チャット発言者が設定した情報を保存するための辞書
        public Dictionary<string, string> UserData { get; set; }

        /// <summary>
        /// TwitchIrcの設定をjson形式で保存します。
        /// </summary>
        /// <param name="fileName">保存するファイルパス</param>
        /// <param name="TwitchIrcConfig">保存する設定を含むTwitchIrcConfigクラス</param>
        public void WriteJsonFromFile(in string fileName, in Dictionary<string, string> userData)
        {
            UserData = userData;
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
                    JsonSerializer.SerializeAsync<Dictionary<string, string>>(fs, userData);
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