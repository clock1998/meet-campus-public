using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure;

namespace WebAPI.Features.CMS.Emails
{
    public class Email : Entity
    {
        public string EmailAdress { get; set; } = string.Empty;
    }
}
