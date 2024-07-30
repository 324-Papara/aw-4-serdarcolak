namespace Para.Base.Email;

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public string FromPassword { get; set; }
}