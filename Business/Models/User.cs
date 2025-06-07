namespace Business.Models;

public class User
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime Created { get; set; }
}
