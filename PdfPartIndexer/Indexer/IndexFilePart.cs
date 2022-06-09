namespace PdfPartIndexer.Indexer;

public class IndexFilePart
{
    public string Instrument { get; set; }

    public int? PartNumber { get; set; }

    public bool Optional { get; set; }

    public string Abbreviations { get; set; }
}