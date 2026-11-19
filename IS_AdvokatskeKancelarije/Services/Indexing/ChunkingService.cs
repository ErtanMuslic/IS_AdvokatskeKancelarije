using System.Text.RegularExpressions;

namespace IS_AdvokatskeKancelarije.Services
{
    /// <summary>
    /// Deli tekst na delove fiksne približne dužine sa preklapanjem.
    /// Kraj dela se pomera na kraj rečenice (ili reči) ako je moguće.
    /// </summary>
    public class ChunkingService
    {
        public int ChunkSize { get; init; } = 1000; // karaktera
        public int Overlap { get; init; } = 150; //karaktera 
        public int MinChunkLength { get; init; } = 20;

        private static readonly char[] SentenceEnd = { '.', '!', '?' };

        // "Član 113", "Član 44a", "ČLAN 5.", "Члан 14." (ne i "člana 5" u tekstu rečenice)
        private static readonly Regex ArticleStart = new(
            @"(?<![\p{L}\p{N}])(?:Član|ČLAN|Члан|ЧЛАН)\s+\d+[a-zа-я]?\.?(?=\s|$)",
            RegexOptions.Compiled);

        // Dokument se deli po članovima ako ih ima bar toliko
        public int MinArticlesForLegalSplit { get; init; } = 1000000;//5

        // Član duži od ovoga deli se na više delova (svaki počinje oznakom člana)
        public int MaxArticleLength { get; init; } = 1500;

        public List<TextChunk> Chunk(IEnumerable<PageText> pages)
        {
            var pageList = pages.ToList();
            var legal = ChunkByArticles(pageList);
            return legal ?? ChunkByPages(pageList);
        }

        /// <summary>
        /// Za zakone i slične dokumente: jedan deo je jedan član. Ako je član predugačak, deli se na više
        /// delova i svaki nosi oznaku člana. Vraća null ako dokument nema strukturu po članovima.
        /// </summary>
        private List<TextChunk>? ChunkByArticles(List<PageText> pages)
        {
            // Spajamo strane u jedan tekst i pamtimo gde koja strana počinje
            var sb = new System.Text.StringBuilder();
            var pageStarts = new List<(int Offset, int? Page)>();
            foreach (var page in pages)
            {
                var t = Regex.Replace(page.Text ?? string.Empty, @"\s+", " ").Trim();
                if (t.Length == 0) continue;
                if (sb.Length > 0) sb.Append(' ');
                pageStarts.Add((sb.Length, page.PageNumber));
                sb.Append(t);
            }

            var text = sb.ToString();
            var matches = ArticleStart.Matches(text);
            if (matches.Count < MinArticlesForLegalSplit)
                return null;

            int? PageAt(int offset)
            {
                int? page = null;
                foreach (var (o, p) in pageStarts)
                {
                    if (o > offset) break;
                    page = p;
                }
                return page;
            }

            var result = new List<TextChunk>();
            var index = 0;

            void AddPlain(int from, int to)
            {
                if (to <= from) return;
                var part = text.Substring(from, to - from).Trim();
                if (part.Length < MinChunkLength) return;
                foreach (var piece in SplitBySize(part, ChunkSize))
                    result.Add(new TextChunk(index++, PageAt(from), piece));
            }

            // Naslov člana (npr. "Teška krađa") stoji ispred reči "Član N", pa ga vraćamo članu kome pripada
            var starts = new int[matches.Count];
            for (var i = 0; i < matches.Count; i++)
                starts[i] = TitleStart(text, matches[i].Index);

            // Tekst pre prvog člana (naslovi, preambula)
            AddPlain(0, starts[0]);

            for (var i = 0; i < matches.Count; i++)
            {
                var start = starts[i];
                var end = i + 1 < matches.Count ? starts[i + 1] : text.Length;
                var article = text.Substring(start, end - start).Trim();
                if (article.Length < MinChunkLength) continue;

                var page = PageAt(start);
                if (article.Length <= MaxArticleLength)
                {
                    result.Add(new TextChunk(index++, page, article));
                    continue;
                }

                // Dugačak član: delovi sa oznakom člana na početku
                var label = matches[i].Value.Trim().TrimEnd('.');
                var pieces = SplitBySize(article, ChunkSize);
                for (var k = 0; k < pieces.Count; k++)
                {
                    var piece = k == 0 ? pieces[k] : $"{label} (nastavak): {pieces[k]}";
                    result.Add(new TextChunk(index++, page, piece));
                }
            }

            return result;
        }

        /// <summary>
        /// Ako ispred oznake člana stoji kratak naslov (bez cifara i zagrada), vraća gde taj naslov počinje.
        /// </summary>
        private static int TitleStart(string text, int articleIndex)
        {
            const int maxTitle = 90;
            var from = Math.Max(0, articleIndex - maxTitle);
            var window = text.Substring(from, articleIndex - from);
            var cut = window.LastIndexOfAny(new[] { '.', '!', '?', ':', ';', ')' });
            if (cut < 0)
                return articleIndex;

            var title = window.Substring(cut + 1).Trim();
            if (title.Length < 3 || title.Any(char.IsDigit) || title.Contains('(') || title.Contains(','))
                return articleIndex;

            return from + cut + 1 + (window.Substring(cut + 1).Length - window.Substring(cut + 1).TrimStart().Length);
        }

        private List<TextChunk> ChunkByPages(List<PageText> pages)
        {
            var result = new List<TextChunk>();
            var index = 0;

            foreach (var page in pages)
            {
                var text = Regex.Replace(page.Text ?? string.Empty, @"\s+", " ").Trim();
                if (text.Length == 0)
                    continue;

                var start = 0;
                while (start < text.Length)
                {
                    var end = Math.Min(start + ChunkSize, text.Length);

                    if (end < text.Length)
                    {
                        // Tražimo kraj rečenice u poslednjoj četvrtini dela, pa kraj reči
                        var window = ChunkSize / 4;
                        var boundary = text.LastIndexOfAny(SentenceEnd, end - 1, window);
                        if (boundary < 0)
                            boundary = text.LastIndexOf(' ', end - 1, window);
                        if (boundary >= 0)
                            end = boundary + 1;
                    }

                    var piece = text.Substring(start, end - start).Trim();
                    if (piece.Length >= MinChunkLength)
                        result.Add(new TextChunk(index++, page.PageNumber, piece));

                    if (end >= text.Length)
                        break;

                    // Sledeći deo počinje malo ranije (preklapanje), ali uvek na početku reči
                    var next = Math.Max(end - Overlap, start + 1);
                    var space = text.IndexOf(' ', next);
                    start = (space >= 0 && space < end) ? space + 1 : next;
                }
            }

            return result;
        }

        /// <summary>Deli tekst na delove od najviše size karaktera sa preklapanjem, po rečenici ili reči.</summary>
        private List<string> SplitBySize(string text, int size)
        {
            var pieces = new List<string>();
            if (text.Length <= size)
            {
                pieces.Add(text);
                return pieces;
            }

            var start = 0;
            while (start < text.Length)
            {
                var end = Math.Min(start + size, text.Length);
                if (end < text.Length)
                {
                    var window = size / 4;
                    var boundary = text.LastIndexOfAny(SentenceEnd, end - 1, window);
                    if (boundary < 0)
                        boundary = text.LastIndexOf(' ', end - 1, window);
                    if (boundary >= 0)
                        end = boundary + 1;
                }

                var piece = text.Substring(start, end - start).Trim();
                if (piece.Length >= MinChunkLength)
                    pieces.Add(piece);

                if (end >= text.Length)
                    break;

                var next = Math.Max(end - Overlap, start + 1);
                var space = text.IndexOf(' ', next);
                start = (space >= 0 && space < end) ? space + 1 : next;
            }

            return pieces;
        }
    }
}
