namespace LOVE.NET.Common
{
    using System.IO;
    using static System.Net.WebRequestMethods;

    public static class GlobalConstants
    {
        public const string SystemName = "LOVE.NET";

        public const string AdministratorRoleName = "Administrator";

        public const string Data = "Data";

        public const string JWT = "JWT";

        public const string AuthSettingsKey = "AuthSettings:Key";

        public const string AuthSettingsIssuer = "AuthSettings:Issuer";

        public const string AuthSettingsAudience = "AuthSettings:Audience";

        public const string SendGridApiKey = "SendGrid:ApiKey";

        public const string CloudinaryCloudName = "Cloudinary:CloudName";

        public const string CloudinaryKey = "Cloudinary:Key";

        public const string CloudinarySecret = "Cloudinary:Secret";

        public const string RefreshTokenValue = "refreshToken";

        public const string Error = "Error";

        public const string Unauthorized = "Unauthorized";

        public const int CitiesMaxCountInDb = 42905;

        public const int CountriesMaxCountInDb = 239;

        public const int GendersMaxCountInDb = 3;

        public const int MinimalAge = 18;

        public const int MaxFileSizeInBytes = 20 * 1024 * 1024;

        public const string DefaultProfilePictureUrl = "https://res.cloudinary.com/dojl8gfnd/image/upload/v1666714042/24-248253_user-profile-default-image-png-clipart-png-download_tuamuf.png";

        public class JWTSecurityScheme
        {
            public const string JWTScheme = "bearer";
            public const string JWTName = "JWT Authentication";
            public const string JWTDescription = "Put **_ONLY_** your JWT Bearer token on textbox below!";
        }

        public class FilePaths
        {
            public const string OneDirectoryUp = "..";

            public const string SystemNameData = $"{SystemName}.{Data}";

            public const string Files = "Files";

            public const string CitiesWithCountriesFileName = "CitiesWithCountries.csv";

            public const string CountriesFileName = "Countries.csv";

            public const string UsersFileName = "Users.json";
        }

        public class ControllerRoutesConstants
        {
            public const string Api = "api/";
            public const string HeaderOrigin = "origin";

            public const string IdentityControllerName = Api + "identity";
            public const string RegisterRoute = "register";
            public const string LoginRoute = "login";
            public const string LogoutRoute = "logout";
            public const string RefreshTokenRoute = "refreshToken";
            public const string GetAccountRoute = "account/{id:guid}";

            public const string EmailControllerName = Api + "email";
            public const string VerifyEmailRoute = "verify";
            public const string ResendEmailConfirmationLinkRoute = "resendEmailConfirmationLink";

            public const string CountriesControllerName = Api + "countries";
            public const string ById = "{id:int}";

            public const string GendersControllerName = Api + "genders";
        }

        public class ControllerResponseMessages
        {
            public const string EmailAlreadyInUse = "Email already in use";
            public const string PasswordsDontMatch = "Password and confirm password must match";
            public const string UnderagedUser = "You need to be 18 years old to register";
            public const string LogoutSuccessfully = "Logout successfully";
            public const string FillAllTheInformation = "Fill all the information";
            public const string UserNotFound = "User not found";
            public const string WrongPassword = "Wrong password";
            public const string WrongOldPassword = "Old password is invalid";
            public const string ChangePasswordsDontMatch = "New password and confirm password must match";
            public const string PasswordChangedSuccessfully = "Password changed successfully";
            public const string InvalidCity = "Invalid city";
            public const string InvalidCountry = "Invalid country";
            public const string UnsupportedFileType = "Unsupported file type";
            public const string MaxFileSizeReached = "Max file size reached";
            public const string InvalidGender = "Invalid gender";
        }

        public class EmailMessagesConstants
        {
            public const string IncorrectEmail = "Your email is incorrect";
            public const string EmailConfirmed = "Email confirmed - you can now login";
            public const string EmailDoesntMatch = "This is not the email you registered with";
            public const string EmailNotConfirmed = "Please, check your email and verify your account";
            public const string EmailAlreadyVerified = "Your email is already verified";
        }

        public class EmailSenderConstants
        {
            public const string FromEmail = "parentassistantapi@abv.bg";
            public const string FromName = "ParentAssistantApi";
            public const string EmailSubject = "Verify Email";
            public const string VerifyUrl = "{0}/{1}?token={2}&email={3}";
            public const string Templates = "templates";
            public const string Email = "email";
            public const string VerifyHtml = "verify.html";
        }

        public class GenderConstants
        {
            public const string Male = "Male";
            public const string Female = "Female";
            public const string Trans = "Trans";
        }
    }
}
