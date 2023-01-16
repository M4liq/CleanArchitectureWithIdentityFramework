﻿namespace Infrastructure.Settings;

public class SmtpClientSettings
{
    public string Host { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
}