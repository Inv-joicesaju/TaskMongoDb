using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TaskMongoDb.Data;
using TaskMongoDb.Models;


namespace TaskMongoDb.Controllers
{

    [Route("api/user")]
    [ApiController]
    public class UserAuthController : Controller
    {
        private readonly MongoDbContext _dbContext;

        //private UserManager<ApplicationUser> _userManager;
        public UserAuthController(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
            //_userManager = userManager;
        }

        [HttpPost("insertData")]
        public IActionResult InsertTaskData()
        {
            TaskModel tasgks = new TaskModel { Title = "Connect to MongoDB"};
            _dbContext.Tasks.InsertOne(tasgks);
            return new OkObjectResult("success");
        }
    }
}
