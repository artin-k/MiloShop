namespace avatCo.Areas.Auth.ViewModels
{
    public class LoginVM
    {
        public string Identifier { get; set; } = ""; // email یا username
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }  // اگر نیاز داری
    }

}
