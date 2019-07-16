using Microsoft.ML.Data;

namespace WebRanking.DataStructures
{
    public class SearchResultData
    {
        [ColumnName("Label"), LoadColumn(0)]
        public uint Label { get; set; }


        [ColumnName("GroupId"), LoadColumn(1)]
        public uint GroupId { get; set; }


        [ColumnName("CoveredQueryTermNumberAnchor"), LoadColumn(2)]
        public float CoveredQueryTermNumberAnchor { get; set; }


        [ColumnName("CoveredQueryTermNumberTitle"), LoadColumn(3)]
        public float CoveredQueryTermNumberTitle { get; set; }


        [ColumnName("CoveredQueryTermNumberUrl"), LoadColumn(4)]
        public float CoveredQueryTermNumberUrl { get; set; }


        [ColumnName("CoveredQueryTermNumberWholeDocument"), LoadColumn(5)]
        public float CoveredQueryTermNumberWholeDocument { get; set; }


        [ColumnName("CoveredQueryTermNumberBody"), LoadColumn(6)]
        public float CoveredQueryTermNumberBody { get; set; }


        [ColumnName("CoveredQueryTermRatioAnchor"), LoadColumn(7)]
        public float CoveredQueryTermRatioAnchor { get; set; }


        [ColumnName("CoveredQueryTermRatioTitle"), LoadColumn(8)]
        public float CoveredQueryTermRatioTitle { get; set; }


        [ColumnName("CoveredQueryTermRatioUrl"), LoadColumn(9)]
        public float CoveredQueryTermRatioUrl { get; set; }


        [ColumnName("CoveredQueryTermRatioWholeDocument"), LoadColumn(10)]
        public float CoveredQueryTermRatioWholeDocument { get; set; }


        [ColumnName("CoveredQueryTermRatioBody"), LoadColumn(11)]
        public float CoveredQueryTermRatioBody { get; set; }


        [ColumnName("StreamLengthAnchor"), LoadColumn(12)]
        public float StreamLengthAnchor { get; set; }


        [ColumnName("StreamLengthTitle"), LoadColumn(13)]
        public float StreamLengthTitle { get; set; }


        [ColumnName("StreamLengthUrl"), LoadColumn(14)]
        public float StreamLengthUrl { get; set; }


        [ColumnName("StreamLengthWholeDocument"), LoadColumn(15)]
        public float StreamLengthWholeDocument { get; set; }


        [ColumnName("StreamLengthBody"), LoadColumn(16)]
        public float StreamLengthBody { get; set; }


        [ColumnName("IdfAnchor"), LoadColumn(17)]
        public float IdfAnchor { get; set; }


        [ColumnName("IdfTitle"), LoadColumn(18)]
        public float IdfTitle { get; set; }


        [ColumnName("IdfUrl"), LoadColumn(19)]
        public float IdfUrl { get; set; }


        [ColumnName("IdfWholeDocument"), LoadColumn(20)]
        public float IdfWholeDocument { get; set; }


        [ColumnName("IdfBody"), LoadColumn(21)]
        public float IdfBody { get; set; }


        [ColumnName("SumTfAnchor"), LoadColumn(22)]
        public float SumTfAnchor { get; set; }


        [ColumnName("SumTfTitle"), LoadColumn(23)]
        public float SumTfTitle { get; set; }


        [ColumnName("SumTfUrl"), LoadColumn(24)]
        public float SumTfUrl { get; set; }


        [ColumnName("SumTfWholeDocument"), LoadColumn(25)]
        public float SumTfWholeDocument { get; set; }


        [ColumnName("SumTfBody"), LoadColumn(26)]
        public float SumTfBody { get; set; }


        [ColumnName("MinTfAnchor"), LoadColumn(27)]
        public float MinTfAnchor { get; set; }


        [ColumnName("MinTfTitle"), LoadColumn(28)]
        public float MinTfTitle { get; set; }


        [ColumnName("MinTfUrl"), LoadColumn(29)]
        public float MinTfUrl { get; set; }


        [ColumnName("MinTfWholeDocument"), LoadColumn(30)]
        public float MinTfWholeDocument { get; set; }


        [ColumnName("MinTfBody"), LoadColumn(31)]
        public float MinTfBody { get; set; }


        [ColumnName("MaxTfAnchor"), LoadColumn(32)]
        public float MaxTfAnchor { get; set; }


        [ColumnName("MaxTfTitle"), LoadColumn(33)]
        public float MaxTfTitle { get; set; }


        [ColumnName("MaxTfUrl"), LoadColumn(34)]
        public float MaxTfUrl { get; set; }


        [ColumnName("MaxTfWholeDocument"), LoadColumn(35)]
        public float MaxTfWholeDocument { get; set; }


        [ColumnName("MaxTfBody"), LoadColumn(36)]
        public float MaxTfBody { get; set; }


        [ColumnName("MeanTfAnchor"), LoadColumn(37)]
        public float MeanTfAnchor { get; set; }


        [ColumnName("MeanTfTitle"), LoadColumn(38)]
        public float MeanTfTitle { get; set; }


        [ColumnName("MeanTfUrl"), LoadColumn(39)]
        public float MeanTfUrl { get; set; }


        [ColumnName("MeanTfWholeDocument"), LoadColumn(40)]
        public float MeanTfWholeDocument { get; set; }


        [ColumnName("MeanTfBody"), LoadColumn(41)]
        public float MeanTfBody { get; set; }


        [ColumnName("VarianceTfAnchor"), LoadColumn(42)]
        public float VarianceTfAnchor { get; set; }


        [ColumnName("VarianceTfTitle"), LoadColumn(43)]
        public float VarianceTfTitle { get; set; }


        [ColumnName("VarianceTfUrl"), LoadColumn(44)]
        public float VarianceTfUrl { get; set; }


        [ColumnName("VarianceTfWholeDocument"), LoadColumn(45)]
        public float VarianceTfWholeDocument { get; set; }


        [ColumnName("VarianceTfBody"), LoadColumn(46)]
        public float VarianceTfBody { get; set; }


        [ColumnName("SumStreamLengthNormalizedTfAnchor"), LoadColumn(47)]
        public float SumStreamLengthNormalizedTfAnchor { get; set; }


        [ColumnName("SumStreamLengthNormalizedTfTitle"), LoadColumn(48)]
        public float SumStreamLengthNormalizedTfTitle { get; set; }


        [ColumnName("SumStreamLengthNormalizedTfUrl"), LoadColumn(49)]
        public float SumStreamLengthNormalizedTfUrl { get; set; }


        [ColumnName("SumStreamLengthNormalizedTfWholeDocument"), LoadColumn(50)]
        public float SumStreamLengthNormalizedTfWholeDocument { get; set; }


        [ColumnName("SumStreamLengthNormalizedTfBody"), LoadColumn(51)]
        public float SumStreamLengthNormalizedTfBody { get; set; }


        [ColumnName("MinStreamLengthNormalizedTfAnchor"), LoadColumn(52)]
        public float MinStreamLengthNormalizedTfAnchor { get; set; }


        [ColumnName("MinStreamLengthNormalizedTfTitle"), LoadColumn(53)]
        public float MinStreamLengthNormalizedTfTitle { get; set; }


        [ColumnName("MinStreamLengthNormalizedTfUrl"), LoadColumn(54)]
        public float MinStreamLengthNormalizedTfUrl { get; set; }


        [ColumnName("MinStreamLengthNormalizedTfWholeDocument"), LoadColumn(55)]
        public float MinStreamLengthNormalizedTfWholeDocument { get; set; }


        [ColumnName("MinStreamLengthNormalizedTfBody"), LoadColumn(56)]
        public float MinStreamLengthNormalizedTfBody { get; set; }


        [ColumnName("MaxStreamLengthNormalizedTfAnchor"), LoadColumn(57)]
        public float MaxStreamLengthNormalizedTfAnchor { get; set; }


        [ColumnName("MaxStreamLengthNormalizedTfTitle"), LoadColumn(58)]
        public float MaxStreamLengthNormalizedTfTitle { get; set; }


        [ColumnName("MaxStreamLengthNormalizedTfUrl"), LoadColumn(59)]
        public float MaxStreamLengthNormalizedTfUrl { get; set; }


        [ColumnName("MaxStreamLengthNormalizedTfWholeDocument"), LoadColumn(60)]
        public float MaxStreamLengthNormalizedTfWholeDocument { get; set; }


        [ColumnName("MaxStreamLengthNormalizedTfBody"), LoadColumn(61)]
        public float MaxStreamLengthNormalizedTfBody { get; set; }


        [ColumnName("MeanStreamLengthNormalizedTfAnchor"), LoadColumn(62)]
        public float MeanStreamLengthNormalizedTfAnchor { get; set; }


        [ColumnName("MeanStreamLengthNormalizedTfTitle"), LoadColumn(63)]
        public float MeanStreamLengthNormalizedTfTitle { get; set; }


        [ColumnName("MeanStreamLengthNormalizedTfUrl"), LoadColumn(64)]
        public float MeanStreamLengthNormalizedTfUrl { get; set; }


        [ColumnName("MeanStreamLengthNormalizedTfWholeDocument"), LoadColumn(65)]
        public float MeanStreamLengthNormalizedTfWholeDocument { get; set; }


        [ColumnName("MeanStreamLengthNormalizedTfBody"), LoadColumn(66)]
        public float MeanStreamLengthNormalizedTfBody { get; set; }


        [ColumnName("VarianceStreamLengthNormalizedTfAnchor"), LoadColumn(67)]
        public float VarianceStreamLengthNormalizedTfAnchor { get; set; }


        [ColumnName("VarianceStreamLengthNormalizedTfTitle"), LoadColumn(68)]
        public float VarianceStreamLengthNormalizedTfTitle { get; set; }


        [ColumnName("VarianceStreamLengthNormalizedTfUrl"), LoadColumn(69)]
        public float VarianceStreamLengthNormalizedTfUrl { get; set; }


        [ColumnName("VarianceStreamLengthNormalizedTfWholeDocument"), LoadColumn(70)]
        public float VarianceStreamLengthNormalizedTfWholeDocument { get; set; }


        [ColumnName("VarianceStreamLengthNormalizedTfBody"), LoadColumn(71)]
        public float VarianceStreamLengthNormalizedTfBody { get; set; }


        [ColumnName("SumTfidfAnchor"), LoadColumn(72)]
        public float SumTfidfAnchor { get; set; }


        [ColumnName("SumTfidfTitle"), LoadColumn(73)]
        public float SumTfidfTitle { get; set; }


        [ColumnName("SumTfidfUrl"), LoadColumn(74)]
        public float SumTfidfUrl { get; set; }


        [ColumnName("SumTfidfWholeDocument"), LoadColumn(75)]
        public float SumTfidfWholeDocument { get; set; }


        [ColumnName("SumTfidfBody"), LoadColumn(76)]
        public float SumTfidfBody { get; set; }


        [ColumnName("MinTfidfAnchor"), LoadColumn(77)]
        public float MinTfidfAnchor { get; set; }


        [ColumnName("MinTfidfTitle"), LoadColumn(78)]
        public float MinTfidfTitle { get; set; }


        [ColumnName("MinTfidfUrl"), LoadColumn(79)]
        public float MinTfidfUrl { get; set; }


        [ColumnName("MinTfidfWholeDocument"), LoadColumn(80)]
        public float MinTfidfWholeDocument { get; set; }


        [ColumnName("MinTfidfBody"), LoadColumn(81)]
        public float MinTfidfBody { get; set; }


        [ColumnName("MaxTfidfAnchor"), LoadColumn(82)]
        public float MaxTfidfAnchor { get; set; }


        [ColumnName("MaxTfidfTitle"), LoadColumn(83)]
        public float MaxTfidfTitle { get; set; }


        [ColumnName("MaxTfidfUrl"), LoadColumn(84)]
        public float MaxTfidfUrl { get; set; }


        [ColumnName("MaxTfidfWholeDocument"), LoadColumn(85)]
        public float MaxTfidfWholeDocument { get; set; }


        [ColumnName("MaxTfidfBody"), LoadColumn(86)]
        public float MaxTfidfBody { get; set; }


        [ColumnName("MeanTfidfAnchor"), LoadColumn(87)]
        public float MeanTfidfAnchor { get; set; }


        [ColumnName("MeanTfidfTitle"), LoadColumn(88)]
        public float MeanTfidfTitle { get; set; }


        [ColumnName("MeanTfidfUrl"), LoadColumn(89)]
        public float MeanTfidfUrl { get; set; }


        [ColumnName("MeanTfidfWholeDocument"), LoadColumn(90)]
        public float MeanTfidfWholeDocument { get; set; }


        [ColumnName("MeanTfidfBody"), LoadColumn(91)]
        public float MeanTfidfBody { get; set; }


        [ColumnName("VarianceTfidfAnchor"), LoadColumn(92)]
        public float VarianceTfidfAnchor { get; set; }


        [ColumnName("VarianceTfidfTitle"), LoadColumn(93)]
        public float VarianceTfidfTitle { get; set; }


        [ColumnName("VarianceTfidfUrl"), LoadColumn(94)]
        public float VarianceTfidfUrl { get; set; }


        [ColumnName("VarianceTfidfWholeDocument"), LoadColumn(95)]
        public float VarianceTfidfWholeDocument { get; set; }


        [ColumnName("VarianceTfidfBody"), LoadColumn(96)]
        public float VarianceTfidfBody { get; set; }


        [ColumnName("BooleanModelAnchor"), LoadColumn(97)]
        public float BooleanModelAnchor { get; set; }


        [ColumnName("BooleanModelTitle"), LoadColumn(98)]
        public float BooleanModelTitle { get; set; }


        [ColumnName("BooleanModelUrl"), LoadColumn(99)]
        public float BooleanModelUrl { get; set; }


        [ColumnName("BooleanModelWholeDocument"), LoadColumn(100)]
        public float BooleanModelWholeDocument { get; set; }


        [ColumnName("BooleanModelBody"), LoadColumn(101)]
        public float BooleanModelBody { get; set; }


        [ColumnName("VectorSpaceModelAnchor"), LoadColumn(102)]
        public float VectorSpaceModelAnchor { get; set; }


        [ColumnName("VectorSpaceModelTitle"), LoadColumn(103)]
        public float VectorSpaceModelTitle { get; set; }


        [ColumnName("VectorSpaceModelUrl"), LoadColumn(104)]
        public float VectorSpaceModelUrl { get; set; }


        [ColumnName("VectorSpaceModelWholeDocument"), LoadColumn(105)]
        public float VectorSpaceModelWholeDocument { get; set; }


        [ColumnName("VectorSpaceModelBody"), LoadColumn(106)]
        public float VectorSpaceModelBody { get; set; }


        [ColumnName("Bm25Anchor"), LoadColumn(107)]
        public float Bm25Anchor { get; set; }


        [ColumnName("Bm25Title"), LoadColumn(108)]
        public float Bm25Title { get; set; }


        [ColumnName("Bm25Url"), LoadColumn(109)]
        public float Bm25Url { get; set; }


        [ColumnName("Bm25WholeDocument"), LoadColumn(110)]
        public float Bm25WholeDocument { get; set; }


        [ColumnName("Bm25Body"), LoadColumn(111)]
        public float Bm25Body { get; set; }


        [ColumnName("LmirAbsAnchor"), LoadColumn(112)]
        public float LmirAbsAnchor { get; set; }


        [ColumnName("LmirAbsTitle"), LoadColumn(113)]
        public float LmirAbsTitle { get; set; }


        [ColumnName("LmirAbsUrl"), LoadColumn(114)]
        public float LmirAbsUrl { get; set; }


        [ColumnName("LmirAbsWholeDocument"), LoadColumn(115)]
        public float LmirAbsWholeDocument { get; set; }


        [ColumnName("LmirAbsBody"), LoadColumn(116)]
        public float LmirAbsBody { get; set; }


        [ColumnName("LmirDirAnchor"), LoadColumn(117)]
        public float LmirDirAnchor { get; set; }


        [ColumnName("LmirDirTitle"), LoadColumn(118)]
        public float LmirDirTitle { get; set; }


        [ColumnName("LmirDirUrl"), LoadColumn(119)]
        public float LmirDirUrl { get; set; }


        [ColumnName("LmirDirWholeDocument"), LoadColumn(120)]
        public float LmirDirWholeDocument { get; set; }


        [ColumnName("LmirDirBody"), LoadColumn(121)]
        public float LmirDirBody { get; set; }


        [ColumnName("LmirJmAnchor"), LoadColumn(122)]
        public float LmirJmAnchor { get; set; }


        [ColumnName("LmirJmTitle"), LoadColumn(123)]
        public float LmirJmTitle { get; set; }


        [ColumnName("LmirJmUrl"), LoadColumn(124)]
        public float LmirJmUrl { get; set; }


        [ColumnName("LmirJmWholeDocument"), LoadColumn(125)]
        public float LmirJmWholeDocument { get; set; }


        [ColumnName("LmirJm"), LoadColumn(126)]
        public float LmirJm { get; set; }


        [ColumnName("NumberSlashInUrl"), LoadColumn(127)]
        public float NumberSlashInUrl { get; set; }


        [ColumnName("LengthUrl"), LoadColumn(128)]
        public float LengthUrl { get; set; }


        [ColumnName("InlinkNumber"), LoadColumn(129)]
        public float InlinkNumber { get; set; }


        [ColumnName("OutlinkNumber"), LoadColumn(130)]
        public float OutlinkNumber { get; set; }


        [ColumnName("PageRank"), LoadColumn(131)]
        public float PageRank { get; set; }


        [ColumnName("SiteRank"), LoadColumn(132)]
        public float SiteRank { get; set; }


        [ColumnName("QualityScore"), LoadColumn(133)]
        public float QualityScore { get; set; }


        [ColumnName("QualityScore2"), LoadColumn(134)]
        public float QualityScore2 { get; set; }


        [ColumnName("QueryUrlClickCount"), LoadColumn(135)]
        public float QueryUrlClickCount { get; set; }


        [ColumnName("UrlClickCount"), LoadColumn(136)]
        public float UrlClickCount { get; set; }


        [ColumnName("UrlDwellTime"), LoadColumn(137)]
        public float UrlDwellTime { get; set; }
    }
}
