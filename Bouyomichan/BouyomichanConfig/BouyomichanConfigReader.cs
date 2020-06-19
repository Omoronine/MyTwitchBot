using System;
using System.IO;
using System.Text.Json;

namespace Omoronine.Bouyomichan
{
    /// <summary>
    /// BouyomichanConfigクラスのえっていデータを読み込むためのクラスです。
    /// 個人用なので、jsonファイルから読み込む実装のみとなっております。
    /// </summary>
    public class BouyomichanConfigReader
    {
        //jsonファイルから読み込むために必要なフィールド
        public BouyomichanConfig BouyomichanConfig{ get; set; }

        /// <summary>
        /// 指定されたjsonファイルから棒読みちゃんの設定情報を読み込みます。
        /// </summary>
        /// <param name="filename">読み込むファイルパス</param>
        /// <returns>読み込んだ設定情報を含むBouyomichanConfigクラスの参照を返します。失敗時はnullを返します。</returns>
        public BouyomichanConfig ReadJsonFromFile( in string filename )
        {            
            //指定した場所のディレクトリを取得する
            string directoryName = Path.GetDirectoryName(filename);
            //ディレクトリを調べ、なければnullを返す
            if (!Directory.Exists(directoryName))
            {
                BouyomichanConfig = null;
                return null;
            }
            //ファイルがあるかを調べ、なければnullを返す
            if (!File.Exists(filename))
            {
                BouyomichanConfig = null;
                return null;
            }

            try
            {
                //引数で指定されたJsonファイルを読み込む
                using (StreamReader sr = new StreamReader(filename))
                {
                    //読み込んだファイルをメンバに収納
                    string jsonString = sr.ReadToEnd();
                    // デシリアライズして _twitchIrcInfo にセット
                    BouyomichanConfig = JsonSerializer.Deserialize<BouyomichanConfigReader>(jsonString).BouyomichanConfig;
                }
                return BouyomichanConfig;
            }
            catch (FieldAccessException)
            {
                //ファイルが見つからなかったらnullを返す
                BouyomichanConfig = null;
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                //ディレクトリが見つからなかったらnullを返す
                BouyomichanConfig = null;
                return null;
            }
            catch (FileNotFoundException)
            {
                //ファイルが見つからなかったらnullを返す
                BouyomichanConfig = null;
                return null;
            }
            catch (JsonException)
            {
                //ファイル内容がおかしい場合はnullを返す
                BouyomichanConfig = null;
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

