using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      var resultContext = await next(); //control after endpoint execution
      //check authentication
      if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;
      //get current username
      var userId = resultContext.HttpContext.User.GetUserId();
      //get userRepo service
      var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
      //get user object
      var user = await repo.GetUserByIdAsync(int.Parse(userId));
      //update property
      user.LastActive = DateTime.UtcNow;
      //save
      await repo.SaveAllAsync();
    }
  }
}