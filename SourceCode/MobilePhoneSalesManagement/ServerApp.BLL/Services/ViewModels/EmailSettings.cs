﻿namespace ServerApp.BLL.Services.ViewModels
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string FromEmail { get; set; }
        public string SmtpPassword { get; set; }
    }

}
