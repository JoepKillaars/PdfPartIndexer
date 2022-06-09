using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfPartIndexer.Indexer;
internal class IndexFile
{
    public IEnumerable<IndexFilePart> Parts { get; set; }

    public IEnumerable<IndexFileSong> Songs { get; set; }
}
