namespace IS_AdvokatskeKancelarije.Services
{
    /// <summary>Tekst jedne strane (PageNumber je null za formate bez strana, npr. DOCX i TXT).</summary>
    public record PageText(int? PageNumber, string Text);

    /// <summary>Jedan deo teksta spreman za embedding.</summary>
    public record TextChunk(int Index, int? PageNumber, string Text);
}
