﻿using eBlog.Domain.Interfaces;

namespace eBlog.Domain.Entities
{
    public class UserRole : IEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid RoleId { get; set; }
        public Role Role { get; set; }
    }
}
