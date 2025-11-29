using Lucene.Net.Index;
using Lucene.Net.Search;
using PDFIndexer.BackgroudTask;
using PDFIndexer.BackgroundTask;
using PDFIndexer.DuplicateManager;
using PDFIndexer.Models.Database;
using PDFIndexer.SearchEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFIndexer
{
    public partial class DuplicateManagerView : Form
    {
        private static readonly Properties.Settings AppSettings = Properties.Settings.Default;

        private Dictionary<string, long> SelectedFiles = new Dictionary<string, long>();

        public DuplicateManagerView()
        {
            InitializeComponent();
        }

        private void DuplicateManagerView_Load(object sender, EventArgs e)
        {
            LoadDuplicates();
        }

        private async void LoadDuplicates()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 0;
            tableLayoutPanel1.RowStyles.Clear();

            SelectedFiles.Clear();

            var duplicates = new Dictionary<string, HashSet<DuplicateItem>>();
            var sizes = new Dictionary<string, long>();

            // 중복 파일 로드
            await Task.Run(() =>
            {
                var found = new List<string>();
                FindAllPdfFiles(ref found, AppSettings.BasePath, true);

                var dbCollection = DBContext.DB.GetCollection<IndexedDocument>("indexed");
                foreach (var path in found)
                {
                    var dbItem = dbCollection.FindOne(LiteDB.Query.EQ("Path", path));
                    if (dbItem == null) continue;

                    var record = new DuplicateItem(dbItem.Path, dbItem.MD5, dbItem.LastModified);
                    if (duplicates.ContainsKey(dbItem.MD5))
                    {
                        if (!duplicates[dbItem.MD5].Contains(record))
                            duplicates[dbItem.MD5].Add(record);
                    } else
                    {
                        duplicates.Add(dbItem.MD5, new HashSet<DuplicateItem> { record });
                        sizes.Add(dbItem.MD5, new FileInfo(path).Length);
                    }
                }
            });

            foreach (var dup in duplicates) {
                if (dup.Value.Count == 1) continue;

                long size = sizes[dup.Key];
                float totalSize = (float)Math.Round(size * dup.Value.Count / 1024f);
                string totalSizeUnit = "KB";
                if (totalSize >= 1024)
                {
                    totalSize = (float)Math.Round(totalSize / 1024f, 2);
                    totalSizeUnit = "MB";
                }
                var label = new Label()
                {
                    Text = $"{dup.Value.Count}개 항목 ({totalSize}{totalSizeUnit})",
                    AutoSize = true,
                };
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayoutPanel1.Controls.Add(label);

                var list = new DuplicateItemListControl
                {
                    FileSize = size,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    CheckOnClick = true,
                    Margin = new Padding(0, 0, 0, 32),
                };
                foreach (var item in dup.Value)
                {
                    list.Items.Add(item.Path);
                }
                list.SelectedIndexChanged += DuplicateItemList_SelectedIndexChanged;
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayoutPanel1.Controls.Add(list);
                list.ClientSize = new Size(1, list.GetItemHeight(0) * list.Items.Count);
            }
        }

        private void DuplicateItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DuplicateItemListControl list = (DuplicateItemListControl)sender;

            string[] items = new string[list.Items.Count];
            list.Items.CopyTo(items, 0);

            List<string> notChecked = new List<string>(items);
            foreach (string item in list.CheckedItems)
            {
                if (!SelectedFiles.ContainsKey(item))
                    SelectedFiles.Add(item, list.FileSize);
                notChecked.Remove(item);
            }

            foreach (string item in notChecked)
            {
                SelectedFiles.Remove(item);
            }

            UpdateSelectedTotalSize();
        }

        private void UpdateSelectedTotalSize()
        {
            long total = SelectedFiles.Values.Sum();
            float converted = (float)Math.Round(total / 1024f);
            string convertedUnit = "KB";
            if (converted >= 1048576)
            {
                converted = (float)Math.Round(total / 1024f / 1024f);
                convertedUnit = "MB";
            } else if (converted >= 1024)
            {
                converted = (float)Math.Round(total / 1024f / 1024f);
                convertedUnit = "GB";
            }

            SelectedTotalSizeLabel.Text = $"{SelectedFiles.Count}개 ({converted}{convertedUnit})";
        }

        private void FindAllPdfFiles(ref List<string> found, string path, bool recursive = false)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.EndsWith(".pdf"))
                {
                    found.Add(file);
                }
            }

            if (recursive)
            {
                var dirs = Directory.GetDirectories(path);
                foreach (var dir in dirs)
                {
                    FindAllPdfFiles(ref found, dir, true);
                }
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            LoadDuplicates();
        }

        private async void SelectDeleteButton_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show($"정말로 선택한 {SelectedFiles.Count}개의 파일을 삭제할까요?\n\n이 작업은 되둘릴 수 없습니다!", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            var basePath = AppSettings.BasePath;
            var failed = new Dictionary<string, string>(); // 경로, 이유

            await Task.Run(() =>
            {
                foreach (var file in SelectedFiles)
                {
                    string path = file.Key;
                    try
                    {
                        File.Delete(path);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // TODO: 이미 없음
                    } catch (UnauthorizedAccessException)
                    {
                        failed.Add(path.Replace(basePath, ""), "권한 없음");
                    } catch (IOException)
                    {
                        failed.Add(path.Replace(basePath, ""), "다른 프로그램이 파일 사용중");
                    } catch
                    {
                        failed.Add(path.Replace(basePath, ""), "알 수 없음");
                    }

                    TaskManager.Enqueue(new RemoveIndexTask(path), priority: true);
                }
            });

            if (failed.Count == 0)
            {
                MessageBox.Show($"모든 {SelectedFiles.Count}개 파일을 성공적으로 삭제했습니다.", "삭제 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                var message = $"파일 {SelectedFiles.Count}개 중 {failed.Count}개 파일 삭제를 실패했습니다.\n\n";
                foreach (var fail in failed)
                {
                    message += $"- {fail.Value} : {fail.Key}\n";
                }
                MessageBox.Show(message, "삭제 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
