using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Runtime;
using Xunit;

namespace SefimMcp.Tests;

public sealed class KnowledgeSearchTests
{
    private static KnowledgeDocument Document(string kind, string id, string title, string body, string? summary = null, string[]? aliases = null) =>
        new(kind, id, "published", "model", title, body, new Dictionary<string, string>(), $"{id}.md", summary, aliases);

    private static KnowledgeIndex BuildIndex() => KnowledgeIndex.Build(
    [
        Document(
            "table",
            "dbo.Product",
            "Ürün Tablosu",
            "# Ürün Tablosu\n\n## Purpose\n\nŞefim'de satılan ürünlerin ana kaydı.\n\n## Write Guidance\n\nÜrün fiyatı değiştirilirken fiyat listesi kontrol edilmelidir.\n",
            "Satışı yapılan ürünlerin ana kaydı.",
            ["urun", "stok kart"]),
        Document(
            "workflow",
            "close-adisyon",
            "Adisyon Kapatma",
            "# Adisyon Kapatma\n\n## Purpose\n\nAçık adisyonun tahsilat sonrası kapatılması.\n\n## Steps\n\nÖdeme alınır ve masa boşaltılır.\n",
            "Açık adisyonun kapatılma akışı."),
        Document(
            "glossary",
            "sefim-glossary",
            "Sözlük",
            "# Sözlük\n\n## Terms\n\n- **Kuver:** Masa başına alınan sabit servis ücreti.\n- **Zayi:** Satılamadan bozulan veya atılan ürün.\n")
    ]);

    [Theory]
    [InlineData("ürün fiyatı")]
    [InlineData("urun fiyati")]
    [InlineData("ÜRÜN FİYAT")]
    [InlineData("ürünün fiyatları")]
    public void Turkish_writing_variants_reach_the_same_document(string query)
    {
        var hits = BuildIndex().Search(query, 5);
        Assert.Contains(hits, hit => hit.Id == "dbo.Product");
    }

    [Fact]
    public void Search_returns_the_section_not_the_whole_document()
    {
        var hit = Assert.Single(BuildIndex().Search("fiyat listesi kontrol", 3), hit => hit.Id == "dbo.Product");
        Assert.Equal("Write Guidance", hit.Section);
        Assert.StartsWith("sefim://knowledge/table/dbo.Product#", hit.Uri);
        Assert.DoesNotContain("Purpose", hit.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Glossary_terms_become_their_own_sections()
    {
        var sections = KnowledgeChunker.Split(Document(
            "glossary",
            "g",
            "Sözlük",
            "# Sözlük\n\n## Terms\n\n- **Kuver:** Masa başına alınan ücret.\n- **Zayi:** Atılan ürün.\n"));

        Assert.Contains(sections, section => section.Heading == "Kuver");
        Assert.Contains(sections, section => section.Heading == "Zayi");
    }

    [Fact]
    public void Aliases_make_a_document_reachable_by_a_word_the_body_never_uses()
    {
        var hits = BuildIndex().Search("stok kart", 5);
        Assert.Contains(hits, hit => hit.Id == "dbo.Product");
    }

    [Fact]
    public void Unrelated_query_scores_nothing()
    {
        Assert.Empty(BuildIndex().Search("kubernetes deployment", 5));
    }

    [Theory]
    [InlineData("Ürün", "urun")]
    [InlineData("İŞLEM", "islem")]
    [InlineData("ÇĞÖŞÜ", "cgosu")]
    [InlineData("Adisyon'un", "adisyon'un")]
    public void Folding_maps_turkish_case_pairs_to_ascii(string input, string expected) =>
        Assert.Equal(expected, KnowledgeText.Fold(input));

    [Fact]
    public void Prefix_matching_stops_below_the_minimum_length()
    {
        Assert.True(KnowledgeText.IsPrefixMatch("urunun", "urun"));
        Assert.False(KnowledgeText.IsPrefixMatch("uzak", "uru"));
    }
}
