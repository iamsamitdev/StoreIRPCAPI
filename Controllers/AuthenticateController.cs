using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StoreIRPCAPI.Models;

namespace StoreIRPCAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{

    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    // Constructor
    public AuthenticateController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    // Register for normal user
    // Post api/authenticate/register-user
    [HttpPost]
    [Route("register-user")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // เช็คว่ามี username นี้ในระบบแล้วหรือไม่
        var userExist = await _userManager.FindByNameAsync(model.Username);
        if (userExist != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User already exist!" 
                }
            );
        }

        // เช็คว่ามี email นี้ในระบบแล้วหรือไม่
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        // สร้าง user ใหม่
        var result = await _userManager.CreateAsync(user, model.Password);

        // ถ้าสร้างไม่สำเร็จ
        if (!result.Succeeded){
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User creation failed! Please check user details and try again." 
                }
            );
        }

        // ถ้าไม่มี Role User ให้สร้าง Role User ใหม่ และเพิ่ม User ลงใน Role User
        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        } else {
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }

        // ถ้าไม่มี Role Manager ให้สร้าง Role Manager ใหม่ 
        if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
        } 

        // ถ้าไม่มี Role Admin ให้สร้าง Role Admin ใหม่ 
        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        }

        // สร้าง user สำเร็จ
        return Ok(new Response { 
            Status = "Success", 
            Message = "User created successfully!" 
        });
    }

    // Register for manager
    // Post api/authenticate/register-manager
    [HttpPost]
    [Route("register-manager")]
    public async Task<IActionResult> RegisterManager([FromBody] RegisterModel model)
    {   
        // เช็คว่ามี username นี้ในระบบแล้วหรือไม่
        var userExist = await _userManager.FindByNameAsync(model.Username);
        if (userExist != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User already exist!" 
                }
            );
        }

        // เช็คว่ามี email นี้ในระบบแล้วหรือไม่
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        // สร้าง user ใหม่
        var result = await _userManager.CreateAsync(user, model.Password);

        // ถ้าสร้างไม่สำเร็จ
        if (!result.Succeeded){
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User creation failed! Please check user details and try again." 
                }
            );
        }

        // ถ้าไม่มี Role User ให้สร้าง Role User ใหม่
        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        } 

        // ถ้าไม่มี Role Manager ให้สร้าง Role Manager ใหม่ และเพิ่ม User ลงใน Role Manager
        if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
        } else {
            await _userManager.AddToRoleAsync(user, UserRoles.Manager);
        }

        // ถ้าไม่มี Role Admin ให้สร้าง Role Admin ใหม่ 
        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        } 

        // สร้าง user สำเร็จ
        return Ok(new Response { 
            Status = "Success", 
            Message = "User created successfully!" 
        });
    }

    // Register for admin
    // Post api/authenticate/register-admin
    [HttpPost]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username!);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User already exists!" 
                }
            );

        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User creation failed! Please check user details and try again." 
                }
            );

        // ถ้าไม่มี Role User ให้สร้าง Role User ใหม่
        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        } 

        // ถ้าไม่มี Role Manager ให้สร้าง Role Manager ใหม่
        if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
        }

        // ถ้าไม่มี Role Admin ให้สร้าง Role Admin ใหม่ และเพิ่ม User ลงใน Role Admin
        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        } else {
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        }

        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }

    // Login
    // Post api/authenticate/login
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username!);

        // ถ้า login สำเร็จ
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        // ถ้า login ไม่สำเร็จ
        return Unauthorized();
    }

    // Method for generating JWT token
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddDays(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return token; 
    }
}