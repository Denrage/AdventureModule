using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Denrage.AdventureModule.Server.AspNet.Controllers.Admin;
[ApiController]
[Route("admin/[controller]/[action]")]
public class ParticipantsController : ControllerBase
{
    public ParticipantsController()
    {
    }

    public IActionResult All()
    {
        return this.Ok("Hello World");
    }
}
