using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class LoginUser
    {
        [Key]
        public int loginuser_id { get; set; }

        public string loginuser_username { get; set; }
        public string loginuser_namesurname { get; set; }
        public string login_password { get; set; }


    }
}
