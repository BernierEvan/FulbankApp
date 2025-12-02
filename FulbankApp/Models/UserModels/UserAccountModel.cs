using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models
{
    public class UserAccountModel
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}