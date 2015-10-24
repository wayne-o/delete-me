using System;

namespace Conference.Api.Public.Models
{
    public class User
    {
        public int Auth { get; set; }

        public int AvatarImage { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public bool God { get; set; }

        public string Bio { get; set; }

        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}