using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspexam.Models
{
    public abstract class BaseEntity {}

    public class User : BaseEntity
        {
            public int id;
            [Required]
            [MinLength(2)]
            [RegularExpression(@"^[a-zA-Z]+$")]
            public string first_name { get; set; }
            [Required]
            [MinLength(1)]
            [RegularExpression(@"^[a-zA-Z]+$")]
            public string last_name { get; set; }
            [Required]
            [EmailAddress]
            public string email{ get; set; }
            [Required]
            [MinLength(3)]
            // [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}")]
            public string password { get; set; }
            [Required]
            [Compare("password")]
            public string confirm_password {get; set;}
            [Required]
            [MinLength(3)]
            public string description{get; set;}
            public DateTime created_at;
            public DateTime updated_at;
        }
        public class Network :  BaseEntity
        {
            public int id;
            [Required]
            [MinLength(4)]
            public DateTime created_at;
            public DateTime updated_at;
            
            //For displaying on views
            public string first_name {get; set;}
            public string last_name {get; set;}
            public int user_id {get; set;}
            public string description {get; set;}
            public int network_id {get; set;}
            public string status {get; set;}
        }
        public class Joiners : BaseEntity
        {
            public int id;
            public int user_id {get; set;}
            public int network_id {get; set;}
        }
}