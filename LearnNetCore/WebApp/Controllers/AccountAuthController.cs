
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LearnNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApp.Controllers
{
    public class AccountAuthController : Controller
    {
        readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44380/api/")
        };

        public IActionResult Index()
        {
            return View();
        }
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }
        [Route("verify")]
        public IActionResult Verify()
        {
            return View();
        }
        [Route("verify/post")]
        public IActionResult VerifCode(User userVM)
        {
            var email = userVM.Email;
            var verCode = userVM.SecurityStamp;
            if (verCode != null)
            {
                var jsonUserVM = JsonConvert.SerializeObject(userVM);
                var buffer = System.Text.Encoding.UTF8.GetBytes(jsonUserVM);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var result = client.PostAsync("account/verify/" + email , byteContent).Result;
                if (result.IsSuccessStatusCode)
                {
                    var data = result.Content.ReadAsStringAsync().Result;
                    if (data != "")
                    {
                        var json = JsonConvert.DeserializeObject(data).ToString();
                        var account = JsonConvert.DeserializeObject<UserViewModel>(json);
                        if (account.RoleName == "Admin" || account.RoleName == "Sales")
                        {
                            HttpContext.Session.SetString("id", account.Id);
                            HttpContext.Session.SetString("uname", account.Username);
                            HttpContext.Session.SetString("email", account.Email);
                            HttpContext.Session.SetString("lvl", account.RoleName);
                            if (account.RoleName == "Admin")
                            {
                                return Json(new { status = true, msg = "Login Successfully !", acc = "Admin" });
                            }
                            else
                            {
                                return Json(new { status = true, msg = "Login Successfully !", acc = "Sales" });
                            }
                        }
                        else
                        {
                            return Json(new { status = false, msg = "Username" });
                        }
                    }
                    else
                    {
                        return Json(new { status = false, msg = "Username" });
                    }
                }
                else
                {
                    return Json(new { status = false, msg = "Please check your code verification" });
                }
            }
            else
            {
                return Json(new { status = false, msg = "Oops! Something went wrong" });
            }
        }
        [Route("verif")]
        [HttpPost]
        public IActionResult Verify(User model)
        {
            if (model.SecurityStamp != null)
            {
                var jsonUserVM = JsonConvert.SerializeObject(model);
                var buffer = System.Text.Encoding.UTF8.GetBytes(jsonUserVM);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resTask = client.PostAsync("Account/verify/", byteContent);
                resTask.Wait();
                var result = resTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var data = result.Content.ReadAsStringAsync().Result;
                    if (data != "")
                    {
                        var json = JsonConvert.DeserializeObject(data).ToString();
                        var account = JsonConvert.DeserializeObject<UserViewModel>(json);
                        if (account.RoleName == "Admin" || account.RoleName == "Sales" || account.RoleName == "HR")
                        {
                            HttpContext.Session.SetString("id", account.Id);
                            HttpContext.Session.SetString("uname", account.Username);
                            HttpContext.Session.SetString("email", account.Email);
                            HttpContext.Session.SetString("lvl", account.RoleName);
                            if (account.RoleName == "Admin")
                            {
                                return Json(new { status = true, msg = "Well done. Your account has been verified", acc = "Admin" });
                            }
                            else if (account.RoleName == "Sales")
                            {
                                return Json(new { status = true, msg = "Well done. Your account has been verified", acc = "Sales" });
                            }
                            else
                            {
                                return Json(new { status = true, msg = "Well done. Your account has been verified", acc = "HR" });
                            }
                        }
                        else
                        {
                            return Json(new { status = false, msg = "Please registration first." });
                        }
                    }
                    else
                    {
                        return Json(new { status = false, msg = "The data must be filled" });
                    }
                }
                else
                {
                    return Json(new { status = false, msg = "Something went wrong" });
                }
            }
            return Redirect("/verify");
        }


            ////var email = HttpContext.Session.GetString("email");
            //var contentData = new StringContent(emailnya, System.Text.Encoding.UTF8, "application/json");

            //var resTask = client.PostAsync("account/Verify/" + verCode, contentData);

            //var result = resTask.Result;
            //if (result.IsSuccessStatusCode)
            //{
            //    var data = result.Content.ReadAsStringAsync().Result;
            //    var json = JsonConvert.DeserializeObject(data).ToString();
            //    var account = JsonConvert.DeserializeObject<UserViewModel>(json);
            //    if (account.RoleName == "Admin" || account.RoleName == "Sales")
            //    {
            //        HttpContext.Session.SetString("id", account.Id);
            //        HttpContext.Session.SetString("uname", account.Username);
            //        HttpContext.Session.SetString("email", account.Email);
            //        HttpContext.Session.SetString("lvl", account.RoleName);
            //        if (account.RoleName == "Admin")
            //        {
            //            return Json(new { status = true, msg = "Login Successfully !", acc = "Admin" });
            //        }
            //        else
            //        {
            //            return Json(new { status = true, msg = "Login Successfully !", acc = "Sales" });
            //        }
            //    }
            //    else
            //    {
            //        return Json(new { status = false, msg = "Username" });
            //    }
            //}

            //return Json(result, new Newtonsoft.Json.JsonSerializerSettings());
        
        public async Task<Uri> CreateLoginAsync(UserViewModel loginVM, string verCode)
        {
            //var id = HttpContext.Session.GetString("id");
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "account/verify/" + verCode, loginVM);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }
        [Route("regisvalidate")]
        public IActionResult Validate(RegisterViewModel registerViewModel)
        {
            if (registerViewModel.Username != null)
            {
                var json = JsonConvert.SerializeObject(registerViewModel);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var result = client.PostAsync("account/register/", byteContent).Result;
                if (result.IsSuccessStatusCode)
                {
                    return Json(new { status = true, code = result, msg = "Registration success" });
                }
                else
                {
                    return Json(new { status = false, msg = "Something went wrong" });
                }
            }
            return Redirect("/register");
        }
        [Route("validate")]
        public IActionResult Validate(UserViewModel userVM)
        {
            if (userVM.Username == null)
            {
                var jsonUserVM = JsonConvert.SerializeObject(userVM);
                var buffer = System.Text.Encoding.UTF8.GetBytes(jsonUserVM);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resTask = client.PostAsync("Account/login/", byteContent);
                resTask.Wait();
                var result = resTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var data = result.Content.ReadAsStringAsync().Result;
                    if (data != "")
                    {
                        var json = JsonConvert.DeserializeObject(data).ToString();
                        var account = JsonConvert.DeserializeObject<UserViewModel>(json);
                        if (account.RoleName == "Admin" || account.RoleName == "Sales" || account.RoleName == "HR")
                        {
                            HttpContext.Session.SetString("id", account.Id);
                            HttpContext.Session.SetString("uname", account.Username);
                            HttpContext.Session.SetString("email", account.Email);
                            HttpContext.Session.SetString("lvl", account.RoleName);
                            if (account.RoleName == "Admin")
                            {
                                return Json(new { status = true, msg = "Login Successfully !", acc = "Admin" });
                            }
                            else if (account.RoleName=="Sales")
                            {
                                return Json(new { status = true, msg = "Login Successfully !", acc = "Sales" });
                            }
                            else
                            {
                                return Json(new { status = true, msg = "Login Successfully !", acc = "HR" });
                            }
                        }
                        else
                        {
                            return Json(new { status = false, msg = "Please registration first." });
                        }
                    }
                    else
                    {
                        return Json(new { status = false, msg = "The data must be filled" });
                    }
                }
                else
                {
                    return Json(new { status = false, msg = "Something went wrong" });
                }
            }
            return Redirect("/login");

        }
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/login");
        }


    }
}
