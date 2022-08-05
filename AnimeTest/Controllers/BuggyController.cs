using System;
using Microsoft.AspNetCore.Mvc;

namespace AnimeBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuggyController : BaseController
    {
        [HttpGet("bad-request")]
        public ActionResult GetBadResult()
        {
            return BadRequest();
        }

        [HttpGet("not-found")]
        public ActionResult GetNotFound()
        {
            return NotFound();
        }
        [HttpGet("unauthorized")]
        public ActionResult GetUnauthorized()
        {
            return Unauthorized();
        }
        [HttpGet("validation-error")]
        public ActionResult GetValidationError()
        {
            ModelState.AddModelError("Problem 1", "Incorrect model 1");
            ModelState.AddModelError("Problem 2", "Incorrect model 2");
            return ValidationProblem();
        }


        [HttpGet("server-error")]
        public ActionResult GetException()
        {
            throw new Exception("This is a server error");
        }
    }
}