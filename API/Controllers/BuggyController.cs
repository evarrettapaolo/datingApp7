using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class BuggyController : BaseApiController
  {
    private readonly DataContext _context;
    public BuggyController(DataContext dataContext)
    {
      _context = dataContext;
    }

    //401
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
      return "return text";
    }

    //404
    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
      var thing = _context.Users.Find(-1);

      if (thing == null) return NotFound();

      return thing;
    }

    //500
    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
      var thing = _context.Users.Find(-1);

      var thingToReturn = thing.ToString(); //generate an exception
      
      return thingToReturn;
    }

    //400
    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
      return BadRequest("This was not a good request");
    }

  }
}