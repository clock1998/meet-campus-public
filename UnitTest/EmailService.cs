//using EmailService;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using System.Threading.Tasks;
//using Template.Db.Auth;
//using Template.WebAPI.Repositories;
//using Xunit;

//public class AuthRepositoryTests
//{
//    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
//    private readonly Mock<IEmailSender> _emailSenderMock;
//    private readonly Mock<IConfiguration> _configurationMock;
//    private readonly Mock<IUrlHelper> _urlHelperMock;
//    private readonly AuthRepository _authRepository;

//    public AuthRepositoryTests()
//    {
//        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
//            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
//        _emailSenderMock = new Mock<IEmailSender>();
//        _configurationMock = new Mock<IConfiguration>();
//        _urlHelperMock = new Mock<IUrlHelper>();
//        _authRepository = new AuthRepository(_configurationMock.Object, _userManagerMock.Object, _emailSenderMock.Object);
//    }

//    [Fact]
//    public async Task SendVerificationEmailAsync_SendsEmailWithCorrectUrl()
//    {
//        // Arrange
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
