using System.Xml.Schema;
using SociallyDistant.Core.Social;

namespace SociallyDistant.Core.Commands
{
    public class SendMail : Command
    {
        public override string Name => "sendmail";

        protected override void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine($"{Name}: usage: {Name} <to> <subject> <text>");
                return;
            }

            var to = args[0];
            var subject = args[1];
            var text = args[2];

            Console.WriteLine($"Sending mail to {to}.");
            Console.WriteLine("Subject: {subject}");
            Console.WriteLine();
            Console.WriteLine(text);

            var email = Context.Mailbox.CreateMessage(to, subject);

            email.Email.AddParagraph(text);
            email.Email.AddParagraph("(Sent from my vOS shell).");

            Complete();
        }
    }
}