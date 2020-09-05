using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using LearnNetCore.Context;
using LearnNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using LearnNetCore.Verify;
using System.Net;
using System.Text;

namespace LearnNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly MyContext _context;
        private readonly UserManager<User> _userManager;
        SmtpClient client = new SmtpClient();
        Verification verification = new Verification();
        RandomGenerator randomGenerator = new RandomGenerator();
        ServiceEmail serviceEmail = new ServiceEmail();


        public AccountController(MyContext myContext, UserManager<User> userManager)
        {
            _context = myContext;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(UserViewModel userVM)
        {
            if (ModelState.IsValid)
            {
                var pwd = userVM.Password;
                var masuk = _context.UserRoles.Include("Role").Include("User").SingleOrDefault(m => m.User.Email == userVM.Email);
                if (masuk == null)
                {
                    return BadRequest("Please use the existing email or register first");
                }
                else if (!BCrypt.Net.BCrypt.Verify(userVM.Password, masuk.User.PasswordHash))
                {
                    return BadRequest("Incorret password");
                }
                else if (pwd == null || pwd.Equals(""))
                {
                    return BadRequest("Please enter the password");
                }
                else
                {
                    var user = new UserViewModel();
                    user.Id = masuk.User.Id;
                    user.Username = masuk.User.UserName;
                    user.Email = masuk.User.Email;
                    user.Phone = masuk.User.PhoneNumber;
                    user.RoleName = masuk.Role.Name;
                    return StatusCode(200, user);

                }
            }
            return BadRequest(500);
        }
        [HttpPost]
        [Route("verify/{id}")]
        public async Task<IActionResult> VerifyCode(string id, string verCode)
        {
            if (ModelState.IsValid)
            {
                var getCode = _context.Users.Where(U => U.SecurityStamp == verCode).Any();
                if (!getCode)
                {
                    return BadRequest(new { msg = "Verification proccess is failed. Please enter the invalid code" });
                }
                var userId = _context.Users.Where(U => U.Id == id).FirstOrDefault();
                userId.SecurityStamp = null;
                userId.EmailConfirmed = true;
                await _context.SaveChangesAsync();
                return Ok("Wel done. Your account is verified");


                //var getUserRole = _context.UserRoles.Include("User").Include("Role").SingleOrDefault(x => x.User.Email == userVM.Email);
                //if (getUserRole == null)
                //{
                //    return NotFound( new { msg = "Please using the existing email or sign up first" });
                //}
                //else if (userVM.VerificationCode != getUserRole.User.SecurityStamp)
                //{
                //    return BadRequest(new { msg = "Incorrect code. Please try again" });
                //}
                //else
                //{
                //    var user = new UserViewModel();
                //    user.Id = getUserRole.User.Id;
                //    user.Username = getUserRole.User.UserName;
                //    user.RoleName = getUserRole.Role.Name;
                //    return StatusCode(200, user);
                //}
            }
            return BadRequest(500);
        }


        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegisterViewModel registerVM)
        {
            var theCode = randomGenerator.GenerateRandom().ToString();
            serviceEmail.SendEmail(registerVM.Email, theCode);
            var pwHashed = BCrypt.Net.BCrypt.HashPassword(registerVM.Password, 12);
                var user = new User
                {
                    Email = registerVM.Email,
                    PasswordHash = pwHashed,
                    UserName = registerVM.Username,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    EmailConfirmed = false,
                    PhoneNumber = registerVM.Phone,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    SecurityStamp = theCode,
                    AccessFailedCount = 0
                };
                _context.Users.AddAsync(user);
                var role = new UserRole
                {
                    UserId = user.Id,
                    RoleId = "2"
                };
                _context.UserRoles.AddAsync(role);
                _context.SaveChanges();
                return Ok("Registered successfully");

        }
        // GET api/values
        [HttpGet]
        public async Task<List<UserViewModel>> GetAll()
        {
            List<UserViewModel> list = new List<UserViewModel>();

            var getUserRole = await _context.UserRoles.Include("User").Include("Role").ToListAsync();
            if (getUserRole.Count == 0)
            {
                return null;
            }
            foreach (var item in getUserRole)
            {
                var user = new UserViewModel()
                {
                    Id = item.User.Id,
                    Username = item.User.UserName,
                    Email = item.User.Email,
                    Password = item.User.PasswordHash,
                    Phone = item.User.PhoneNumber,
                    RoleName = item.Role.Name,
                };
                list.Add(user);
            }
            return list;
        }
        [HttpGet("{id}")]
        public UserViewModel GetID(string id)
        {

            var getData = _context.UserRoles.Include("User").Include("Role").SingleOrDefault(x => x.UserId == id);
            if (getData == null || getData.Role == null || getData.User == null)
            {
                return null;
            }
            var user = new UserViewModel()
            {
                Id = getData.User.Id,
                Username = getData.User.UserName,
                Email = getData.User.Email,
                Password = getData.User.PasswordHash,
                Phone = getData.User.PhoneNumber,
                RoleID = getData.Role.Id,
                RoleName = getData.Role.Name
            };
            return user;
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var getId = _context.Users.Find(id);
            _context.Users.Remove(getId);
            _context.SaveChanges();
            return Ok("Deleted succesfully");
        }
        [HttpPost]
        public IActionResult Create(UserViewModel userVM)
        {
            if (ModelState.IsValid)
            {
                var randomCode = randomGenerator.GenerateRandom();
                var verifyMsg = " Verification code: " + randomCode + "\n\n"
                          + "Enter this code to your application \n\n\n Thank you ";

                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = TimeSpan.FromMinutes(10).Minutes;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(verification.EmailV, verification.PasswordV);

                MailMessage mailMessage = new MailMessage("cjdeveloper123@gmail.com", userVM.Email, "Create Email", verifyMsg);
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                client.Send(mailMessage);
                var pwHashed = BCrypt.Net.BCrypt.HashPassword(userVM.Password, 12);
                var user = new User
                {
                    UserName = userVM.Username,
                    Email = userVM.Email,
                    SecurityStamp = randomCode.ToString(),
                    PasswordHash = pwHashed,
                    PhoneNumber = userVM.Phone,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                };
                _context.Users.Add(user);
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = "2"
                };
                _context.UserRoles.Add(userRole);
                _context.SaveChanges();
                return Ok("Successfully Created");
            }
            return BadRequest("Not Successfully");
        }
        [HttpPut("{id}")]
        public IActionResult Update(string id, UserViewModel userVM)
        {
            if (ModelState.IsValid)
            {
                var getData = _context.UserRoles.Include("Role").Include("User").SingleOrDefault(x => x.UserId == id);
                getData.User.UserName = userVM.Username;
                getData.User.Email = userVM.Email;
                getData.User.PhoneNumber = userVM.Phone;
                if (!BCrypt.Net.BCrypt.Verify(userVM.Password, getData.User.PasswordHash))
                {
                    getData.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userVM.Password);
                }
                getData.RoleId = userVM.RoleID;

                _context.UserRoles.Update(getData);
                _context.SaveChanges();
                return Ok("Update success");
            }
            return BadRequest("Something wrong");
        }
    }
}
