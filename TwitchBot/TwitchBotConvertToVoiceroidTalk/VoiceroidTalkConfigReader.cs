using System;
using System.IO;
using System.Text.Json;

namespace Omoronine.TwitchBot.Bouyomichan
{
    /// <summary>
    /// TwitchIrcの設定データを読み込むクラスです。
    /// 個人用の為、jsonファイルのみを読み込みます。
    /// </summary>
    class VoiceroidTalkConfigReader
    {
        //jsonファイルから読み込むために必要なフィールド
        public VoiceroidTalkConfig VoiceroidTalkConfig { get; set; }

        /// <summary>
        /// 指定されたjsonファイルからVoiceroidTalkの設定情報を読み込みます。
        /// </summary>
        /// <param name="filename">読み込むファイルパス</param>
        /// <returns>読み込んだ設定情報を含むVoiceroidTalkConfigeクラスの参照を返します。失敗時はnullを返します。</returns>
        public VoiceroidTalkConfig ReadJsonFromFile(in string filename)
        {
            //指定した場所にディレクトリがあるかを調べる
            string directoryName = Path.GetDirectoryName(filename);
            //ディレクトリを調べ、なければnullを返す
            if (!Directory.Exists(directoryName))
            {
                VoiceroidTalkConfig = null;
                return null;
            }
            //ファイルがあるかを調べ、なければnullを返す
            if (!File.Exists(filename))
            {
                VoiceroidTalkConfig = null;
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
                    VoiceroidTalkConfig = JsonSerializer.Deserialize<VoiceroidTalkConfig>(jsonString);
                }
                return VoiceroidTalkConfig;
            }
            catch (FieldAccessException)
            {
                //ファイルが見つからなかったらnullを返す
                VoiceroidTalkConfig = null;
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                //ディレクトリが見つからなかったらnullを返す
                VoiceroidTalkConfig = null;
                return null;
            }
            catch (FileNotFoundException)
            {
                //ファイルが見つからなかったらnullを返す
                VoiceroidTalkConfig = null;
                return null;
            }
            catch (JsonException)
            {
                //ファイル内容がおかしい場合はnullを返す
                VoiceroidTalkConfig = null;
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