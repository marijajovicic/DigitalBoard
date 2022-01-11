using DigitalBoard.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalBoard.Helper;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DigitalBoard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDriver _driver;

        public HomeController(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<IActionResult> IndexAsync()
        {
            if(SessionHelper.IsUsernameEmpty(HttpContext.Session))
                return RedirectToAction("Login", "User");

            ViewData["userId"] = SessionHelper.GetUserId(HttpContext.Session);

            string cypherQuery = @"MATCH (u:User)-[:POSTED]->(p:Post)<-[c:COMMENTED]-(uwc:User) return u, p, c, uwc
                                    UNION
                                    MATCH (u:User)-[:POSTED]->(p:Post) return u, p, null as c, null as uwc";

            IList<Post> postsResponse = new List<Post>();
            IResultCursor result;
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                result = await session.RunAsync(cypherQuery);
                var records = await result.ToListAsync();
                records.ForEach(rec =>
                {
                    INode userNode = rec["u"].As<INode>();
                    INode postNode = rec["p"].As<INode>();
                    INode userWhoCommentedNode = rec["uwc"].As<INode>();
                    IRelationship commentRelationship = rec["c"].As<IRelationship>();

                    Post postAlreadyExists = postsResponse.FirstOrDefault(postModel => postModel.Id == postNode.Id);
                    var index = postsResponse.IndexOf(postAlreadyExists);

                    Comment comment = getComment(userWhoCommentedNode, commentRelationship);

                    if (postAlreadyExists != null)
                    {
                        if (comment != null)
                        {
                            postsResponse[index].Comments.Add(comment);
                        }
                    }
                    else
                    {
                        Post post = createPost(userNode, postNode, comment); 
                        postsResponse.Add(post);
                    }
                });
            }
            finally
            {
                await session.CloseAsync();
            }


            return View(postsResponse.OrderByDescending(post => post.CreationDate).ToList());
        }

        private static Post createPost(INode userNode, INode postNode, Comment comment)
        {
            Post post = new()
            {
                Id = (int)postNode.Id,
                Content = postNode.Properties["content"].As<string>(),
                CreationDate = postNode.Properties["creationDate"].As<DateTime>(),
                User = new User
                {
                    Id = (int)userNode.Id,
                    Username = userNode.Properties["username"].As<string>(),
                },
            };

            if (comment is not null)
            {
                post.Comments.Add(comment);
            }

            return post;
        }

        private Comment getComment(INode userWhoCommented, IRelationship comment)
        {
            if (comment is null)
                return null;

            return new Comment()
            {
                Id = (int)comment.Id,
                Content = comment["content"].As<string>(),
                SubmitDate = comment["submitDate"].As<DateTime>(),
                Submiter = new User()
                {
                    Id = (int)userWhoCommented.Id,
                    Username = userWhoCommented.Properties["username"].As<string>()
                }
            }; 
        }
    }
}
