using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace QsoManager.Infrastructure.Identity;

public class ApplicationUser : MongoIdentityUser<string>
{
    public ApplicationUser() : base()
    {
    }

    public ApplicationUser(string userName, string email) : base(userName, email)
    {
    }
}

public class ApplicationRole : MongoIdentityRole<string>
{
    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}
