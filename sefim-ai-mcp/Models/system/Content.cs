namespace SefimMcp.Models.system;

public class Content
{
    public record McpToolResponse(
        [property: JsonPropertyName("content")] McpContent[] Content
    );

    public record McpContent(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text = null,
        [property: JsonPropertyName("data")] [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Data = null,
        [property: JsonPropertyName("mimeType")] [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MimeType = null
    );
}