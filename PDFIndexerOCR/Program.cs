using PDFIndexerShared;
using Sdcb.PaddleOCR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDFIndexerOCR
{
    internal class Program
    {
        private static bool SingleMode = false;

        private static string Path;

        private static Paddle OCRProvider;

        static void Main(string[] args)
        {
            Console.WriteLine("PDFIndexerOCR\n");

            ParseArgs(args);

            OCRProvider = new Paddle();

            if (!SingleMode)
            {
                MainProcessWatcher();
                StartPipeServer();
            } else
            {
                OCRSingle();
            }
        }

        private static void ParseArgs(string[] args)
        {
            var priority = ProcessPriorityClass.Idle;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var nextArg = args[i + 1];

                switch (arg)
                {
                    case "-i":
                    case "--input":
                        if (nextArg == null) throw new ArgumentException($"Invalid value for {arg}");
                        SingleMode = true;
                        Path = nextArg;
                        break;

                    case "-p":
                    case "--priority":
                        if (nextArg == null) throw new ArgumentException($"Invalid value for {arg}");

                        switch (nextArg.ToLower())
                        {
                            case "high":
                                priority = ProcessPriorityClass.AboveNormal;
                                break;
                            case "normal":
                                priority = ProcessPriorityClass.Normal;
                                break;
                            case "low":
                                priority = ProcessPriorityClass.BelowNormal;
                                break;
                            case "idle":
                                priority = ProcessPriorityClass.Idle;
                                break;
                            default:
                                throw new ArgumentException($"Invalid value for {arg}");
                        }
                        break;
                }
            }

            Process.GetCurrentProcess().PriorityClass = priority;
        }

        private static void StartPipeServer()
        {
            using (var server = new NamedPipeServerStream("PDFIndexerOCR", PipeDirection.InOut))
            {
                server.WaitForConnection();

                using (var reader = new StreamReader(server))
                using (var writer = new StreamWriter(server) { AutoFlush = true })
                {
                    while (server.IsConnected)
                    {
                        try
                        {
                            string input = reader.ReadLine();

                            // 잘못된 입력 --> 연결 종료
                            if (string.IsNullOrEmpty(input)) break;

                            // TODO: OCR 수행
                            List<OCRRegion> regions = new List<OCRRegion>();

                            PaddleOcrResult result = OCRProvider.OCR(input);
                            foreach (PaddleOcrResultRegion region in result.Regions)
                            {
                                regions.Add(new OCRRegion()
                                {
                                    Text = region.Text,
                                    Score = region.Score,

                                    CenterX = (int)region.Rect.Center.X,
                                    CenterY = (int)region.Rect.Center.Y,
                                    Width = (int)region.Rect.Size.Width,
                                    Height = (int)region.Rect.Size.Height,
                                    Angle = region.Rect.Angle,
                                });
                            }

                            var res = new OCRPipeResponse(result.Text, regions.ToArray());
                            var response = PipeResponse.ToJSON(res);
                            Console.WriteLine(response);
                            writer.WriteLine(response);
                        }
                        catch (Exception e)
                        {
                            // TODO: 클라이언트에 에러 전송

                            Console.Error.WriteLine(e.ToString());
                        }
                    }
                }
            }
        }

        private static async void MainProcessWatcher()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (Process.GetProcessesByName("PDFIndexer").Length == 0)
                    {
                        // Kill
                        Console.WriteLine("No main process detected. Exit");
                        Environment.Exit(0);
                    }

#if DEBUG
                    Thread.Sleep(1000);
#else
                    Thread.Sleep(5000);
#endif
                }
            });
        }

        private static void OCRSingle()
        {
            // TODO:
        }
    }
}
