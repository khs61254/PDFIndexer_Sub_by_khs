using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Cjk;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using PDFIndexer.Journal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace PDFIndexer.SearchEngine
{
    internal class LuceneProvider
    {
        private string BasePath;

        private const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;

        private LuceneDirectory IndexDirectory;
        private Analyzer analyzer;
        private IndexReader reader;
        private IndexSearcher searcher;

        public LuceneProvider(string basePath)
        {
            BasePath = basePath;

            Initialize();
        }

        public void Initialize()
        {
            Logger.Write(JournalLevel.Info, "LuceneProvider 초기화");

            IndexDirectory = FSDirectory.Open(Path.Combine(BasePath, ".index"));
            analyzer = new CJKAnalyzer(luceneVersion);

            try
            {
                searcher = InitializeIndexSearcher();
                // SetReadyState(true);

                Logger.Write("기존 인덱스 찾음");
            }
            catch (DirectoryNotFoundException)
            {
                // 인덱스 없음
                searcher = null;

                Logger.Write(JournalLevel.Warning, "디스크에 저장된 인덱스 없음");
            }

            Logger.Write(JournalLevel.Info, "LuceneProvider 초기화 --> 완료");
        }

        private IndexSearcher InitializeIndexSearcher()
        {
            reader = DirectoryReader.Open(IndexDirectory);
            return new IndexSearcher(reader);
        }

        public IndexWriter GetIndexWriter()
        {
            IndexWriterConfig indexConfig = new IndexWriterConfig(luceneVersion, analyzer);
            indexConfig.OpenMode = OpenMode.CREATE;
            IndexWriter writer = new IndexWriter(IndexDirectory, indexConfig);

            return writer;
        }

        public IndexReader GetIndexReader()
        {
            return reader;
        }

        public Analyzer GetAnalyzer()
        {
            return analyzer;
        }

        public IndexSearcher GetIndexSearcher()
        {
            return searcher;
        }

        public Document SearchDocument(int doc)
        {
            return searcher.Doc(doc);
        }
    }
}
