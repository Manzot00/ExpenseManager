using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Client = Supabase.Client;
using Supabase.Postgrest;
using ExpensesManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExpensesManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Client _supabaseClient;

        public CategoryController(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }
        [HttpPost("createUserCategory")]
        public async Task<ActionResult<UserCategory>> CreateUserCategory(UserCategoryDto userCategoryDto)
        {
            try
            {
                var userCategory = new UserCategory
                {
                    Name = userCategoryDto.Name,
                    Type = userCategoryDto.Type,
                    UserId = userCategoryDto.UserId
                };
                // Insert the user-specific category into the database
                var response = await _supabaseClient.From<UserCategory>().Insert(userCategory);
                var createdCategory = response.Models[0];

                return Ok(new
                {
                    Id = createdCategory.Id.ToString(),
                    Name = createdCategory.Name,
                    Type = createdCategory.Type,
                    UserId = createdCategory.UserId.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("createGlobalCategory")]
        public async Task<ActionResult<GlobalCategory>> CreateGlobalCategory(GlobalCategoryDto globalCategoryDto)
		{
			try
			{
				var globalCategory = new GlobalCategory
				{
					Name = globalCategoryDto.Name,
					Type = globalCategoryDto.Type
				};
				// Insert the global category into the database
				var response = await _supabaseClient.From<GlobalCategory>().Insert(globalCategory);
				var createdCategory = response.Models[0];

				return Ok(new
				{
					Id = createdCategory.Id.ToString(),
					Name = createdCategory.Name,
					Type = createdCategory.Type
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

        [HttpGet("getAllGlobalCategories")]
        public async Task<ActionResult<IEnumerable<GlobalCategoryResponse>>> GetGlobalCategories()
        {
            try
            {
                var response = await _supabaseClient
					.From<GlobalCategory>()
					.Get();

                var categories = response.Models.Select(category => new GlobalCategoryResponse
                {
                    Id = category.Id.ToString(),
                    Name = category.Name,
                    Type = category.Type
                }).ToList();

                return Ok(categories);
            }
            catch(Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
        }


		[HttpGet("getAllUserCategories/{userId}")]
        public async Task<ActionResult<IEnumerable<UserCategoryResponse>>> GetCategories(string userId)
        {
            try
            {
                var response = await _supabaseClient
                    .From<UserCategory>()
                    .Filter("user_id", Constants.Operator.Equals, userId.ToString())
                    .Get();

                var categories = response.Models.Select(category => new UserCategoryResponse
                {
                    Id = category.Id.ToString(),
                    Name = category.Name,
                    Type = category.Type,
                    UserId = category.UserId.ToString()
                }).ToList();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getUserCategory/{id}")]
        public async Task<ActionResult<UserCategoryDto>> GetCategory(string id)
        {
            try 
            {
                var response = await _supabaseClient
                    .From<UserCategory>()
                    .Filter("id", Constants.Operator.Equals, id)
                    .Get();

                if (response.Models.Count == 0)
                {
                    return NotFound(new { message = "Category not found" });
                }

                var category = response.Models[0];

                return Ok(new UserCategoryDto
                {
                    Name = category.Name,
                    Type = category.Type,
                    UserId = category.UserId.ToString()
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getGlobalCategory/{id}")]
        public async Task<ActionResult<GlobalCategoryDto>> GetGlobalCategory(string id)
        {
            try
            {
                var response = await _supabaseClient
					.From<GlobalCategory>()
					.Filter("id", Constants.Operator.Equals, id)
					.Get();

                if (response.Models.Count == 0)
				{
					return NotFound(new { message = "Category not found" });
				}

				var category = response.Models[0];

				return Ok(new GlobalCategoryDto
				{
					Name = category.Name,
					Type = category.Type
				});

			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
            }
        }


		[HttpPut("editUserCategory/{id}")]
        public async Task<ActionResult<UserCategory>> EditCategory(UserCategoryDto userCategoryDto, string id)
        {
            try
            {
                var category = new UserCategory
                {
                    Id = id,
                    Name = userCategoryDto.Name,
                    Type = userCategoryDto.Type,
                    UserId = userCategoryDto.UserId.ToString()
                };

                // Salva le modifiche nel database
                var response = await _supabaseClient
                    .From<UserCategory>()
                    .Update(category);

                var updatedCategory = response.Models[0];

                return Ok(new
                {
                    Id = updatedCategory.Id.ToString(),
                    Name = updatedCategory.Name,
                    Type = updatedCategory.Type,
                    UserId = updatedCategory.UserId.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("editGlobalCategory/{id}")]
        public async Task<ActionResult<GlobalCategory>> EditGlobalCategory(GlobalCategoryDto globalCategoryDto, string id)
		{
			try
			{
				var category = new GlobalCategory
				{
					Id = id,
					Name = globalCategoryDto.Name,
					Type = globalCategoryDto.Type
				};

				// Save the changes to the database
				var response = await _supabaseClient
					.From<GlobalCategory>()
					.Update(category);

				var updatedCategory = response.Models[0];

				return Ok(new
				{
					Id = updatedCategory.Id.ToString(),
					Name = updatedCategory.Name,
					Type = updatedCategory.Type
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

        [HttpDelete("deleteUserCategory/{id}")]
        public async Task<ActionResult> DeleteCategory(string id)
        {
            try
            {
                await _supabaseClient
                    .From<UserCategory>()
                    .Filter("id", Constants.Operator.Equals, id)
                    .Delete();

                return Ok(new { message = "Category (" + id + ") deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("deleteGlobalCategory/{id}")]
        public async Task<ActionResult> DeleteGlobalCategory(string id)
		{
			try
			{
				await _supabaseClient
					.From<GlobalCategory>()
					.Filter("id", Constants.Operator.Equals, id)
					.Delete();

				return Ok(new { message = "Category (" + id + ") deleted" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

        //utilizzata per ottenere le categorie utente quando l'admin gestisce le categorie globali
        [HttpGet("getUserCategoryByNameType/{type}/{name}")]
        public async Task<ActionResult<IEnumerable<UserCategoryResponse>>> GetUserCategoryId(string type, string name)
		{
			try
			{
				var categories = await _supabaseClient
					.From<UserCategory>()
					.Filter("type", Constants.Operator.Equals, type)
					.Filter("name", Constants.Operator.Equals, name)
					.Get();

				if (categories.Models.Count == 0)
				{
					return NotFound(new { message = "Category not found" });
				}

                return Ok(categories.Models.Select(category => new UserCategoryResponse
				{
					Id = category.Id.ToString(),
					Name = category.Name,
					Type = category.Type,
					UserId = category.UserId.ToString()
				}).ToList());
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}


    }

    public class UserCategoryDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
    }

    public class UserCategoryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
    }

    public class GlobalCategoryResponse
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
	}

    public class GlobalCategoryDto
	{
		public string Name { get; set; }
		public string Type { get; set; }
	}

}
