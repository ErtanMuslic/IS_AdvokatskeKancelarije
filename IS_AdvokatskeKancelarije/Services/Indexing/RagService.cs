using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace IS_AdvokatskeKancelarije.Services
{
    public class RagSettings
    {
        public int TopK { get; set; } = 5;

        // Ispod ove sličnosti izvod se ne šalje modelu; vrednost se podešava tokom evaluacije
        public double MinScore { get; set; } = 0.30;
    }

    public record AssistantSource(
        int Number,
        Guid DocumentId,
        string? DocumentName,
        int? PageNumber,
        double Score,
        string Text);

    /// <param name="UnverifiedNumbers">Brojevi iz odgovora koji se ne nalaze u izvodima (moguća izmišljotina).</param>
    public record AssistantAnswer(
        string Answer,
        List<AssistantSource> Sources,
        bool Grounded,
        List<string>? UnverifiedNumbers = null);

    /// <summary>RAG: pronalazi relevantne delove dokumenata i traži od modela odgovor zasnovan na njima.</summary>
    public class RagService
    {
        private readonly SearchService _search;
        private readonly OllamaService _ollama;
        private readonly RagSettings _settings;

        public RagService(SearchService search, OllamaService ollama, IOptions<RagSettings> settings)
        {
            _search = search;
            _ollama = ollama;
            _settings = settings.Value;
        }

        public async Task<AssistantAnswer> AskAsync(string question, Guid? caseId, CancellationToken ct)
        {
            var hits = await _search.SearchAsync(question, caseId, _settings.TopK, ct);
            var relevant = hits.Where(h => h.Score >= _settings.MinScore).ToList();

            // Nema dovoljno relevantnog sadržaja: ne pitamo model, jer bi mogao da izmišlja odgovor
            if (relevant.Count == 0)
                return new AssistantAnswer(RagPromptBuilder.NotFoundAnswer, new List<AssistantSource>(), false);

            var messages = RagPromptBuilder.Build(question, relevant);
            var answer = await _ollama.ChatAsync(messages, ct: ct);

            // Model je sam odbio da odgovori: ne prikazujemo izvore uz odgovor koji ih ne koristi
            if (IsRefusal(answer))
                return new AssistantAnswer(RagPromptBuilder.NotFoundAnswer, new List<AssistantSource>(), false);

            var sources = relevant
                .Select((h, i) => new AssistantSource(i + 1, h.DocumentId, h.DocumentName, h.PageNumber, h.Score, h.Text))
                .ToList();

            var unverified = FindUnverifiedNumbers(answer, question, relevant.Select(h => h.Text));
            return new AssistantAnswer(answer, sources, true, unverified);
        }

        internal static bool IsRefusal(string answer) =>
            answer.Contains("nisam pronašao odgovor", StringComparison.OrdinalIgnoreCase);

        private static readonly Regex NumberRegex = new(@"\d+(?:[.,]\d+)*", RegexOptions.Compiled);
        private static readonly Regex WordRegex = new(@"\p{L}+", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> NumberWords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["jedan"] = "1",
            ["jedna"] = "1",
            ["jedno"] = "1",
            ["jedne"] = "1",
            ["dva"] = "2",
            ["dve"] = "2",
            ["tri"] = "3",
            ["četiri"] = "4",
            ["pet"] = "5",
            ["šest"] = "6",
            ["sedam"] = "7",
            ["osam"] = "8",
            ["devet"] = "9",
            ["deset"] = "10",
            ["jedanaest"] = "11",
            ["dvanaest"] = "12",
            ["trinaest"] = "13",
            ["četrnaest"] = "14",
            ["petnaest"] = "15",
            ["šesnaest"] = "16",
            ["sedamnaest"] = "17",
            ["osamnaest"] = "18",
            ["devetnaest"] = "19",
            ["dvadeset"] = "20",
            ["trideset"] = "30",
            ["četrdeset"] = "40",
            ["pedeset"] = "50",
            ["šezdeset"] = "60",
            ["stotinu"] = "100"
        };

        // "1.500.000,00" i "1500000" su isti broj
        private static string Normalize(string number) => number.Replace(".", "").Replace(",", "");

        /// <summary>
        /// Vraća brojeve (dve ili više cifara) koji se pojavljuju u odgovoru, a ne postoje ni u izvodima ni u pitanju.
        /// Oznake izvora [1] i brojevi stavki liste se ne računaju. Brojevi zapisani rečima u izvodu važe kao cifre.
        /// </summary>
        internal static List<string> FindUnverifiedNumbers(string answer, string question, IEnumerable<string> sourceTexts)
        {
            var known = new HashSet<string>();

            foreach (var text in sourceTexts.Append(question))
            {
                foreach (Match m in NumberRegex.Matches(text))
                {
                    known.Add(Normalize(m.Value));
                    // Iznos sa decimalama ("5.000.000,00"): važi i bez decimala
                    var comma = m.Value.LastIndexOf(',');
                    if (comma > 0 && m.Value.Length - comma - 1 == 2)
                        known.Add(Normalize(m.Value[..comma]));
                }

                foreach (Match w in WordRegex.Matches(text))
                    if (NumberWords.TryGetValue(w.Value, out var digits))
                        known.Add(digits);
            }

            var cleaned = Regex.Replace(answer, @"\[\d+(?:\s*,\s*\d+)*\]", " ");
            cleaned = Regex.Replace(cleaned, @"(?m)^\s*\d+[.)]\s", " ");

            var result = new List<string>();
            foreach (Match m in NumberRegex.Matches(cleaned))
            {
                var normalized = Normalize(m.Value);
                if (normalized.Length < 2 || known.Contains(normalized) || result.Contains(m.Value))
                    continue;
                result.Add(m.Value);
            }

            return result;
        }
    }
}
