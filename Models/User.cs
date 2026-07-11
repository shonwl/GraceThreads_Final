using System;

namespace GraceThreads.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        /// <summary>
        /// 0 = admin, 1 = customer
        /// </summary>
        public byte Role { get; set; } = 1;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastLoginAt { get; set; }
        public byte[]? RowVersion { get; set; }
        // Navigation
        public List<Order> Orders { get; set; } = new();
    }
}
