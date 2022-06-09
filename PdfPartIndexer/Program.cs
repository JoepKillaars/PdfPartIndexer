using PdfPartIndexer.Utils;

namespace PdfPartIndexer;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var rootDir = @"d:\workdir";
            var indexer = new PdfIndexer
            {
                IndexFileLocation = Path.Combine(rootDir, "index.json"),
                BackupDirectory = Path.Combine(rootDir, ".backup"),
                PartsDirectory = Path.Combine(rootDir, "parts"),
                SongDirectory = @"C:\Users\joep.killaars.AX\Dropbox\Private\Music\Wa'n Brass\Arrangementen",
            };

            await indexer.Run().ConfigureAwait(false);

            LogHelper.DisplayContextResult();
        }
        catch (Exception ex)
        {
            LogHelper.LogException(ex);
        }

        Console.ReadLine();
    }
}

