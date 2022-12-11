namespace LOVE.NET.Services.Tests
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Email;
    using LOVE.NET.Services.Messaging;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;

    using Moq;


    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.EmailMessagesConstants;

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
        public void SuccessSendEmailConfirmation()
        {
            var user = dbContext.Users.FirstOrDefault();

            Assert.DoesNotThrowAsync(async () => await emailService.SendEmailConfirmationAsync("origin", user));
        }

        [Test]
        public async Task SuccessVerifyEmail()
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == "6666666");

            var result = await emailService.VerifyEmailAsync(user?.Email, MockToken);

            var updatedUser = dbContext.Users.FirstOrDefault(u => u.Id == user.Id);

            Assert.Multiple(() =>
            {
                Assert.That(result.Succeeded, Is.True);
                Assert.That(updatedUser?.EmailConfirmed, Is.True);
                Assert.That(updatedUser?.LockoutEnabled, Is.False);
            });
        }

        [Test]
        public async Task FailVerifyEmailUnauthorized()
        {
            var result = await emailService.VerifyEmailAsync("66666", MockToken);

            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.Errors, Does.Contain(Unauthorized));
        }

        [Test]
        public async Task FailVerifyEmailIncorrectEmail()
        {
            userManagerMock.Setup(x =>
               x.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

            var user = dbContext.Users.FirstOrDefault();

            var result = await emailService.VerifyEmailAsync(user?.Email, MockToken);

            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.Errors, Does.Contain(IncorrectEmail));
        }

        [Test]
        public async Task SuccessResendEmailConfirmationLink()
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == "6666666");

            var result = await emailService.ResendEmailConfirmationLinkAsync(user?.Email, "origin"); 
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task FailResendEmailConfirmationLinkChangedEmailInTheUrl()
        {
            var result = await emailService.ResendEmailConfirmationLinkAsync("changedEmail", "origin");
            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.Errors, Does.Contain(EmailDoesntMatch));
        }

        [Test]
        public async Task FailResendEmailConfirmationLinkAlreadyVerified()
        {
            var user = dbContext.Users.FirstOrDefault(x => !x.LockoutEnabled);

            var result = await emailService.ResendEmailConfirmationLinkAsync(user?.Email, "origin");
            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.Errors, Does.Contain(EmailAlreadyVerified));
        }
    }
}
