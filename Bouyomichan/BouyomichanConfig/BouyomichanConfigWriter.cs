using System;
using System.IO;
using System.Text.Json;

namespace Omoronine.Bouyomichan
{
    /// <summary>
    /// 棒読みちゃんに送る設定をファイルに保存します。
    /// 個人用なので、json形式のみの対応となります。
    /// </summary>
    class BouyomichanConfigWriter
    {
        //ファイル書き込みに必要なフィールド
        public BouyomichanConfig BouyomichanConfig { get; set; }

        /// <summary>
        /// 棒読みちゃんに送る設定をjson形式で保存します。
        /// </summary>
        /// <param name="fileName">保存するファイルパス</param>
        /// <param name="bouyomichanConfig">保存する設定を含むBouyomichanConfigクラス</param>
        public void WriteJsonFromFile(in string fileName , in BouyomichanConfig bouyomichanConfig)
        {
            BouyomichanConfig = bouyomichanConfig;
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
                    JsonSerializer.SerializeAsync<BouyomichanConfigWriter>(fs,this);
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
