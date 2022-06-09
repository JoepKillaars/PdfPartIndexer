using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfPartIndexer.Utils;

internal class LogContext
{
    private IList<string> _warnings = new List<string>();
    private IList<string> _errors = new List<string>();

    public IEnumerable<string> Warnings => this._warnings.ToArray();

    public IEnumerable<string> Errors => this._errors.ToArray();

    public void AddWarning(string warning)
        => this._warnings.Add(warning);

    public void AddError(string error)
        => this._errors.Add(error);
}
