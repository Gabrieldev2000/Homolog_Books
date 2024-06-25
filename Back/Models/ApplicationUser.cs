using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;


    public class ApplicationUser : IdentityUser
    {
        public ICollection<Book> Books { get; set; }
    }
