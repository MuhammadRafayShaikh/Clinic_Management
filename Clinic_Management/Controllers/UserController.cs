using Clinic_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using System.Text;

namespace Clinic_Management.Controllers
{
    public class UserController : Controller
    {
        private readonly myDbContext myDbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly EmailSettings emailSettings;
        public UserController(
            myDbContext myDbContext,
            IWebHostEnvironment webHostEnvironment,
            IOptions<EmailSettings> emailSettings
            )
        {
            this.webHostEnvironment = webHostEnvironment;
            this.myDbContext = myDbContext;
            this.emailSettings = emailSettings.Value;
        }
        public IActionResult Register(string? email = null, string? returnUrl = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    var urlBytes = Convert.FromBase64String(returnUrl);
                    var decodeUrl = Encoding.UTF8.GetString(urlBytes);
                    if (Url.IsLocalUrl(decodeUrl))
                    {
                        TempData["returnUrl"] = decodeUrl;
                    }
                }
            }
            catch
            {
                TempData["returnUrl"] = null;
            }

            if (email != null)
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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(User user, IFormFile? Image, string? remember)
        {
            //return Json(remember);
            //return Json(Image);
            //return Json(user);
            if (string.IsNullOrEmpty(user.Phone))
            {
                ModelState.AddModelError("Phone", "Phone is required");
            }
            if (string.IsNullOrEmpty(user.Address))
            {
                ModelState.AddModelError("Address", "Address is required");
            }
            if (user.Gender != null)
            {
                TempData["userType"] = "patient";
                if (string.IsNullOrEmpty(user.MedicalHistory))
                {
                    ModelState.AddModelError("MedicalHistory", "Medical History is required");
                }
            }
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

                    this.CookiesSet(user, remember);

                    var smtpClient = new SmtpClient(emailSettings.Host)
                    {
                        Port = emailSettings.Port,
                        Credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(emailSettings.UserName),
                        Subject = "User Registration",
                        Body = $"Congratulations! Hi, {HttpContext.Session.GetString("name")}, You have successfully registered",
                        IsBodyHtml = false,
                    };

                    mailMessage.To.Add(HttpContext.Session.GetString("email"));

                    mailMessage.CC.Add(emailSettings.CCEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                    TempData["success"] = "Successfully Registered";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.email = user.Email;
                    TempData["email_err"] = "Email Already Exists";
                    return View(user);
                }
            }
            ViewBag.email = user.Email;
            return View(user);
        }

        public async Task<JsonResult> checkEmail(string email)
        {
            bool user_email;
            if (HttpContext.Session.GetString("id") == null)
            {
                user_email = await myDbContext.Users
                    .Where(x => x.Email == email)
                    .AnyAsync();
            }
            else
            {
                user_email = await myDbContext.Users
                    .Where(x => x.Email == email && x.Id != Convert.ToInt32(HttpContext.Session.GetString("id")))
                    .AnyAsync();
            }

            if (user_email)
            {
                return Json("Email Already Exist");
            }
            else
            {
                return Json("");
            }
        }

        public IActionResult Login(string? returnUrl)
        {
            //return Json(returnUrl.ToArray());
            //return Json(returnUrl);
            try
            {
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    var bytesUrl = Convert.FromBase64String(returnUrl);
                    var decodeUrl = Encoding.UTF8.GetString(bytesUrl);
                    if (Url.IsLocalUrl(decodeUrl))
                    {
                        TempData["returnUrl"] = decodeUrl;
                    }
                }
            }
            catch
            {
                TempData["returnUrl"] = null;
            }
            //TempData["returnUrl"] = returnUrl;
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(User user, string? returnUrl, string remember)
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
                    HttpContext.Session.SetString("id", userData.Id.ToString());
                    HttpContext.Session.SetString("name", userData.Name);
                    HttpContext.Session.SetString("email", userData.Email);
                    HttpContext.Session.SetString("role", userData.Role.ToString());
                    HttpContext.Session.SetString("image", userData.Image);
                    HttpContext.Session.SetString("password", userData.Password);
                    if (userData.Role == 3)
                    {
                        HttpContext.Session.SetString("gender", userData.Gender.ToString());
                        HttpContext.Session.SetString("medicalhistory", userData.MedicalHistory);
                    }
                    if (userData.Role == 0 || userData.Role == 1 || userData.Role == 3)
                    {
                        HttpContext.Session.SetString("phone", userData.Phone);
                        HttpContext.Session.SetString("address", userData.Address);
                    }
                    else
                    {
                        HttpContext.Session.SetString("staff_role", userData.Staff_Role.ToString());
                    }

                    this.CookiesSet(userData, remember);

                    TempData["successLogin"] = "Successfully Login";
                    if (userData.Role == 0 || userData.Role == 3)
                    {
                        if (TempData["returnUrl"] == null)
                        {
                            returnUrl = null;
                        }
                        else
                        {
                            returnUrl = TempData["returnUrl"].ToString();
                        }
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
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
        private void CookiesSet(User userData, string? remember)
        {
            if (remember == "true")
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("id", userData.Id.ToString(), cookieOptions);
                Response.Cookies.Append("name", userData.Name, cookieOptions);
                Response.Cookies.Append("email", userData.Email, cookieOptions);
                Response.Cookies.Append("role", userData.Role.ToString(), cookieOptions);
                Response.Cookies.Append("image", userData.Image, cookieOptions);
                if (userData.Role == 3)
                {
                    Response.Cookies.Append("gender", userData.Gender.ToString(), cookieOptions);
                    Response.Cookies.Append("medicalhistory", userData.MedicalHistory, cookieOptions);
                }
                if (userData.Role == 0 || userData.Role == 1 || userData.Role == 3)
                {
                    Response.Cookies.Append("phone", userData.Phone, cookieOptions);
                    Response.Cookies.Append("address", userData.Address, cookieOptions);
                }
                else
                {
                    Response.Cookies.Append("staff_role", userData.Staff_Role.ToString(), cookieOptions);
                }
            }
        }

        [AuthenticationFilter]
        public IActionResult Logout()
        {
            if (Request.Cookies["remember_info_shown"] != null)
            {
                Response.Cookies.Delete("remember_info_shown");
            }
            HttpContext.Session.Clear();
            Response.Cookies.Delete("id");
            Response.Cookies.Delete("name");
            Response.Cookies.Delete("email");
            Response.Cookies.Delete("role");
            Response.Cookies.Delete("image");

            if (Request.Cookies["role"] == "3")
            {
                Response.Cookies.Delete("gender");
                Response.Cookies.Delete("medicalhistory");
            }

            if (Request.Cookies["role"] == "0" || Request.Cookies["role"] == "1" || Request.Cookies["role"] == "3")
            {
                Response.Cookies.Delete("phone");
                Response.Cookies.Delete("address");
            }

            else
            {
                Response.Cookies.Delete("staff_role");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
