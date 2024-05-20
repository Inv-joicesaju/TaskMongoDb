using Amazon.Runtime.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using System.Text;

namespace TaskMongoDb.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger,  IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;

        }



        private dynamic ValidateToken(string token)
        {
            // You need to replace this with your secret key and other configuration
            var secretKey = _configuration["JWT:Secret"];
            var audience = _configuration["JWT:ValidAudience"];
            var issuer = _configuration["JWT:ValidIssuer"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidIssuer = issuer,
                ValidAudience = audience,
                RequireExpirationTime = true,
                ValidateLifetime = true
            };

            try
            {
                // Validate token
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                var result = new
                {
                    Message = "token is valid",
                    isValid = true,
                };
                return result;
            }
            catch (SecurityTokenExpiredException)
            {
                var result = new
                {
                    Message = new { Message = "Invalid token", ErrorCode = 1004 },
                    isValid = false,
                };
                _logger.LogError("Token has expired.");
                return result;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                var result = new
                {
                    Message = new { Message = "Invalid token", ErrorCode = 1004 },
                    isValid = false,
                };
                _logger.LogError("Token signature is invalid.");
                return result;
            }
            catch (Exception ex)
            {
                var result = new
                {
                    Message = new { Message = "Invalid token", ErrorCode = 1004 },
                    isValid = false,
                };
                _logger.LogError("Invalid tokens. " + ex.Message);
                return result;
            }
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                //checked that the  customException worked or not
                //throw new CustomException1("This is a custom exception.");

                // Check if there is a token in the request
                var token1 = context.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrWhiteSpace(token1))
                {
                    throw new Exception("token required");
                }

                var token = token1.Substring("Bearer ".Length).Trim();
                Debug.WriteLine(token);

                // Validate the token
                var validationMessage = ValidateToken(token);

                if (!validationMessage.isValid)
                {
                    throw new Exception(validationMessage.Message);
                }


                //checking the user type(0-client user)
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                // Continue processing if the token is valid
                await _next(context);

            }
           /* catch (InvalidTokenException ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                await HandleInvalidTokenAsync(context, ex.ErrorResponse);
            }
            catch (TokenRequiredException ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                await HandleInvalidTokenAsync(context, ex.ErrorResponse);
            }
            catch (CustomException1 ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                await HandleInvalidTokenAsync(context, new ErrorResponse { Message = ex.Message, ErrorCode = 1111 });
                return;
            }*/
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                await HandleInvalidTokenAsync(context, new { Message = "Invalid token", ErrorCode = 1004 });
            }
        }


        private async Task HandleInvalidTokenAsync(HttpContext context, Object errorResponse)
        {
            try
            {
                // Check if headers are read-only before attempting to modify
                if (!context.Response.HasStarted)
                {
                    _logger.LogError($"{errorResponse}");
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    string jsonResponse = JsonSerializer.Serialize(new { errorResponse });

                    Debug.WriteLine("in moddleware code");
                    Debug.WriteLine(jsonResponse);

                    await context.Response.WriteAsync(jsonResponse);
                }
            }
            catch (Exception ex)
            {
                // You might want to return an appropriate error response here
                var errorResponse1 = new { message = ex.Message, errorCode = 500 };
                var errorJson = JsonSerializer.Serialize(errorResponse1);
                await context.Response.WriteAsync(errorJson);
            }

        }
    }
}
