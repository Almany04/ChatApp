﻿namespace ChatApp.Endpoint.Dtos
{
    public class UserLoginDto
    {
        public required string Email { get; set; } = "";

        public required string Password { get; set; } = "";
    }
}
