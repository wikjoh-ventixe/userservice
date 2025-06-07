using Business.Dtos;
using Business.Models;

namespace Business.Interfaces;

public interface IUserService
{
    Task<UserResult<User?>> CreateUserAsync(CreateUserRequestDto request);
    Task<UserResult<User?>> CreateUserWithPasswordAsync(CreateUserRequestDto request, string password);
}
