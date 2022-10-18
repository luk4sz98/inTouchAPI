﻿using System.ComponentModel.DataAnnotations;

namespace inTouchAPI.Dtos;

public class UserRegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public byte Age { get; set; } = default;

    [Required]
    public char Sex { get; set; } = 'M';
}
