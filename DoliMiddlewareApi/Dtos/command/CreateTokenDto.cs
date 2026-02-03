using System.ComponentModel.DataAnnotations;

namespace DoliMiddlewareApi.Dtos.command;

public class CreateTokenDto
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = "";
    [Required]
    [StringLength(255)]
    public string Password { get; set; } = "";



}