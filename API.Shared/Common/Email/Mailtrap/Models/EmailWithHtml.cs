using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Shared.Common.Email.Mailtrap.Models
{
    public class EmailWithHtml
    {
        /// <summary>
        /// The sender of the email.
        /// </summary>
        [Required]
        public Address From { get; set; }

        /// <summary>
        /// Recipients who will receive the email. Max 1000 items.
        /// </summary>
        [Required]
        public List<Address> To { get; set; }

        /// <summary>
        /// Recipients who will receive a carbon copy of the email. Max 1000 items.
        /// </summary>
        public List<Address> Cc { get; set; }

        /// <summary>
        /// Recipients who will receive a blind carbon copy of the email. Max 1000 items.
        /// </summary>
        public List<Address> Bcc { get; set; }

        /// <summary>
        /// The email address to which recipients will be able to reply.
        /// </summary>
        [JsonPropertyName("reply_to")]
        public Address ReplyTo { get; set; }

        /// <summary>
        /// Array of attachments to include with the email.
        /// </summary>
        public List<Attachment> Attachments { get; set; }

        /// <summary>
        /// Custom headers to include with the email.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Custom variables specific to the email.
        /// Total size in JSON form must not exceed 1000 bytes.
        /// </summary>
        [JsonPropertyName("custom_variables")]
        public Dictionary<string, string> CustomVariables { get; set; }

        /// <summary>
        /// The subject of the email.
        /// </summary>
        [Required]
        public string Subject { get; set; }

        /// <summary>
        /// Text version of the email body.
        /// Required if 'Html' is not provided.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// HTML version of the email body.
        /// Required in this model.
        /// </summary>
        [Required]
        public string Html { get; set; }

        /// <summary>
        /// Category for the email. Max 255 characters.
        /// </summary>
        public string Category { get; set; }
    }
}