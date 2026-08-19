namespace ArtemisBankingPro.Application.DTOs.Commerce;

public class CreateCommerceDto
{
    public string Name { get; set; } = string.Empty;
    public string Rnc { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateCommerceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}