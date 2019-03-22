using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrightIdeas.Models
{
    public class Idea
    {
        [Key]
        public int IdeaId {get; set;}

        [Required (ErrorMessage = "Whats your Idea?")]
        [MinLength(5, ErrorMessage="Idea must be atleast 5 characters long")]
        public string Post {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public int UserId {get; set;}
        public User Poster {get; set;}
        public List <Like> Likes {get; set;}
    }

    public class Like
    {
        [Key]
        public int LikeId {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public int UserId {get; set;}
        public User User {get; set;}
        public int IdeaId {get; set;}
        public Idea Idea {get; set;}
    }

    public class Allideas
    {
        public List<Idea> allIdeas {get; set;}
        public Idea newIdea {get; set;}
    }
}
