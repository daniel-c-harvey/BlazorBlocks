using System.ComponentModel.DataAnnotations;

namespace API.Shared.Common.Email.Mailtrap.Models;

public class Address
{
    /// <summary>
    /// Email address.
    /// </summary>
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// Name associated with the email address.
    /// </summary>
    public string Name { get; set; }
}
