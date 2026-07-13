namespace LoveNet.Identity.Api;

/// <summary>User-facing messages ported from the monolith so response bodies stay identical.</summary>
public static class IdentityMessages
{
    public const string EmailAlreadyInUse = "Email already in use";
    public const string PasswordsDontMatch = "Password and confirm password must match";
    public const string UnderagedUser = "You need to be 18 years old to register";
    public const string FillAllTheInformation = "Fill all the information";
    public const string UserNotFound = "User not found";
    public const string WrongPassword = "Wrong password";
    public const string BannedUntil = "You are banned until {0}";
    public const string InvalidGender = "Invalid gender";
    public const string InvalidCity = "Invalid city";
    public const string InvalidCountry = "Invalid country";
    public const string UserCouldNotBeBanned = "User could not be banned";
    public const string CantBanUserInThePast = "Can't ban user in the past";
    public const string CantBanYourself = "You can't ban yourself";

    public const string IncorrectEmail = "Your email is incorrect";
    public const string EmailConfirmed = "Email confirmed - you can now login";
    public const string CheckYourEmail = "Check your email";
    public const string PasswordResetSuccessful = "Password reset is successful - you can now login";
    public const string EmailDoesntMatch = "This is not the email you registered with";
    public const string EmailNotConfirmed = "Please, check your email and verify your account";
    public const string EmailAlreadyVerified = "Your email is already verified";

    // Reference-data bounds (owned by Profile; used here only for cheap range checks).
    public const int CitiesMaxCountInDb = 42905;
    public const int CountriesMaxCountInDb = 239;
    public const int GendersMaxCountInDb = 3;
}
