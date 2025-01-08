using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(Dictionary<string, string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();

            To.AddRange(to.Select(x => new MailboxAddress(x.Key, x.Value)));
            Subject = subject;
            Content = content;
        }
    }
}
