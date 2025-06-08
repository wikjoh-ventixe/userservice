using Business.Dtos;
using Business.Interfaces;
using Grpc.Core;
using Grpc.UserAuth;
using System.Diagnostics;

namespace GrpcServer.Services;

public class GrpcUserAuthService(IUserService UserService) : GrpcUserAuth.GrpcUserAuthBase
{
    private readonly IUserService _UserService = UserService;

    public override async Task<UserLoginResponse> LoginUser(UserLoginRequest request, ServerCallContext context)
    {
        Debug.WriteLine($"Attempting to login User");

        var result = await _UserService.LoginUserAsync(new UserLoginRequestDto
        {
            Email = request.Email,
            Password = request.Password,
        });

        var response = new UserLoginResponse
        {
            Succeeded = result.Succeeded,
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
        };
        if (result.Data != null)
        {
            response.AuthInfo = new AuthInfo
            {
                UserId = result.Data.UserId,
                EmailConfirmed = result.Data.EmailConfirmed
            };
        }

        return response;
    }
}
