using System.ComponentModel.DataAnnotations;

namespace StreamFileUpload.App.Models
{
    public class Profile
    {
        [Required(ErrorMessage = "Name is required field")]
        public string Name { get; set; }
        public int Age { get; set; }
    }
}