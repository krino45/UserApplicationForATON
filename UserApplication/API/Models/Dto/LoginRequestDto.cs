﻿namespace UserApplication.API.Models.Dto
{
    public class LoginRequestDto
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }
}