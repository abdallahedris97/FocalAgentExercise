using CsvHelper;
using FocalAgent.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FocalAgent.Controllers
{
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private const string MetadataFilePath = "metadata.csv";
        private const string MovieStatsFilePath = "stats.csv";

        [HttpPost("metadata")]
        public IActionResult PostMetadata([FromBody] Metadata metadata)
        {
            List<Metadata> metadataList = ReadMetadataFromCsv();
            metadataList.Add(metadata);
            WriteMetadataToCsv(metadataList);

            return Ok();
        }

        [HttpGet("metadata/{movieId}")]
        public IActionResult GetMetadata(int movieId)
        {
            List<Metadata> metadataList = ReadMetadataFromCsv();

            var metadata = metadataList
                .Where(m => m.MovieId == movieId)
                .GroupBy(m => m.Language)
                .Select(g => g.OrderByDescending(m => m.MovieId).First())
                .OrderBy(m => m.Language)
                .ToList();

            if (metadata.Count == 0)
            {
                return NotFound();
            }

            return Ok(metadata);
        }

        [HttpGet("movies/stats")]
        public IActionResult GetMovieStats()
        {
            List<MovieStats> statsRecords = ReadMovieStatsFromCsv();
            List<Metadata> metadataRecords = ReadMetadataFromCsv();

            var mergedData = from stats in statsRecords
                             join metadata in metadataRecords on stats.movieId equals metadata.MovieId
                             select new
                             {
                                 stats.movieId,
                                 metadata.Title,
                                 AverageWatchDurationS = stats.watchDurationMs / 1000,
                                 stats.watchDurationMs,
                                 metadata.ReleaseYear
                             };

            var groupedData = mergedData.GroupBy(x => x.movieId)
                                         .Select(group => new
                                         {
                                             MovieId = group.Key,
                                             Title = group.First().Title,
                                             AverageWatchDurationS = (long)group.Average(x => x.AverageWatchDurationS),
                                             ReleaseYear = group.First().ReleaseYear
                                         });

            var sortedData = groupedData.OrderByDescending(x => x.AverageWatchDurationS);

            var outputData = sortedData.Select(item => new
            {
                movieId = item.MovieId,
                title = item.Title,
                averageWatchDurationS = item.AverageWatchDurationS,
                watches = statsRecords.LongCount(s => s.movieId == item.MovieId),
                releaseYear = item.ReleaseYear
            });

            return Ok(outputData);
        }

        private List<Metadata> ReadMetadataFromCsv()
        {
            if (!System.IO.File.Exists(MetadataFilePath))
            {
                return new List<Metadata>();
            }

            using (var reader = new StreamReader(MetadataFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<Metadata>().ToList();
            }
        }

        private void WriteMetadataToCsv(List<Metadata> metadataList)
        {
            using (var writer = new StreamWriter(MetadataFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(metadataList);
            }
        }

        private List<MovieStats> ReadMovieStatsFromCsv()
        {
            if (!System.IO.File.Exists(MovieStatsFilePath))
            {
                return new List<MovieStats>();
            }

            using (var reader = new StreamReader(MovieStatsFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<MovieStats>().ToList();
            }
        }
    }
}