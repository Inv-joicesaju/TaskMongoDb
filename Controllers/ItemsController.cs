using Microsoft.AspNetCore.Mvc;
using TaskMongoDb.Models;
using TaskMongoDb.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Amazon.Runtime.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TaskMongoDb.Controllers
{
    [Route("api/items")]
    [ApiController]
    public class ItemsController : Controller
    {
        private readonly MongoDbContext _dbContext;
        public ItemsController(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        /// <summary>
        /// add items
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddItem(Item item)
        {
            var userId = GetUserId(Request);//get token getting in GetUserId function
            item.OwnerId = userId;  //take user id from token
            try
            {
                var filter = Builders<Item>.Filter.And(
                Builders<Item>.Filter.Eq(item => item.OwnerId, userId),
                Builders<Item>.Filter.Eq(item => item.ItemName, item.ItemName));

                var items = await _dbContext.Items.Find(filter).ToListAsync();
                if(items.Count>0) //check that if the item is already added or not by this user
                {
                    throw new Exception("item already added");
                }
          
                await _dbContext.Items.InsertOneAsync(item);
                return Ok(item);
            }
            catch (Exception ex)
            { 
               return BadRequest(ex.Message);
            }
        }





        /// <summary>
        /// get all items by userid
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> GetItems([FromQuery] ItemQueryModel itemQueryModel)
        {
            var userId = GetUserId(Request);//get token getting in GetUserId function
            var filter = Builders<Item>.Filter.Eq(item => item.OwnerId, userId);
            try
            {
                var items = await _dbContext.Items.Find(filter)
                                .Skip((itemQueryModel.PageNUmber - 1) * itemQueryModel.DataPerPage)
                                .Limit(itemQueryModel.DataPerPage)
                                .SortBy(item => item.ItemPrice)
                                .ToListAsync();

                return new OkObjectResult(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        /// <summary>
        /// update item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdatingItem updatingItem, string itemId)
        {
            var userId = GetUserId(Request);
            var filter = Builders<Item>.Filter.And(
                                        Builders<Item>.Filter.Eq(item => item.OwnerId, userId),
                                        Builders<Item>.Filter.Eq(item => item.Id, itemId));

            var oldItem =await _dbContext.Items.Find(filter).FirstOrDefaultAsync();
            var itemName = updatingItem.ItemName!=null?updatingItem.ItemName:oldItem.ItemName;
            var itemPrice = updatingItem.ItemPrice!=null?updatingItem.ItemPrice:oldItem.ItemPrice; 

            var update = Builders<Item>.Update
                                        .Set(item => item.ItemName, itemName)
                                        .Set(item => item.ItemPrice, itemPrice);

            var result = await _dbContext.Items.UpdateOneAsync(filter, update);


            /*  
             *  another method
              var updateBuilder = Builders<Item>.Update;
              var updateDefinitions = new List<UpdateDefinition<Item>>();

              if(updatingItem.ItemName!=null)
              {
                  Console.WriteLine("in if 1");
                  var fieldExpression = Builders<Item>.Update.Set(item => item.ItemName, updatingItem.ItemName);
                  updateDefinitions.Add(fieldExpression);
              }

              if(updatingItem.ItemPrice!=null)
              {
                  Console.WriteLine("in if 2");
                  var fieldExpression = Builders<Item>.Update.Set(item => item.ItemPrice, updatingItem.ItemPrice);
                  updateDefinitions.Add(fieldExpression);
              }

              var combinedUpdateDefinition = updateBuilder.Combine(updateDefinitions);  
              var result = await _dbContext.Items.UpdateOneAsync(filter, combinedUpdateDefinition);
             */


            if (result.ModifiedCount > 0)
            {
                return Ok("Successfully Updated");
            }
            else
            {
                return BadRequest("no updates");
            }
        }



        /// <summary>
        /// delete an item by id
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteItem(string itemId)
        {
            var userId = GetUserId(Request);
            try
            {
                //check the user is able to delete the item
                var filter = Builders<Item>.Filter.And(
                             Builders<Item>.Filter.Eq(item => item.OwnerId, userId),
                             Builders<Item>.Filter.Eq(item => item.Id, itemId));

                var result = _dbContext.Items.FindOneAndDelete(filter);
                if(result== null)
                {
                    throw new Exception("item deleteion failed");
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





        //set function for userId
        public string GetUserId(HttpRequest request)
        {
            var accessToken = request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(accessToken) as JwtSecurityToken;
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            return userIdClaim;
        }

    }
}
