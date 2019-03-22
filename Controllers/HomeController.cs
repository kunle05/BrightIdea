using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using BrightIdeas.Models;

namespace Secrets.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }


        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/createUser")]
        public IActionResult CreateUser(User User)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == User.Email))
                {
                    ModelState.AddModelError("User.Email", "Email already exists!");
                    return View("Index");
                }

                Regex rgx = new Regex(@"^[a-z A-Z]+$");
                Regex rgx2 = new Regex(@"^[a-zA-Z0-9]+$");

                if(rgx2.IsMatch(User.Username) && rgx.IsMatch(User.Name))
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    User.Password = Hasher.HashPassword(User, User.Password);

                    dbContext.Add(User);
                    dbContext.SaveChanges();

                    User thisUser = dbContext.Users.Last();
                    int id = thisUser.UserId;
                    HttpContext.Session.SetInt32("UserId", id);
                    HttpContext.Session.SetString("Username", thisUser.Username);

                    return RedirectToAction("BrightIdeas");
                }
                 else
                {
                    ModelState.AddModelError("Username", "Name must be letters only");
                    return View("Index");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("/signin")]
        public IActionResult UserLogin(myUser myUser)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == myUser.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("myUser.Email", "Invalid Email or Password");
                    return View("Index");
                }

                var hasher = new PasswordHasher<myUser>();
                var result = hasher.VerifyHashedPassword(myUser, userInDb.Password, myUser.Password);

                if(result == 0)
                {
                    ModelState.AddModelError("myUser.Email", "Invalid Email or Password");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                HttpContext.Session.SetString("Username", userInDb.Username);

                return RedirectToAction("BrightIdeas");
            }
            return View("Index");
        }

        [HttpGet("bright_ideas")]
        public IActionResult BrightIdeas()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if(userId == null)
            {
                ModelState.AddModelError("myUser.Email", "Login to continue");
                return View("Index");
            }
            int id = (int)userId;
            ViewBag.User = id;          
            ViewBag.Name = HttpContext.Session.GetString("Username");

            Allideas ideas = new Allideas()
            {
                allIdeas = dbContext.Ideas.Include(idea => idea.Poster)
                .Include(idea => idea.Likes)
                .ThenInclude(Like => Like.User)
                .OrderByDescending(d => d.Likes.Count)
                .ToList()
            };
            return View(ideas);
        }

        [HttpPost("post/ideas")]
        public IActionResult postIdeas(Idea newIdea)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.User = (int)userId;

            string Name = HttpContext.Session.GetString("Username");
            ViewBag.Name = Name;

            if(ModelState.IsValid)
            {
                if(newIdea.UserId == userId)
                {
                    dbContext.Add(newIdea);
                    dbContext.SaveChanges();
                }
                return RedirectToAction("BrightIdeas");
            }
            else
            {
                Allideas ideas = new Allideas()
                {
                    allIdeas = dbContext.Ideas.Include(idea => idea.Poster)
                    .Include(idea => idea.Likes)
                    .ThenInclude(Like => Like.User)
                    .OrderByDescending(d => d.Likes.Count)
                    .ToList()
                };
                return View("BrightIdeas", ideas);
            }
        }

        [HttpGet("bright_ideas/{id}")]
        public IActionResult ThisIdea(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if(userId == null)
            {
                ModelState.AddModelError("myUser.Email", "Login to continue");
                return View("Index");
            }
            ViewBag.User = (int)userId;

            Idea thisIdea = dbContext.Ideas.Include(idea => idea.Poster)
            .Include(idea => idea.Likes)
            .ThenInclude(Like => Like.User)
            .FirstOrDefault(idea => idea.IdeaId == id);

            return View(thisIdea);
        }

        [HttpGet("users/{id}")]
        public IActionResult ThisUser(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if(userId == null)
            {
                ModelState.AddModelError("myUser.Email", "Login to continue");
                return View("Index");
            }
            ViewBag.User = (int)userId;

            User thisUser = dbContext.Users.Where(u => u.UserId == id)
            .Include(i => i.PostedIdeas)
            .Include(i => i.LikedPost).Single();
            
            return View(thisUser);
        }

        [HttpGet("like/{id}")]
        public IActionResult likeIdea(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if(userId == null)
            {
                ModelState.AddModelError("myUser.Email", "Login to continue");
                return View("Index");
            }
            
            Like tolike = new Like()
            {
                UserId = (int)userId,
                IdeaId = id
            };

            dbContext.Add(tolike);
            dbContext.SaveChanges();

            return RedirectToAction("BrightIdeas");
        }

        [HttpGet("destroy/{id}")]
        public IActionResult deleteIdea(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if(userId == null)
            {
                ModelState.AddModelError("myUser.Email", "Login to continue");
                return View("Index");
            }
            
            Idea todelete = dbContext.Ideas.FirstOrDefault(idea => idea.IdeaId == id);
            dbContext.Remove(todelete);
            dbContext.SaveChanges();

            return RedirectToAction("BrightIdeas");
        }

        [HttpGet("/logout")]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }
    }
}