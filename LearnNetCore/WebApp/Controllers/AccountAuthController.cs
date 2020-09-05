﻿
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
        public IActionResult VerifCode(UserViewModel userVM)
        {
            if (userVM.VerificationCode != null)
            {
                var id = HttpContext.Session.GetString("id");
                var jsonUserVM = JsonConvert.SerializeObject(userVM);
                var buffer = System.Text.Encoding.UTF8.GetBytes(jsonUserVM);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var result = client.PostAsync("account/verify/" + id, byteContent).Result;
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
        [HttpPost]
        public IActionResult Verify(string verCode)
        {
            var id = HttpContext.Session.GetString("id");
            var contentData = new StringContent(verCode, System.Text.Encoding.UTF8, "application/json");

            var resTask = client.PostAsync("account/Verify/" + id, contentData);

            var result = resTask.Result;
            if (result.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("verified", "true");
            }

            return Json(result, new Newtonsoft.Json.JsonSerializerSettings());
        }
        public async Task<Uri> CreateLoginAsync(UserViewModel loginVM, string verCode)
        {
            var id = HttpContext.Session.GetString("id");
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "account/verify/" + id, loginVM);
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
            return Redirect("/dashboard");
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
                    return Json(new { status = false, msg = "Username" });
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
