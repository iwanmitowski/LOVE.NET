using LOVE.NET.Data.Models;
using LOVE.NET.Services.Email;
using LOVE.NET.Services.Genders;
using LOVE.NET.Services.Messaging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

using Moq;

using Newtonsoft.Json.Linq;

namespace LOVE.NET.Services.Tests
{
    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;
    using static LOVE.NET.Common.GlobalConstants.EmailMessagesConstants;
    using static LOVE.NET.Common.GlobalConstants.EmailSenderConstants;

    public class EmailServiceTests : TestsSetUp
    {
        private IEmailService emailService;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private IEmailSender emailSender;
        private IWebHostEnvironment environment;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();

            userManagerMock = GetUserManagerMock();
            emailSender = new Mock<SendGridEmailSender>(MockToken).Object;
            var environmentMock = new Mock<IWebHostEnvironment>();

            environmentMock.Setup(x =>
                x.WebRootPath)
                .Returns(MockPath);

            environment = environmentMock.Object;

            emailService = new EmailService(
                userManagerMock.Object,
                emailSender,
                environment);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public async Task SuccessSendEmailConfirmation()
        {
            var user = dbContext.Users.FirstOrDefault();

            Assert.DoesNotThrowAsync(async () => await emailService.SendEmailConfirmationAsync("origin", user));
        }

        [Test]
        public async Task SuccessVerifyEmail()
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == "6666666");

            var result = await emailService.VerifyEmailAsync(user.Email, MockToken);

            var updatedUser = dbContext.Users.FirstOrDefault(u => u.Id == user.Id);

            Assert.True(result.Succeeded);
            Assert.True(updatedUser.EmailConfirmed);
            Assert.False(updatedUser.LockoutEnabled);
        }

        [Test]
        public async Task FailVerifyEmailUnauthorized()
        {
            var result = await emailService.VerifyEmailAsync("66666", MockToken);

            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(Unauthorized));
        }

        [Test]
        public async Task FailVerifyEmailIncorrectEmail()
        {
            userManagerMock.Setup(x =>
               x.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

            var user = dbContext.Users.FirstOrDefault();

            var result = await emailService.VerifyEmailAsync(user.Email, MockToken);

            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(IncorrectEmail));
        }

        [Test]
        public async Task SuccessResendEmailConfirmationLink()
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == "6666666");

            var result = await emailService.ResendEmailConfirmationLinkAsync(user.Email, "origin");
            Assert.True(result.Succeeded);
        }

        [Test]
        public async Task FailResendEmailConfirmationLinkChangedEmailInTheUrl()
        {
            var result = await emailService.ResendEmailConfirmationLinkAsync("changedEmail", "origin");
            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(EmailDoesntMatch));
        }

        [Test]
        public async Task FailResendEmailConfirmationLinkAlreadyVerified()
        {
            var user = dbContext.Users.FirstOrDefault(x => !x.LockoutEnabled);

            var result = await emailService.ResendEmailConfirmationLinkAsync(user.Email, "origin");
            Assert.That(result.Errors.Count(), Is.GreaterThan(0));
            Assert.True(result.Errors.Contains(EmailAlreadyVerified));
        }
    }
}
