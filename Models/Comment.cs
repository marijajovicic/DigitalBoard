using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalBoard.Models
{
    public class Comment
    {
          public int Id { get; set; }

          public string Content { get; set; } 

          public DateTime SubmitDate { get; set; }

          public User Submiter { get; set; }
    }
}
