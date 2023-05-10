using Core.Data;
using Core.Data.DatabaseContext;
using Core.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace VKTest.Core.Tests
{
    public class UserRepositoryTests
    {
        private readonly UserRepository _repository;
        private readonly PosgreSQLContext _context;

        public UserRepositoryTests()
        {
            var _dbContextOptions = new DbContextOptionsBuilder<PosgreSQLContext>()
                .EnableSensitiveDataLogging()
                .UseInMemoryDatabase("UserREpositoryTestDatabase" + DateTime.Now)
                .ConfigureWarnings(b =>
                    b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new PosgreSQLContext(_dbContextOptions);

            _repository = new UserRepository(null, _context);
        }

        [Fact]
        public async Task AddAsync_Test_TryAddUser_Success()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var now = DateTime.Now;

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.User,
                Description = "description"
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Blocked,
                Description = "description"
            };

            var userToAdd = new User("login", "password", userGroupToAdd, userStateToAdd)
            {
                CreatedDate = now
            };

            var result = await _repository.AddAsync(userToAdd);

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(1, await _context.User.CountAsync());

            var addedUser = await _context.User.SingleAsync();

            Assert.Equal(userToAdd.Login, addedUser.Login);
            Assert.Equal(userToAdd.Password, addedUser.Password);
            Assert.Equal(userToAdd.CreatedDate, addedUser.CreatedDate);

            Assert.Equal(GroupCode.User, addedUser.UserGroup.Code);
            Assert.Equal(userGroupToAdd.Description, addedUser.UserGroup.Description);

            Assert.Equal(StatusCode.Active, addedUser.UserState.Code);
            Assert.Equal(userStateToAdd.Description, addedUser.UserState.Description);
        }

        [Fact]
        public async Task AddAsync_Test_TryAddSecondUserWithGroupCodeAdmin_Fail()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.Admin,
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Active
            };

            var userToAdd = new User("login", "password", userGroupToAdd, userStateToAdd);

            await _context.User.AddAsync(userToAdd);

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var secondAdminToAdd = new User("login2", "password", userGroupToAdd, userStateToAdd);

            var result = await _repository.AddAsync(secondAdminToAdd);

            Assert.NotNull(result);
            Assert.False(result.Succeeded);

            Assert.Equal(1, await _context.User.CountAsync());
        }

        [Fact]
        public async Task AddAsync_Test_TrylAddSecondUserWithSameLoginInParallel_Fail()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.User,
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Active
            };

            var userToAdd = new User("login", "password", userGroupToAdd, userStateToAdd);

            _ = _repository.AddAsync(userToAdd);

            _context.ChangeTracker.Clear();

            var secondAdminToAdd = new User("login", "password", userGroupToAdd, userStateToAdd);

            var result = await _repository.AddAsync(secondAdminToAdd);

            Assert.NotNull(result);
            Assert.False(result.Succeeded);

            Assert.Equal(1, await _context.User.CountAsync());
        }

        [Fact]
        public async Task AddAsync_Test_TryAddSecondUserWithSameLoginAfter5secDelay_Fail()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.User,
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Active
            };

            var userToAdd = new User("login", "password", userGroupToAdd, userStateToAdd);

            await _repository.AddAsync(userToAdd);

            _context.ChangeTracker.Clear();

            var secondAdminToAdd = new User("login", "password", userGroupToAdd, userStateToAdd);

            var result = await _repository.AddAsync(secondAdminToAdd);

            Assert.NotNull(result);
            Assert.False(result.Succeeded);

            Assert.Equal(1, await _context.User.CountAsync());
        }

        [Fact]
        public async Task AddAsync_Test_TryAddNullUser_Fail()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var result = await _repository.AddAsync(null!);

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.False(await _context.User.AnyAsync());
        }

        [Fact]
        public async Task DeleteAsync_Test_Success()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.User
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Active
            };

            var userToAdd = new User("login", "password", userGroupToAdd, userStateToAdd);

            await _context.User.AddAsync(userToAdd);

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var id = _context.User.SingleAsync().Result.Id;
            _context.ChangeTracker.Clear();

            var result = await _repository.DeleteAsync(id);

            Assert.NotNull(result);
            Assert.True(result.Succeeded);

            Assert.Equal(1, await _context.User.CountAsync());

            var userAfterDeletion = await _context.User.SingleAsync();

            Assert.NotNull(userAfterDeletion);
            Assert.Equal(StatusCode.Blocked, userAfterDeletion.UserState.Code);
        }

        [Fact]
        public async Task GetAsync_Test_Success()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.User
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Active
            };

            var now = DateTime.Now;

            var userToAdd = new User("login", "password", userGroupToAdd, userStateToAdd)
            {
                CreatedDate = now
            };

            await _context.User.AddAsync(userToAdd);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetAsync(
                filter: u => u.Login == "login",
                includeProperties: $"{nameof(UserGroup)},{nameof(User.UserState)}");

            Assert.NotNull(result);
            Assert.True(result.Succeeded);

            var userFromDb = result.Model;

            Assert.NotNull(userFromDb);

            Assert.Equal(userToAdd.Login, userFromDb!.Login);
            Assert.Equal(userToAdd.Password, userFromDb.Password);
            Assert.Equal(userToAdd.CreatedDate, userFromDb.CreatedDate);

            Assert.Equal(userGroupToAdd.Code, userFromDb.UserGroup.Code);
            Assert.Equal(userGroupToAdd.Description, userFromDb.UserGroup.Description);

            Assert.Equal(userGroupToAdd.Description, userFromDb.UserGroup.Description);
            Assert.Equal(userGroupToAdd.Description, userFromDb.UserGroup.Description);
        }

        [Fact]
        public async Task GetAsync_Test_UserIsNull_Success()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var result = await _repository.GetAsync(u => u.Login == "login");

            Assert.NotNull(result);
            Assert.True(result.Succeeded);

            var userFromDb = result.Model;

            Assert.Null(userFromDb);
        }

        [Fact]
        public async Task GetRangeAsync_Test_Success()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userGroupToAdd = new UserGroup()
            {
                Code = GroupCode.User
            };

            var userStateToAdd = new UserState()
            {
                Code = StatusCode.Active
            };

            var now = DateTime.Now;

            List<User> userListToAdd = new()
            {
                new User("login1", "password", userGroupToAdd, userStateToAdd),
                new User("login2", "password", userGroupToAdd, userStateToAdd),
                new User("login3", "password", userGroupToAdd, userStateToAdd)
            };

            await _context.User.AddRangeAsync(userListToAdd);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetRangeAsync(
                includeProperties: $"{nameof(UserGroup)},{nameof(User.UserState)}");

            Assert.NotNull(result);
            Assert.True(result.Succeeded);

            var userFromDb = result.Model;

            Assert.NotNull(userFromDb);
            Assert.Equal(3, new List<User>(userFromDb!).Count);
        }

        [Fact]
        public async Task GetRangeAsync_Test_GetFromDatabaseWithZeroRows_Success()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var result = await _repository.GetRangeAsync();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
        }
    }
}