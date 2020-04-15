using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class Coverage
    {
        public static Coverage Create(uint linesCovered, uint lineNotCovered, uint blocksCovered, uint blocksNotCovered)
        {
            return new Coverage(linesCovered, lineNotCovered, blocksCovered, blocksNotCovered);
        }

        private Coverage(uint linesCovered, uint lineNotCovered, uint blocksCovered, uint blocksNotCovered)
        {
            LinesCovered = linesCovered;
            LinesNotCovered = lineNotCovered;
            BlocksCovered = blocksCovered;
            BlocksNotCovered = blocksNotCovered;
        }

        public override string ToString()
        {
            return $"Lines: {LinesCovered}/{TotalLines}({LinesCoveredPercentage}) Branch: {BlocksCovered}/{TotalBlocks}({BlocksCoveredPercentage})";
        }

        [JsonProperty("lines-covered")]
        public uint LinesCovered { get; }

        [JsonProperty("lines-not-covered")]
        public uint LinesNotCovered { get; }

        [JsonProperty("branches-covered")]
        public uint BlocksCovered { get; }

        [JsonProperty("branches-not-covered")]
        public uint BlocksNotCovered { get; }

        [JsonProperty("total-lines")]
        public uint TotalLines => LinesCovered + LinesNotCovered;

        [JsonProperty("total-branches")]
        public uint TotalBlocks => BlocksCovered + BlocksNotCovered;

        [JsonProperty("lines-covered-percentage")]
        public string LinesCoveredPercentage => $"{LinesCoverage:P}";

        [JsonIgnore]
        public decimal LinesCoverage => decimal.Divide(LinesCovered, TotalLines == 0 ? 1 : TotalLines);

        [JsonProperty("branches-covered-percentage")]
        public string BlocksCoveredPercentage => $"{BlockCoverage:P}";

        [JsonIgnore]
        public decimal BlockCoverage => decimal.Divide(BlocksCovered, TotalBlocks == 0 ? 1 : TotalBlocks);
    }
}