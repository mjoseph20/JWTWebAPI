﻿namespace JWTAuthServer.DTOs
{
    public class ProfileDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; }
    }
}
