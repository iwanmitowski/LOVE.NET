using Microsoft.AspNetCore.Identity;

namespace LoveNet.Identity.Api.Data;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string name)
        : base(name)
    {
    }
}
