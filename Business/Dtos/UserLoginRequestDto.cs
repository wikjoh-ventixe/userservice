namespace Business.Dtos;

public class UserLoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
