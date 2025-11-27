using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PDFIndexerShared
{
    public struct OCRRegion
    {
        [JsonInclude]
        public string Text;
        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
        public float Score;

        [JsonInclude]
        public int CenterX;
        [JsonInclude]
        public int CenterY;
        [JsonInclude]
        public int Width;
        [JsonInclude]
        public int Height;
        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
        public float Angle;
    }
}
