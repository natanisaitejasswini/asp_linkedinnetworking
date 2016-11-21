using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExamApp.Factory;
using aspexam.Models;
using CryptoHelper;


namespace aspexam.Controllers
{
    public class ExamController : Controller
    {
        private readonly UserRepository userFactory;
         private readonly ExamRepository examFactory;

        public ExamController()
        {
            userFactory = new UserRepository();
            examFactory = new ExamRepository();
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            return View("Login");
        }
// Post Methods:: Login, Registration
        [HttpPost]
        [Route("registration")]
        public IActionResult Create(User newuser)
        {   
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                 if(userFactory.FindEmail(newuser.email) == null){ // Checking email is registered previously
                    userFactory.Add(newuser);
                    ViewBag.User_Extracting = userFactory.FindByID();
                    int current_other_id = ViewBag.User_Extracting.id;
                    HttpContext.Session.SetInt32("current_id", (int) current_other_id);
                    ///Inserting into Network table
                    examFactory.Add_Network((int)HttpContext.Session.GetInt32("current_id"));
                    ///Inserting the same user into joiners table by extracting last entered network
                    ViewBag.Network_Extracting = examFactory.Network_Last_ID();
                    examFactory.Add_Joiner(ViewBag.Network_Extracting.id, (int)HttpContext.Session.GetInt32("current_id"));
                    return RedirectToAction("Dashboard");
                }
                 else{
                    temp_errors.Add("Email is already in use");
                    TempData["errors"] = temp_errors;
                    return RedirectToAction("Index");
                }
            }
            foreach(var error in ModelState.Values)
            {
                if(error.Errors.Count > 0)
                {
                    temp_errors.Add(error.Errors[0].ErrorMessage);
                }  
            }
            TempData["errors"] = temp_errors;
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Route("login")]
        public IActionResult Login(string email, string password)
        {
            List<string> temp_errors = new List<string>();
            if(email == null)
            {
                temp_errors.Add("Enter Email field to Login");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
            if(password == null)
            {
                temp_errors.Add("Enter password field to Login");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
//Login User Id Extracting query
          User check_user = userFactory.FindEmail(email);
            if(check_user == null)
            {
                temp_errors.Add("Email is not registered");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
            bool correct = Crypto.VerifyHashedPassword((string) check_user.password, password);
            if(correct)
            {
                HttpContext.Session.SetInt32("current_id", check_user.id);
                return RedirectToAction("Dashboard");
            }
            else{
                temp_errors.Add("Password is not matching");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
        }
 //Dashboard start
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            //on refresh once after logout
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
             if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            //Dashboard begins
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            //Accepted Invitations
            ViewBag.Extract_Network_CurrentUser = examFactory.Extract((int)HttpContext.Session.GetInt32("current_id"));
            ViewBag.Other_Users = examFactory.others(ViewBag.Extract_Network_CurrentUser );
            //Invitation box
            ViewBag.User_Except_networks = examFactory.ExceptCurrentUserNetworks((int)HttpContext.Session.GetInt32("current_id"));
            //Accepting Invitation
            return View("Dashboard");
        }
//Accepting Invitation
        [HttpGet]
        [Route("join/{id}")]
        public IActionResult Network_Join(string id = "")
        {
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            examFactory.Join_Network(id,(int)HttpContext.Session.GetInt32("current_id"));
            return RedirectToAction("Dashboard");
        }

//Show
        [HttpGet]
        [Route("show/{id}")]
        public IActionResult Show(string id = "")
        {
            //on refresh once after logout
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            ViewBag.Network_Info = examFactory.Network_Info(id);
            return View("ShowUser");
        }
//Ignore part
        [HttpGet]
        [Route("ignore/{id}")]
        public IActionResult Network_Ignore(string id = "")
        {
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            examFactory.Ignore_Network(id,(int)HttpContext.Session.GetInt32("current_id"));
            return RedirectToAction("Dashboard");
        }
//Ignored Connections
        [HttpGet]
        [Route("users")]
        public IActionResult Ignored()
        {
            //on refresh once after logout
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            ViewBag.Ignored_Networks = examFactory.Ignored((int)HttpContext.Session.GetInt32("current_id"));
            return View("ProfilePage");
        }
// Logout
        [HttpGet]
        [Route("logout")]
         public IActionResult Logout()
         {
             HttpContext.Session.Clear();
             return RedirectToAction("Index");
         }
//Ignore to connect
        [HttpGet]
        [Route("ignoreconnect/{id}")]
        public IActionResult Ignore_Connect(string id = "")
        {
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            examFactory.Delete_Joiner(id,(int)HttpContext.Session.GetInt32("current_id"));
            examFactory.Join_Network(id,(int)HttpContext.Session.GetInt32("current_id"));
            return RedirectToAction("Dashboard");
        }
    }
}
