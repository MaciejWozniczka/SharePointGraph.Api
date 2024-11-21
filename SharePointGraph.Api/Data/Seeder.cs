namespace SharePointGraph.Api.Data;

public class Seeder
{
    public static async Task SeedUsers(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        User user = await userManager.FindByEmailAsync("maciej.wozniczka@outlook.com");
        if (user == null)
        {
            user = new User()
            {
                Email = "maciej.wozniczka@outlook.com",
                UserName = "maciej.wozniczka@outlook.com",
            };

            var result = await userManager.CreateAsync(user, "<password>");

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Could not create new user in seeder");
            }
        }

        if (!await roleManager.RoleExistsAsync("administrator"))
        {
            var result = await roleManager.CreateAsync(new IdentityRole("administrator"));

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Could not create new role");
            }
        };

        if (!await roleManager.RoleExistsAsync("user"))
        {
            var result = await roleManager.CreateAsync(new IdentityRole("user"));

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Could not create new role");
            }
        };

        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Contains("administrator"))
        {
            await userManager.AddToRoleAsync(user, "administrator");

        };

        if (!userRoles.Contains("user"))
        {
            await userManager.AddToRoleAsync(user, "user");
        }
    }
}