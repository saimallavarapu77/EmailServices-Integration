using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Domain.Entities
{

    public class PasswordResetToken
    {
        public int Id { get; set; }          // Primary Key
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
