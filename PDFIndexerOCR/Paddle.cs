using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFIndexerOCR
{
    internal class Paddle
    {
//#if DEBUG
//        private const bool debug = true;
//#else
//        private const bool debug = false;
//#endif

        FullOcrModel model = LocalFullModels.KoreanV4;

        Action<PaddleConfig> device;

        public Paddle()
        {
            device = PaddleDevice.Onnx(cpuMathThreadCount: 4, glogEnabled: false);
        }

        public PaddleOcrResult OCR(byte[] image)
        {
            using (PaddleOcrAll all = new PaddleOcrAll(model, device)
            {
                // 요거 켜면 정확도 낮아짐
                //AllowRotateDetection = true,
                //Enable180Classification = true,
            })
            {
                using (Mat src = Cv2.ImDecode(image, ImreadModes.Color))
                {
                    PaddleOcrResult result = all.Run(src);

                    return result;
                }
            }
        }
    }
}
