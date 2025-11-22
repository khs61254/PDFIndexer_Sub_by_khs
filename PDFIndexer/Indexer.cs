using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using PDFIndexer.Journal;
using static Lucene.Net.Documents.Field;
using static Lucene.Net.Util.Packed.PackedInt32s;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.Analysis.Cjk;

public delegate void ReadyEventHandler();

namespace PDFIndexer
{
    public class Indexer : IDisposable
    {
        private bool _IsDisposed;

        private LuceneDirectory indexDirectory;

        private const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;

        private string indexPath;

        private Analyzer analyzer;
        private IndexReader reader;
        private IndexSearcher searcher;

        private bool _ready = false;
        public bool ready { get { return _ready; } }

        /// <summary>
        /// Indexer가 준비될때 발생합니다. 이벤트가 Main Thread에서 발생하지 않을 수도 있습니다.
        /// </summary>
        public event ReadyEventHandler OnReady;

        public Indexer(string indexPath)
        {
            this.indexPath = indexPath;
        }

        #region Finalize 관련
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_IsDisposed) return;

            if (disposing)
            {
                if (indexDirectory != null) indexDirectory.Dispose();
                if (analyzer != null) analyzer.Dispose();
                if (reader != null) reader.Dispose();
            }

            _IsDisposed = true;
        }

        ~Indexer()
        {
            Dispose(false);
        }
        #endregion Finalize 관련

        #region 이벤트 관련
        private void SetReadyState(bool ready)
        {
            _ready = ready;

            if (ready) Ready();
        }

        protected virtual void Ready()
        {
            Logger.Write(JournalLevel.Info, "Indexer Ready");
            OnReady?.Invoke();
        }
        #endregion 이벤트 관련
    }
}
