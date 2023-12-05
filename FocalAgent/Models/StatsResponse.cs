namespace FocalAgent.Models
{
    public class StatsResponse
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AverageWatchDurationS { get; set; }
        public int Watches { get; set; }
        public int ReleaseYear { get; set; }
    }
}
