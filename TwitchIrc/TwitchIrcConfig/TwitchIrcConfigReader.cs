using System;
using System.IO;
using System.Text.Json;

namespace Omoronine.TwitchIrc
{
    /// <summary>
    /// TwitchIrcの設定データを読み込むクラスです。
    /// 個人用の為、jsonファイルのみを読み込みます。
    /// </summary>
    class TwitchIrcConfigReader
    {
        //jsonファイルから読み込むために必要なフィールド
        public TwitchIrcConfig TwitchIrcConfig { get; set; }

        /// <summary>
        /// 指定されたjsonファイルからTwitchIrcの設定情報を読み込みます。
        /// </summary>
        /// <param name="filename">読み込むファイルパス</param>
        /// <returns>読み込んだ設定情報を含むTwitchIrcConfigeクラスの参照を返します。失敗時はnullを返します。</returns>
        public TwitchIrcConfig ReadJsonFromFile(in string filename)
        {
            //指定した場所のディレクトリを取得する
            string directoryName = Path.GetDirectoryName(filename);
            //ディレクトリを調べ、なければnullを返す
            if (!Directory.Exists(directoryName))
            {
                TwitchIrcConfig = null;
                return null;
            }
            //ファイルがあるかを調べ、なければnullを返す
            if (!File.Exists(filename))
            {
                TwitchIrcConfig = null;
                return null;
            }

            try
            {
                //引数で指定されたJsonファイルを読み込む
                using (StreamReader sr = new StreamReader(filename))
                {
                    //読み込んだファイルをメンバに収納
                    string jsonString = sr.ReadToEnd();
                    // デシリアライズして TwitchIrcConfig にセット
                    TwitchIrcConfig = JsonSerializer.Deserialize<TwitchIrcConfig>(jsonString);
                }
                return TwitchIrcConfig;
            }
            catch (FieldAccessException)
            {
                //ファイルが見つからなかったらnullを返す
                TwitchIrcConfig = null;
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                //ディレクトリが見つからなかったらnullを返す
                TwitchIrcConfig = null;
                return null;
            }
            catch (FileNotFoundException)
            {
                //ファイルが見つからなかったらnullを返す
                TwitchIrcConfig = null;
                return null;
            }
            catch (JsonException)
            {
                //ファイル内容がおかしい場合はnullを返す
                TwitchIrcConfig = null;
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
