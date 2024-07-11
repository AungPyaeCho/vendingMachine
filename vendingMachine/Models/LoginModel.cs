using System.ComponentModel.DataAnnotations;

namespace vendingMachine.Models
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        public bool RememberMe { get; set; }
    }
}
