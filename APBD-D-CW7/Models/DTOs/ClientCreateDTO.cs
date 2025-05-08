using System.ComponentModel.DataAnnotations;

namespace APBD_D_CW7.Models.DTOs;

public class ClientCreateDTO
{
    public int Id { get; set; }
    [Length(3,30)]
    public required string FirstName { get; set; }
    [Length(3,30)]
    public required string LastName { get; set; }
    [RegularExpression(@"^.*@.*$", ErrorMessage = "Adres musi zawierać znak @")]
    public required string Email { get; set; }
    [Length(9,9)]
    public required string Telephone { get; set; }
    [Length(11,11)][RegularExpression(@"^\d+$")]
    public required string Pesel { get; set; }
}