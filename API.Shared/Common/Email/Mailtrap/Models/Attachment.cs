using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Shared.Common.Email.Mailtrap.Models;

public class Attachment
{
    /// <summary>
    /// The Base64 encoded content of the attachment.
    /// </summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// The MIME type of the content.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The attachment's filename.
    /// </summary>
    [Required]
    public string Filename { get; set; }

    /// <summary>
    /// How the attachment should be displayed.
    /// Options: "inline" or "attachment".
    /// Default: "attachment"
    /// </summary>
    public string Disposition { get; set; } = "attachment";

    /// <summary>
    /// The attachment's content ID. Used when disposition is "inline".
    /// </summary>
    [JsonPropertyName("content_id")]
    public string ContentId { get; set; }
}
