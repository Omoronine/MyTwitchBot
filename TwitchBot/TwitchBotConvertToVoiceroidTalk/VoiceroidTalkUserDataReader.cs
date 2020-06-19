using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Omoronine.TwitchBot.Bouyomichan
{
    class VoiceroidTalkUserDataReader
    {

        //チャット発言者が設定した情報を保存するための辞書
        public Dictionary<string, string> UserData { get; set; }

        /// <summary>
        /// 指定されたjsonファイルからVoiceroidTalkののユーザー情報を読み込みます。
        /// </summary>
        /// <param name="filename">読み込むファイルパス</param>
        /// <returns>読み込んだ設定情報を含む辞書クラスの参照を返します。失敗時はnullを返します。</returns>
        public Dictionary<string, string> ReadJsonFromFile(in string filename)
        {
            //指定した場所にディレクトリがあるかを調べる
            string directoryName = Path.GetDirectoryName(filename);
            //ディレクトリを調べ、なければnullを返す
            if (!Directory.Exists(directoryName))
            {
                UserData = null;
                return null;
            }
            //ファイルがあるかを調べ、なければnullを返す
            if (!File.Exists(filename))
            {
                UserData = null;
                return null;
            }

            try
            {
                //引数で指定されたJsonファイルを読み込む
                using (StreamReader sr = new StreamReader(filename))
                {
                    //読み込んだファイルをメンバに収納
                    string jsonString = sr.ReadToEnd();
                    // デシリアライズして VoiceroidTalkConfig にセット
                    UserData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                }
                return UserData;
            }
            catch (FieldAccessException)
            {
                //ファイルが見つからなかったらnullを返す
                UserData = null;
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                //ディレクトリが見つからなかったらnullを返す
                UserData = null;
                return null;
            }
            catch (FileNotFoundException)
            {
                //ファイルが見つからなかったらnullを返す
                UserData = null;
                return null;
            }
            catch (JsonException)
            {
                //ファイル内容がおかしい場合はnullを返す
                UserData = null;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
