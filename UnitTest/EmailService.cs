//using EmailService;
//using Microsoft.AspNetCore.Authentication.OAuth;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using System.Threading.Tasks;
//using WebAPI.Features.Auth;
//using Xunit;

//public class AuthRepositoryTests
//{
//    private readonly Mock<IEmailSender> _emailSenderMock;
//    private readonly Mock<IConfiguration> _configurationMock;
//    private readonly Mock<IUrlHelper> _urlHelperMock;
//    [Fact]
//    public async Task SendVerificationEmailAsync_SendsEmailWithCorrectUrl()
//    {
//        // Arrange
//        var message = new Message(new Dictionary<string, string> { { appUser.UserName, appUser.Email } }, "Email Verification", verificationUrl);
//        _emailSenderMock.SendEmail(message, "Meet Campus");
//        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "testuser", Email = "jianglinyi97@gmail.com" };
//        var token = "test-token";
//        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync(token);
//        _urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("https://test.com/verify-email");

//        // Act
//        await _authRepository.SendVerificationEmailAsync(user, _urlHelperMock.Object, "https");

//        // Assert
//        _emailSenderMock.Verify(x => x.SendEmail(It.Is<EmailService.Message>(m =>
//            m.To.Equals(user.Email) &&
//            m.Subject == "Email Verification" &&
//            m.Content.Contains("https://test.com/verify-email")), "Campus Meet"), Times.Once);
//    }
//}
