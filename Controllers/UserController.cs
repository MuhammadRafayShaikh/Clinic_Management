using Clinic_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Net.Mail;
using System.Net;

namespace Clinic_Management.Controllers
{
    public class UserController : Controller
    {
        private readonly myDbContext myDbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        public UserController(myDbContext myDbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.myDbContext = myDbContext;
        }
        public IActionResult Register(string? email = null)
        {
            if(email != null)
            {
                if (!string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
                {
                    TempData["error"] = "You are already logged in";
                }
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                ViewBag.email = email;
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user, IFormFile? Image)
        {
            //return Json(Image);
            //return Json(user);
            if (ModelState.IsValid)
            {
                //return Json(Image);
                if (Image != null)
                {

                    //return Json(DateTime.Now.ToString("yyyyMMddHHmmss"));

                    var namenotExt = Path.GetFileNameWithoutExtension(Image.FileName);
                    var extension = Path.GetExtension(Image.FileName);
                    //return Json(ImageNamess);

                    var ImageName = namenotExt + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    var path = Path.Combine(webHostEnvironment.WebRootPath, "UserImages");

                    var ImagePath = Path.Combine(path, ImageName);

                    using (var stream = new FileStream(ImagePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    user.Image = ImageName;

                }
                else
                {
                    user.Image = "user.png";
                }
                var user_email = await myDbContext.Users.Where(x => x.Email == user.Email).FirstOrDefaultAsync();
                if (user_email == null)
                {

                    var hash = new PasswordHasher<User>();
                    user.Password = hash.HashPassword(user, user.Password);
                    if (user.Gender == null)
                    {
                        user.Role = 0;
                    }
                    else
                    {
                        user.Role = 3;
                    }
                    await myDbContext.Users.AddAsync(user);
                    await myDbContext.SaveChangesAsync();
                    var user_id = await myDbContext.Users.Where(x => x.Email == user.Email).FirstOrDefaultAsync();
                    HttpContext.Session.SetString("id", user_id.Id.ToString());
                    HttpContext.Session.SetString("name", user.Name);
                    HttpContext.Session.SetString("email", user.Email);
                    HttpContext.Session.SetString("phone", user.Phone);
                    HttpContext.Session.SetString("address", user.Address);
                    HttpContext.Session.SetString("role", user.Role.ToString());
                    HttpContext.Session.SetString("image", user.Image);
                    HttpContext.Session.SetString("password", user.Password);

                    if (user.Role == 3)
                    {
                        HttpContext.Session.SetString("gender", user.Gender.ToString());
                        HttpContext.Session.SetString("medicalhistory", user.MedicalHistory);
                    }
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("rafayrashid457@gmail.com", "siuymtzsjdocebzk"),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("rafayrashid457@gmail.com"),
                        Subject = "Seminar Registration",
                        Body = $"Congratulations! Hi, {HttpContext.Session.GetString("name")}, You have successfully registered",
                        IsBodyHtml = false,
                    };

                    mailMessage.To.Add(HttpContext.Session.GetString("email"));

                    mailMessage.CC.Add("aptechrafay2@gmail.com");

                    await smtpClient.SendMailAsync(mailMessage);
                    TempData["success"] = "Successfully Registered";
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    TempData["email_err"] = "Email Already Exists";
                    return View(user);
                }
            }
            TempData["error"] = "Error";
            return View(user);
        }

        public async Task<JsonResult> checkEmail(string email)
        {
            var user_email = await myDbContext.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (user_email != null)
            {
                return Json("Email Already Exist");
            }
            else
            {
                return Json("");
            }
        }

        public IActionResult Login()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                TempData["error"] = "Please Fill All Fields";
                return View(user);
            }
            var userData = await myDbContext.Users.Where(x => x.Email == user.Email).FirstOrDefaultAsync();
            //return Json(userData);
            if (userData != null)
            {
                var hash = new PasswordHasher<User>();

                var result = hash.VerifyHashedPassword(user, userData.Password, user.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    if(userData.Role == 3)
                    {
                        HttpContext.Session.SetString("gender", userData.Gender.ToString());
                        HttpContext.Session.SetString("medicalhistory", userData.MedicalHistory);
                    }
                    if (userData.Role == 0 || userData.Role == 1 || userData.Role == 3)
                    {

                        HttpContext.Session.SetString("id", userData.Id.ToString());
                        HttpContext.Session.SetString("name", userData.Name);
                        HttpContext.Session.SetString("email", userData.Email);
                        HttpContext.Session.SetString("phone", userData.Phone);
                        HttpContext.Session.SetString("address", userData.Address);
                        HttpContext.Session.SetString("role", userData.Role.ToString());
                        HttpContext.Session.SetString("image", userData.Image);
                        HttpContext.Session.SetString("password", userData.Password);

                    }
                    else
                    {
                        HttpContext.Session.SetString("id", userData.Id.ToString());
                        HttpContext.Session.SetString("name", userData.Name);
                        HttpContext.Session.SetString("email", userData.Email);
                        HttpContext.Session.SetString("staff_role", userData.Staff_Role.ToString());
                        HttpContext.Session.SetString("role", userData.Role.ToString());
                        HttpContext.Session.SetString("image", userData.Image);
                        HttpContext.Session.SetString("password", userData.Password);
                    }
                    if (userData.Role == 0 || userData.Role == 3)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }
                else
                {
                    TempData["error"] = "Password is incorrect";
                    return View(user);
                }
            }
            else
            {
                TempData["error"] = "Email is incorrect";
                return View(user);
            }
            return View(user);
        }
        [AuthenticationFilter]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }

    }
}
