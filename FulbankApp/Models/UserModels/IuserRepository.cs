using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FulbankApp.Models
{
    public interface IuserRepository
    {
        bool AuthenticateUser(NetworkCredential credential);
        void Add(UserClass userModel);
        void Edit(UserClass userModel);
        void Remove(int id);
        UserClass GetById(int id);
        UserClass GetByUsername(string username);
        IEnumerable<UserClass> GetByAll();
    }
}