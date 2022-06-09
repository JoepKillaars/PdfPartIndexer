using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfPartIndexer.Indexer;

internal static class IndexFilePartExtensions
{
    public static bool IsAudioPart(this IndexFilePart part)
        => part.Instrument == "Audio" && part.PartNumber == null;
}
