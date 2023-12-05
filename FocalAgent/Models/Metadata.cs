namespace FocalAgent.Models
{
    public class Metadata
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
    }
}
