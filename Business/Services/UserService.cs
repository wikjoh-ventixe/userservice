using Business.Dtos;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Protos;
using System.Reflection;

namespace Business.Services;

public class UserService(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager, IUserRepository userRepository, GrpcUserProfile.GrpcUserProfileClient grpcUserProfileClient, SignInManager<UserEntity> signInManager) : IUserService
{
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly GrpcUserProfile.GrpcUserProfileClient _grpcUserProfileClient = grpcUserProfileClient;


    // CREATE
    public async Task<UserResult<User?>> CreateUserAsync(CreateUserRequestDto request)
    {
        if (request == null)
            return UserResult<User?>.BadRequest("Request cannot be null.");

        if (await _userManager.Users.AnyAsync(u => u.Email == request.Email))
            return UserResult<User?>.AlreadyExists("User with given email adress already exists.");

        var userEntity = new UserEntity
        {
            Email = request.Email,
            UserName = request.Email,
        };

        try
        {
            await _userRepository.BeginTransactionAsync();

            var createUserResult = await _userManager.CreateAsync(userEntity);
            if (!createUserResult.Succeeded)
            {
                await _userRepository.RollbackTransactionAsync();
                return UserResult<User?>.InternalServerError($"Failed creating user with email {userEntity.Email}. Rolling back.");
            }

            var createdUserEntity = _userManager.Users.FirstOrDefault(x => x.Id == userEntity.Id);
            if (createdUserEntity == null)
            {
                await _userRepository.RollbackTransactionAsync();
                return UserResult<User?>.InternalServerError($"Failed retrieving user entity after creation. Rolling back.");
            }

            // Add user profile via gRPC. If adding user profile fails then rollback
            var grpcRequest = new CreateUserProfileRequest
            {
                UserId = createdUserEntity.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                StreetAddress = request.StreetAddress,
                PostalCode = request.PostalCode,
                City = request.City,
            };

            var grpcResponse = await _grpcUserProfileClient.CreateUserProfileAsync(grpcRequest);
            if (!grpcResponse.Succeeded)
            {
                await _userRepository.RollbackTransactionAsync();
                return UserResult<User?>.InternalServerError($"Failed creating user profile. Rolling back.");
            }

            await _userRepository.CommitTransactionAsync();
            var createdUser = new User
            {
                Id = createdUserEntity.Id,
                Email = createdUserEntity.Email!,
                Created = createdUserEntity.Created
            };
            return UserResult<User?>.Created(createdUser);
        }
        catch (Exception ex)
        {
            await _userRepository.RollbackTransactionAsync();
            return UserResult<User?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }

    public async Task<UserResult<User?>> CreateUserWithPasswordAsync(CreateUserRequestDto request, string password)
    {
        if (request == null)
            return UserResult<User?>.BadRequest("Request cannot be null.");

        if (await _userManager.Users.AnyAsync(u => u.Email == request.Email))
            return UserResult<User?>.AlreadyExists("User with given email adress already exists.");

        var userEntity = new UserEntity
        {
            Email = request.Email,
            UserName = request.Email,
        };

        try
        {
            await _userRepository.BeginTransactionAsync();

            var createUserResult = await _userManager.CreateAsync(userEntity, password);
            if (!createUserResult.Succeeded)
            {
                await _userRepository.RollbackTransactionAsync();
                return UserResult<User?>.InternalServerError($"Failed creating user with email {userEntity.Email}. Rolling back.");
            }

            var createdUserEntity = _userManager.Users.FirstOrDefault(x => x.Id == userEntity.Id);
            if (createdUserEntity == null)
            {
                await _userRepository.RollbackTransactionAsync();
                return UserResult<User?>.InternalServerError($"Failed retrieving user entity after creation. Rolling back.");
            }

            // Add user profile via gRPC. If adding user profile fails then rollback
            var grpcRequest = new CreateUserProfileRequest
            {
                UserId = createdUserEntity.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                StreetAddress = request.StreetAddress,
                PostalCode = request.PostalCode,
                City = request.City,
            };

            var grpcResponse = await _grpcUserProfileClient.CreateUserProfileAsync(grpcRequest);
            if (!grpcResponse.Succeeded)
            {
                await _userRepository.RollbackTransactionAsync();
                return UserResult<User?>.InternalServerError($"Failed creating user profile. Rolling back.");
            }

            await _userRepository.CommitTransactionAsync();
            var createdUser = new User
            {
                Id = createdUserEntity.Id,
                Email = createdUserEntity.Email!,
                Created = createdUserEntity.Created
            };
            return UserResult<User?>.Created(createdUser);
        }
        catch (Exception ex)
        {
            await _userRepository.RollbackTransactionAsync();
            return UserResult<User?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }


    // READ
    public async Task<UserResult<AuthData>> LoginUserAsync(UserLoginRequestDto request)
    {
        if (request == null)
            return UserResult<AuthData>.BadRequest("Request cannot be null.");

        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            return UserResult<AuthData>.Ok(new AuthData
            {
                UserId = user!.Id,
                EmailConfirmed = user.EmailConfirmed,
            });
        }

        return UserResult<AuthData>.Unauthorized("User authentication failed.");
    }
}
