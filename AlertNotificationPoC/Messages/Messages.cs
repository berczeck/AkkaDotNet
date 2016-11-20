using System.Collections.Generic;

namespace Messages
{
    public class Constant
    {
        public const int MessageCount = 100*100;
        public const int WorkerCount = 50;
    }
    public class StartAnalysis
    {
        public string Text { get; set; }
    }

    public class FinishAnalysis
    {
    }

    public class AnalysisFinished
    {
        public List<AnalyzeDetail> Result { get; set; }
    }

    public class CategoryAnalyzed
    {
        public List<AnalyzeDetail> Result { get; set; }
    }

    public class KeywordAnalyzed
    {
        public List<AnalyzeDetail> Result { get; set; }
    }

    public class AnalyzeDetail
    {
        public string Code { get; set; }
        public string Category { get; set; }
    }

    public class SendRealTimeNotification
    {
    }

    public class SendSms
    {
        public int Index { get; set; }
    }

    public class SendEmail
    {
        public int Index { get; set; }
    }
}
