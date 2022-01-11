using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalBoard.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public IList<Comment> Comments { get; set; } = new List<Comment>();

        public DateTime CreationDate{get;set;}

        public User User { get; set; }
    }
}
