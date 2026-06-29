using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Domain.Entities;

public class EmailOtp
{
    public int Id { get; set; }


    public int UserId { get; set; }

    public string OtpCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
