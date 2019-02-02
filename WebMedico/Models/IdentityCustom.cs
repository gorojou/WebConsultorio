using System;
using System.Security.Principal;

namespace WebMedico.Models
{
    interface ICustomPrincipal : IPrincipal
    {
        int IdDoctor { get; set; }

        string Name { get; set; }

        string LastName { get; set; }

        string Email { get; set; }

    }

    public class CustomPrincipal : ICustomPrincipal
    {
        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }

        public IIdentity Identity { get; }

        public CustomPrincipal(string username)
        {
            Identity = new GenericIdentity(username);
        }

        public int IdDoctor { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool AlwaysLoggedIn { get; set; }
    }

    public class CustomPrincipalSerializedModel
    {
        public int IdDoctor { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool AlwaysLoggedIn { get; set; }
    }
}