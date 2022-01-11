﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DigitalBoard.Models;
using DigitalBoard.Helper;

namespace DigitalBoard.Controllers
{
    public class UserController : Controller
    {
        private readonly IDriver _driver;

        public UserController(IDriver driver)
        {
            _driver = driver;
        }

        public IActionResult Login()
        {
            if(!SessionHelper.IsUsernameEmpty(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            return View();
        }

        public IActionResult Signup()
        {
            if(!SessionHelper.IsUsernameEmpty(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            return View();
        }

       [HttpPost]
       public async Task<IActionResult> Login(string username, string password)
        {
            if(!SessionHelper.IsUsernameEmpty(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            IResultCursor result;
            int userId = -1;
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                result = await session.RunAsync($"MATCH (u:User {{username: '{username}', password: '{password}'}}) RETURN id(u)");

                var res = await result.ToListAsync();
                if(res.Count == 0)
                    return RedirectToAction("Login", "Home");

                userId = res[0]["id(u)"].As<int>();
                    
                if (userId != -1)
                {
                    SessionHelper.SetUsername(HttpContext.Session, username);
                    SessionHelper.SetUserId(HttpContext.Session, userId);
                    return RedirectToAction("Index", "Home");
                }
            }
            finally
            {
                await session.CloseAsync();
            }
            
            return RedirectToAction("Login", "Home");
        }
        
        [HttpPost]
        public async Task<IActionResult> Signup( string username,string password, string email)
        {
            if(!SessionHelper.IsUsernameEmpty(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            var statementText = new StringBuilder();
            statementText.Append($"CREATE (user:User {{username: '{username}', password:  '{password}', email:  '{email}' }}) return id(user)");

            IResultCursor result;
            int userId = -1;
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                result = await session.RunAsync(statementText.ToString());
                userId = await result.SingleAsync(record => record["id(user)"].As<int>());

                if (userId != -1)
                {
                    SessionHelper.SetUsername(HttpContext.Session, username);
                    SessionHelper.SetUserId(HttpContext.Session, userId);
                    return RedirectToAction("Index", "Home");
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return RedirectToAction("Register", "Home");
        }


        public async Task<IActionResult> FollowUser(int userToFollowId)
        {
            int? userId = SessionHelper.GetUserId(HttpContext.Session);
            if(SessionHelper.IsUsernameEmpty(HttpContext.Session) || userId is null)
                return RedirectToAction("Login");
            
            var statementText = new StringBuilder();
            statementText.Append(@$"MATCH(u:User) WHERE id(u)={userId} 
                                    MATCH (uu:User) WHERE id(uu)={userToFollowId} 
                                    CREATE (u)-[:FOLLOW]->(uu)");
            var session = _driver.AsyncSession();
            var result = await session.WriteTransactionAsync(tx => tx.RunAsync(statementText.ToString()));
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> UnfollowUser(int userToUnfollowId)
        {
            int? userId = SessionHelper.GetUserId(HttpContext.Session);
            if(SessionHelper.IsUsernameEmpty(HttpContext.Session) || userId is null)
                return RedirectToAction("Login");

            var statementText = new StringBuilder();
            statementText.Append(@$"MATCH(u:User) WHERE id(u)={userId}
                                    MATCH (uu:User) WHERE id(uu)={userToUnfollowId} 
                                    MATCH (u)-[x:FOLLOW]->(uu) DELETE x");
            var session = _driver.AsyncSession();
            var result = await session.WriteTransactionAsync(tx => tx.RunAsync(statementText.ToString()));
            return RedirectToAction("Index", "Home"); 
        }
    }
}
