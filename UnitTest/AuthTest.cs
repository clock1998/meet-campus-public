using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Features.Auth.Command;

namespace Test
{
    public class AuthTest : BaseIntegrationTest
    {
        protected readonly RegisterHandler _registerHandler;
        public AuthTest(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _registerHandler = _scope.ServiceProvider.GetRequiredService<RegisterHandler>();
        }

        [Fact]
        public async Task Register_ShouldCreateNewUser()
        {
            var request = new RegisterRequest("test@mcgill.ca","12345678", "12345678", "test", "test");
            var user = await _registerHandler.HandleAsync(request);
            Assert.NotNull(_context.Users.FirstOrDefault(n => n.Id == user.Id));
        }
    }
}
