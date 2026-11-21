namespace MiloShop.Web.ViewModels.Auth
{
    public class LoginVM
    {
        public string Identifier { get; set; } = ""; // email یا username
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }  // اگر نیاز داری
    }

}
