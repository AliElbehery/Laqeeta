//using Laqeeta.Core.DTOs;
//using Laqeeta.Core.Enums;
//using Laqeeta.Core.Exceptions;
//using Laqeeta.Core.Localization;
//using Microsoft.EntityFrameworkCore;

//namespace Laqeeta.Tests;

//public class AuthServiceTests : IDisposable
//{
//    private readonly Scenario S = new();

//    public void Dispose() => S.Dispose();

//    private static RegisterDto Reg(string username = "ali", string email = "Ali@Test.com", string password = "Passw0rd!")
//        => new("Ali Elbehery", username, email, "01234567890", password);

//    // ------------------------------------------------------------ Register

//    [Fact]
//    public async Task Register_creates_a_passenger_with_a_hashed_password_and_a_token()
//    {
//        var result = await S.Auth().RegisterAsync(Reg(email: "  Ali@Test.com "));

//        Assert.Equal(UserRole.User, result.User.Role);
//        Assert.Null(result.User.StationId);
//        Assert.Equal("ali@test.com", result.User.Email);
//        Assert.Equal($"fake-token-{result.User.UserId}", result.Token);

//        var stored = await S.Read(db => db.Users.SingleAsync(u => u.UserId == result.User.UserId));
//        Assert.NotEqual("Passw0rd!", stored.PasswordHash);
//        Assert.False(string.IsNullOrWhiteSpace(stored.PasswordHash));
//        Assert.True(stored.IsActive);
//    }

//    [Fact]
//    public async Task Register_rejects_a_duplicate_username_or_email()
//    {
//        await S.Auth().RegisterAsync(Reg());

//        var byName = await Assert.ThrowsAsync<ConflictException>(() => S.Auth().RegisterAsync(Reg(email: "other@test.com")));
//        var byEmail = await Assert.ThrowsAsync<ConflictException>(() => S.Auth().RegisterAsync(Reg(username: "ali2")));
//        Assert.Equal(Err.UsernameTaken, byName.Code);
//        Assert.Equal(Err.EmailTaken, byEmail.Code);
//    }

//    [Theory]
//    [InlineData("short1")]          // too short
//    [InlineData("onlyletters")]     // no digit
//    [InlineData("1234567890")]      // no letter
//    public async Task Register_rejects_weak_passwords(string password)
//    {
//        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => S.Auth().RegisterAsync(Reg(password: password)));
//        Assert.Equal(Err.PasswordWeak, ex.Code);
//    }

//    // ------------------------------------------------------------ Login

//    [Fact]
//    public async Task Login_works_with_username_or_email()
//    {
//        await S.Auth().RegisterAsync(Reg());

//        var byName = await S.Auth().LoginAsync(new LoginDto("ali", "Passw0rd!"));
//        var byEmail = await S.Auth().LoginAsync(new LoginDto("ALI@test.com", "Passw0rd!"));

//        Assert.Equal(byName.User.UserId, byEmail.User.UserId);
//        Assert.False(string.IsNullOrEmpty(byName.Token));
//    }

//    [Fact]
//    public async Task Login_gives_the_same_error_for_a_wrong_password_and_an_unknown_user()
//    {
//        await S.Auth().RegisterAsync(Reg());

//        var wrongPassword = await Assert.ThrowsAsync<UnauthorizedException>(() => S.Auth().LoginAsync(new LoginDto("ali", "WrongPass1")));
//        var unknownUser = await Assert.ThrowsAsync<UnauthorizedException>(() => S.Auth().LoginAsync(new LoginDto("nobody", "Passw0rd!")));

//        Assert.Equal(wrongPassword.Message, unknownUser.Message);   // no user enumeration
//        Assert.Equal(Err.InvalidCredentials, wrongPassword.Code);
//        Assert.Equal(Err.InvalidCredentials, unknownUser.Code);
//    }

//    [Fact]
//    public async Task Login_is_forbidden_for_a_disabled_account_but_only_with_the_right_password()
//    {
//        var registered = await S.Auth().RegisterAsync(Reg());
//        await S.SetActiveAsync(registered.User.UserId, false);

//        await Assert.ThrowsAsync<ForbiddenException>(() => S.Auth().LoginAsync(new LoginDto("ali", "Passw0rd!")));
//        await Assert.ThrowsAsync<UnauthorizedException>(() => S.Auth().LoginAsync(new LoginDto("ali", "WrongPass1")));
//    }

//    // ------------------------------------------------------------ Change password

//    [Fact]
//    public async Task ChangePassword_replaces_the_password()
//    {
//        var registered = await S.Auth().RegisterAsync(Reg());

//        await S.Auth().ChangePasswordAsync(registered.User.UserId, new ChangePasswordDto("Passw0rd!", "NewPassw0rd!"));

//        await Assert.ThrowsAsync<UnauthorizedException>(() => S.Auth().LoginAsync(new LoginDto("ali", "Passw0rd!")));
//        await S.Auth().LoginAsync(new LoginDto("ali", "NewPassw0rd!"));
//    }

//    [Fact]
//    public async Task ChangePassword_validates_the_current_and_the_new_password()
//    {
//        var registered = await S.Auth().RegisterAsync(Reg());
//        var id = registered.User.UserId;

//        await Assert.ThrowsAsync<UnauthorizedException>(() => S.Auth().ChangePasswordAsync(id, new ChangePasswordDto("wrong", "NewPassw0rd!")));
//        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Auth().ChangePasswordAsync(id, new ChangePasswordDto("Passw0rd!", "weak")));
//        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Auth().ChangePasswordAsync(id, new ChangePasswordDto("Passw0rd!", "Passw0rd!")));
//        await Assert.ThrowsAsync<NotFoundException>(() => S.Auth().ChangePasswordAsync(999, new ChangePasswordDto("a", "NewPassw0rd!")));
//    }

//    // ------------------------------------------------------------ Employees / profile

//    [Fact]
//    public async Task CreateEmployee_creates_an_employee_assigned_to_the_station()
//    {
//        var dto = new CreateEmployeeDto("New Employee", "emp.new", "emp.new@test.com", "01234567890", "Passw0rd!", S.AttabaId);

//        var user = await S.Auth().CreateEmployeeAsync(dto);

//        Assert.Equal(UserRole.Employee, user.Role);
//        Assert.Equal(S.AttabaId, user.StationId);
//    }

//    [Fact]
//    public async Task CreateEmployee_rejects_an_unknown_station()
//    {
//        var dto = new CreateEmployeeDto("New Employee", "emp.new", "emp.new@test.com", "01234567890", "Passw0rd!", 999);

//        await Assert.ThrowsAsync<NotFoundException>(() => S.Auth().CreateEmployeeAsync(dto));
//    }

//    [Fact]
//    public async Task GetProfile_returns_the_user_or_throws()
//    {
//        var profile = await S.Auth().GetProfileAsync(S.OmarId);
//        Assert.Equal("omar", profile.Username);

//        await Assert.ThrowsAsync<NotFoundException>(() => S.Auth().GetProfileAsync(999));
//    }
//}