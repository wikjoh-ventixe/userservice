using Data.Context;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;

public class UserRepository(UserDbContext context) : BaseRepository<UserEntity>(context), IUserRepository
{

}