using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFIndexer.DuplicateManager
{
    internal class DuplicateItemListControl : CheckedListBox
    {
        public long FileSize { get; set; }
    }
}
