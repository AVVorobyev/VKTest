using Core.Data;
using Core.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Web.Helpers;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _repository;

        public UserController(UserRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAsync([FromBody] User? user)
        {
            if (user == null)
            {
                return BadRequest(user);
            }

            user.Password = Crypto.HashPassword(user.Password);

            var result = await _repository.AddAsync(user);

            return result.Succeeded
                ? Ok()
                : StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage);
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetAsync([FromQuery] int? id)
        {
            if (id == null)
            {
                return BadRequest(id);
            }

            var userResult = await _repository.GetAsync(
                u => u.Id == id,
                $"{nameof(Core.Data.Models.User.UserGroup)},{nameof(Core.Data.Models.User.UserState)}",
                false);

            return userResult.Succeeded
                ? Ok(userResult.Model)
                : StatusCode(StatusCodes.Status500InternalServerError, userResult.ErrorMessage);
        }

        [HttpGet("GetRange")]
        public async Task<IActionResult> GetRangeAsync([FromQuery] int? skip, [FromQuery] int? take)
        {
            if (skip == null)
            {
                skip = 0;
            }

            if (take == null)
            {
                take = 0;
            }

            var userListResult = await _repository.GetRangeAsync(
                null,
                (int)skip,
                (int)take,
                $"{nameof(Core.Data.Models.User.UserGroup)},{nameof(Core.Data.Models.User.UserState)}");

            return userListResult.Succeeded
                ? Ok(userListResult.Model)
                : StatusCode(StatusCodes.Status500InternalServerError, userListResult.ErrorMessage);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromQuery] int? id)
        {
            if (id == null)
            {
                return BadRequest(id);
            }

            var result = await _repository.DeleteAsync((int)id);

            return result.Succeeded
                ? Ok()
                : StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage);
        }
    }
}