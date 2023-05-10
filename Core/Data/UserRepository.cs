using Core.Data.DatabaseContext;
using Core.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Core.Data
{
    public class UserRepository : Repository<User>
    {
        private static readonly ConcurrentDictionary<string, User> _concurrentDictionary = new();
        private readonly PosgreSQLContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly DbSet<User> _dbSetUser;
        private readonly DbSet<UserGroup> _dbSetUserGroup;
        private readonly DbSet<UserState> _dbSetUserState;

        public UserRepository(ILogger<UserRepository>? logger, PosgreSQLContext context)
        {
            _logger = logger ?? NullLogger<UserRepository>.Instance;
            _context = context;
            _dbSetUser = _context.Set<User>();
            _dbSetUserGroup = _context.Set<UserGroup>();
            _dbSetUserState = _context.Set<UserState>();
        }

        /// <summary>
        /// Adds a User to the Database.
        /// Sets CreatedDate and Status code.
        /// It is guaranteed that adding a new user to the database should take 5 seconds
        /// </summary>
        /// <param name="user">User to add to the database</param>
        /// <returns><see cref="{Result}</returns>
        /// 
        /// <exception cref="InvalidOperationException">Throw if tryint to add <see cref="User"/> with <see cref="User.Login"/>
        /// when a <see cref="User"/> with that login already exists in the Database</exception>
        /// 
        /// <exception cref="InvalidOperationException">Throw if tryint to add <see cref="User"/> with <see cref="GroupCode.Admin"/>
        /// when a <see cref="User"/> with that code already exists in the Databas</exception>
        /// 
        /// <exception cref="InvalidOperationException">Throw if tryint to add <see cref="User"/> with <see cref="User.Login"/>
        /// when a <see cref="User"/> with that login is already added to the database</exception>
        public override async Task<Result> AddAsync(User user)
        {
            _logger.LogInformation("Adding the {User} to the Database",
                nameof(User));

            try
            {
                Guard.NotNull(user, nameof(user));
                Guard.NotNull(user.Login, nameof(user.Login));
                Guard.NotNull(user.Password, nameof(user.Password));

                user.CreatedDate = SystemDate.Now();
                user.UserState.Code = StatusCode.Active;

                if (user.UserGroup.Code == GroupCode.Admin &&
                    await _dbSetUserGroup.AnyAsync(g => g.Code == GroupCode.Admin))
                {
                    throw new InvalidOperationException(
                        $"{nameof(User)} with {nameof(GroupCode)} {nameof(GroupCode.Admin)} already exist in the Database");
                }

                if (_concurrentDictionary.TryAdd(user.Login, user))
                {
                    try
                    {
                        // Adding a new user to the database should take 5 seconds
                        Thread.Sleep(5000);

                        if (_dbSetUser.Any(u => u.Login == user.Login))
                        {
                            throw new InvalidOperationException(
                                $"{nameof(User)} with same {nameof(User.Login)} already exist in Database");
                        }

                        await _dbSetUser.AddAsync(user);

                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        _concurrentDictionary.Remove(user.Login, out _);
                    }

                    return Result.Success();
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Adding a {nameof(User)} with the same {nameof(User.Login)} is already in process");
                }
            }
            catch (Exception e)
            {
                return Result.Fail(e, e.Message);
            }
        }

        /// <summary>
        /// Gets the user by the following filter from the database
        /// </summary>
        /// <param name="includeProperties">Gets property names separated by commas.
        /// see cref="EntityFrameworkQueryableExtensions.
        /// Include{TEntity, TProperty}(IQueryable{TEntity}, Expression{Func{TEntity, TProperty}})"/></param>
        /// <param name="asNoTracking"><see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}(IQueryable{TEntity})"/></param>
        /// <returns><see cref="{Result}</returns>
        public override async Task<Result<User>> GetAsync(
            Expression<Func<User, bool>> filter,
            string? includeProperties = null,
            bool asNoTracking = false)
        {
            Guard.NotNull(filter, nameof(filter));

            _logger.LogInformation("Getting the {User} from the Database",
                nameof(User));

            try
            {
                IQueryable<User> query = _dbSetUser;

                if (asNoTracking)
                {
                    query = query.AsNoTracking();
                }

                if (includeProperties != null)
                {
                    foreach (var property in includeProperties.Split(
                        ',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(property);
                    }
                }

                var user = await query.FirstOrDefaultAsync(filter);

                return Result<User>.Success(user);
            }
            catch (Exception e)
            {
                return Result<User>.Fail(e, e.Message);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="User"/> by the following filter from the Database
        /// </summary>
        /// <param name="skip">Quantity of skipped <see cref="User"/></param>
        /// <param name="take">Quantity of the taken <see cref="User"/></param>
        /// <param name="includeProperties">Gets property names separated by commas.
        /// <see cref="EntityFrameworkQueryableExtensions.
        /// Include{TEntity, TProperty}(IQueryable{TEntity}, Expression{Func{TEntity, TProperty}})"/></param>
        /// <returns><see cref="{Result}</returns>
        public override async Task<Result<IEnumerable<User>>> GetRangeAsync(
            Expression<Func<User, bool>>? filter = null,
            int skip = 0,
            int take = 10,
            string? includeProperties = null)
        {
            _logger.LogInformation("Getting the {User} list from the Database",
                nameof(User));

            try
            {
                IQueryable<User> query = _dbSetUser;

                if (includeProperties != null)
                {
                    foreach (var property in includeProperties.Split(
                        ',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(property);
                    }
                }

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                var user = await query
                    .Skip(skip)
                    .Take(take)
                    .ToArrayAsync();

                return Result<IEnumerable<User>>.Success(user);
            }
            catch (Exception e)
            {
                return Result<IEnumerable<User>>.Fail(e, e.Message);
            }
        }

        /// <summary>
        /// Changes the <see cref="User" <see cref="StatusCode.Active"/> to the <see cref="StatusCode.Blocked"/>
        /// in the Database/>
        /// </summary>
        /// <param name="id"><see cref="User.Id"/></param>
        /// <returns><see cref="{Result}</returns>
        /// <exception cref="InvalidOperationException">Throwing if the <see cref="User"/>
        /// with following Id not found in the Database</exception>
        public override async Task<Result> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting the {User} from the Database.",
                nameof(User));

            try
            {
                var userState = await _dbSetUserState
                    .AsNoTracking()
                    .Include(nameof(User))
                    .FirstOrDefaultAsync(us =>
                        us.UserId == id);

                if (userState == null)
                {
                    throw new InvalidOperationException(
                        $"Not found");
                }

                userState.Code = StatusCode.Blocked;

                _dbSetUserState.Update(userState);

                await _context.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Fail(e, e.Message);
            }
        }
    }
}
