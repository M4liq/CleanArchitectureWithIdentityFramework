﻿namespace Application.Common.Dtos;

public class IdentityActionResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; }
}