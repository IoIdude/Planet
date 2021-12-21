using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Planet.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace Planet.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext db;
        private IHttpContextAccessor httpContextAccessor;

        public HomeController(ApplicationContext appContext, IHttpContextAccessor httpContext)
        {
            db = appContext;
            httpContextAccessor = httpContext;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string email, string login, int? id, int page = 1, SortState sortOrder = SortState.EmailAsc)
        {
            IQueryable<User> users = db.Users;

            //фильтрация или поиск
            if (id != null && id > 0)
            {
                users = users.Where(p => p.Id == id);
            }
            if (!String.IsNullOrEmpty(email))
            {
                users = users.Where(p => p.Email.Contains(email));
            }
            if (!String.IsNullOrEmpty(login))
            {
                users = users.Where(p => p.Login.Contains(login));
            }

            //сортировка
            switch(sortOrder)
            {
                case SortState.IdAsc:
                    {
                        users = users.OrderBy(m => m.Id);
                        break;
                    }
                case SortState.IdDesc:
                    {
                        users = users.OrderByDescending(m => m.Id);
                        break;
                    }
                case SortState.EmailAsc:
                    {
                        users = users.OrderBy(m => m.Email); 
                        break;
                    }

                case SortState.EmailDesc:
                    {
                        users = users.OrderByDescending(m => m.Email); 
                        break;
                    }

                case SortState.LoginAsc:
                    {
                        users = users.OrderBy(m => m.Login); 
                        break;
                    }

                case SortState.LoginDesc:
                    {
                        users = users.OrderByDescending(m => m.Login); 
                        break;
                    }
                default:
                    {
                        users = users.OrderBy(m => m.Id);
                        break;
                    }
            }

            //пагинация
            int pageSize = 16;
            var count = await users.CountAsync();
            var item = await users.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                SortViewModel = new SortViewModel(sortOrder),
                FilteViewModel = new FilteViewModel(id, email, login),
                Users = item
            };
            return View(viewModel); 
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user,  IFormFile uploadImg)
        {
            byte[] imageData = null;

            using (var binaryReader = new BinaryReader(uploadImg.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)uploadImg.Length);
            }

            user.Avatar = imageData;
            user.Role = db.Roles.Single(p => p.Role_Name == "User");

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(predicate => predicate.Id == id);
                if (User != null)
                {
                    return View(user);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(predicate => predicate.Id == id);
                if (User != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(predicate => predicate.Id == id);
                if (User != null)
                {
                    return View(user);
                }
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user, IFormFile uploadImg)
        {
            byte[] imageData = null;

            using (var binaryReader = new BinaryReader(uploadImg.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)uploadImg.Length);
            }

            user.Avatar = imageData;
            user.Role = db.Roles.Single(p => p.Role_Name == "User");

            db.Users.Update(user);
            await db.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }
            else return RedirectToAction("Profile");
        }

        public ViewResult Reg()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Reg(User user, IFormFile uploadImg)
        {
            if (ModelState.IsValid)
            {
                User proverka = db.Users.FirstOrDefault(m => m.Login == user.Login && m.Email == user.Email);

                if (proverka == null)
                {
                    await AddPicture(user, uploadImg);
                    return RedirectToAction("Auth");
                }
                else if (proverka != null)
                {
                    return RedirectToAction("Reg");
                }
            }

            return View(user);
        }

        [HttpPost]
        public Task<IActionResult> AddPicture(User user, IFormFile uploadImg)
        {
            byte[] imageData = null;

            // считываем переданный файл в массив байтов
            using (var binaryReader = new BinaryReader(uploadImg.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)uploadImg.Length);
            }
            
            user.Avatar = imageData;

            return RoleAdd(user);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAdd(User user)
        {
            //Role role = new Role { Role_Name = "Admin" };
            //Role role2 = new Role { Role_Name = "User" };

            //db.Roles.AddRange(role, role2);
            //await db.SaveChangesAsync();

            user.Role = db.Roles.Single(p => p.Role_Name == "User");

            db.Add(user);
            await db.SaveChangesAsync();
            await Authenticate(user);

            return RedirectToAction("Auth");
        }

        //[HttpGet]
        //public IActionResult Auth()
        //{
        //    return View();
        //}

        [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Auth(User user)
        {
            if (user.Login != null)
            {
                User proverka = db.Users.Include(p => p.Role).FirstOrDefault(m => m.Login == user.Login && m.Password == user.Password);
                
                if (proverka != null)
                {
                    await Authenticate(proverka);

                    if (proverka.RoleID == db.Roles.FirstOrDefault(p => p.Role_Name == "Admin").Id) // HttpContext.User.IsInRole("Admin")
                    {
                        return RedirectToAction("Index");
                    }
                    else if (proverka.RoleID == db.Roles.FirstOrDefault(p => p.Role_Name == "User").Id) // HttpContext.User.IsInRole("User")
                    {
                        return RedirectToAction("Profile");
                    }
                }
                else
                {
                    return RedirectToAction("Auth");
                }
            }

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Profile(string login)
        {
            User users = db.Users.FirstOrDefault(p => p.Login == login); 

            if (users != null)
            {
                return View(users);
            }
            else if (User.Identity.IsAuthenticated)
            {
                User user = db.Users.FirstOrDefault(p => p.Login == HttpContext.User.Identity.Name);

                return View(user);
            }
            else return RedirectToAction("Auth");
        }

        //[HttpGet]
        //[Authorize(Roles = "User")]
        //public async Task<IActionResult> EditProfile()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        User user = db.Users.FirstOrDefault(p => p.Login == HttpContext.User.Identity.Name);
        //        return View(user);
        //    }
        //    else return RedirectToAction("Auth");
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (User.IsInRole("Admin"))
            {
                if (id != null)
                {
                    User user = await db.Users.FirstOrDefaultAsync(predicate => predicate.Id == id);
                    if (User != null)
                    {
                        return View(user);
                    }
                }
            }
            else
            {
                User user = db.Users.FirstOrDefault(p => p.Login == HttpContext.User.Identity.Name);
                return View(user);
            }
            return NotFound();
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Role_Name)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            //HttpContext.Session.SetString("loginUser", user.Login);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public IActionResult AddPost()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddPost(Post post, IFormFile uploadImg)
        {
            byte[] imageData = null;

            if (User.Identity.IsAuthenticated)
            {
                User user = db.Users.FirstOrDefault(p => p.Login == HttpContext.User.Identity.Name);

                using (var binaryReader = new BinaryReader(uploadImg.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)uploadImg.Length);
                }

                
                post.User = db.Users.Find(user.Id); //Single(p => p.Id == user.Id);
                post.Picture = imageData;
                //db.Posts.Include(p => p.User);

                db.Add(post);
                await db.SaveChangesAsync();

                return RedirectToAction("Profile"); ;
            }

            return RedirectToAction("AddPost");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Posts(int id) // string login
        {
            IndexViewModel viewModel = new IndexViewModel
            {
                Posts = db.Posts,
                IdentityUserLog = db.Users.FirstOrDefault(item => item.Login == User.Identity.Name).Id,
                ID = id,
                //Login = login
                //Login = HttpContext.Session.GetString("loginUser")
            };

            return View(viewModel);
        }

        public IActionResult CheckPosts()
        {
            IQueryable<Post> posts = db.Posts;
            //db.Posts.Include(p => p.User);
            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id != null)
            {
                Post post = await db.Posts.FirstOrDefaultAsync(predicate => predicate.id == id);
                if (post != null)
                {
                    db.Posts.Remove(post);
                    await db.SaveChangesAsync();
                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (User.IsInRole("User"))
                    {
                        return RedirectToAction("Posts");
                    }
                }
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(int? id)
        {
            Post post = await db.Posts.FirstOrDefaultAsync(predicate => predicate.id == id);

            if (post != null)
            {
                return View(post);
            }
            else return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Post post, IFormFile  uploadImg)
        {
            byte[] imageData = null;

            using (var binaryReader = new BinaryReader(uploadImg.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)uploadImg.Length);
            }

            post.User = db.Users.Find(post.UserID);
            post.Picture = imageData;

            db.Posts.Update(post);
            await db.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            
            return RedirectToAction("Auth");
        }
    }
}


