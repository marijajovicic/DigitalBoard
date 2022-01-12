using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Threading.Tasks;
using DigitalBoard.Helper;
using System.Text;

namespace DigitalBoard.Controllers
{
    public class PostController : Controller
    {
        private readonly IDriver _driver;

        public PostController(IDriver driver)
        {
            _driver = driver;
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(string content)
        {
            int? userId = SessionHelper.GetUserId(HttpContext.Session);
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session) || userId is null)
                return RedirectToAction("Login", "User");

            var statementText = new StringBuilder();
            statementText.Append(@$"MATCH (u: User) where id(u) = {userId}
                                    CREATE (u)-[:POSTED]->(post:Post {{content: '{content.Replace("'", "")}', creationDate: '{DateTime.Now}'}})");

            var session = _driver.AsyncSession();
            var result = await session.WriteTransactionAsync(tx => tx.RunAsync(statementText.ToString()));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string content, int postId)
        {
            int? userId = SessionHelper.GetUserId(HttpContext.Session);
            if(SessionHelper.IsUsernameEmpty(HttpContext.Session) || userId is null)
                return RedirectToAction("Login");

            var statementText = new StringBuilder();
            statementText.Append(@$"
                MATCH (user:User), (post:Post) WHERE id(user)= {userId} AND id(post)= {postId}
                CREATE (user)-[x:COMMENTED {{content: '{content.Replace("'", "")}', submitDate: '{DateTime.Now}' }}]->(post)");

            var session = _driver.AsyncSession();
            var result = await session.WriteTransactionAsync(tx => tx.RunAsync(statementText.ToString()));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(string content, int commentId)
        {
            if(SessionHelper.IsUsernameEmpty(HttpContext.Session))
                return RedirectToAction("Login");

            var statementText = $"MATCH ()-[c]->() WHERE id(c) = {commentId} SET c.content = \"{content}\"";

            var session = _driver.AsyncSession();
            var result = await session.WriteTransactionAsync(tx => tx.RunAsync(statementText));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            int? userId = SessionHelper.GetUserId(HttpContext.Session);
            if(SessionHelper.IsUsernameEmpty(HttpContext.Session) || userId is null)
                return RedirectToAction("Login");

            var statementText = new StringBuilder();
            statementText.Append($"MATCH (u)-[com]->() WHERE id(com)={commentId} and id(u)={userId} DELETE com");
           
            var session = this._driver.AsyncSession();
            var result = await session.WriteTransactionAsync(tx => tx.RunAsync(statementText.ToString()));
            return RedirectToAction("Index", "Home");
        }
    }
}
