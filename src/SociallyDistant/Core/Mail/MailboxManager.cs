using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.WorldObjects;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Rendering;

namespace SociallyDistant.Core.Mail
{
    public class MailboxManager : ISystem
    {
        private const double FlushInterval = 5;
        private double _flush = 0;
        private Scene _scene;
        private Dictionary<string, Queue<DeliveryRequest>> _deliverQueues = new();

        public void Init(Scene scene)
        {
            _scene = scene;
        }

        public void Unload()
        {
        }

        public void Load()
        {
            // Find all agents.
            var agents = _scene.Registry.View<AgentData>();
            
            // Loop through them.
            foreach (var entity in agents)
            {
                // Get the agent data.
                var agent = _scene.Registry.GetComponent<AgentData>(entity);
                
                // Create a mailbox.
                var mailbox = new Mailbox(agent.UserName + "@test.xyz");
                
                // Attach it to the entity.
                _scene.Registry.AddComponent(entity, mailbox);

            }

            RestoreMailFromSave();
        }

        public void Update(GameTime gameTime)
        {
            // Retrieve all entities with a mailbox on them.
            var mailboxEntities = _scene.Registry.View<Mailbox>();
            
            // Loop through them.
            foreach (var entity in mailboxEntities)
            {
                // This retrieves a reference to the entity's mailbox.
                var mailbox = _scene.Registry.GetComponent<Mailbox>(entity);
                
                // Try to get an outbox request.
                if (mailbox.TryGetOutboxRequest(out var outboxRequest))
                {
                    AddToDeliverQueue(mailbox.Address, outboxRequest);
                }
                
                // Check if there is a delivery queue for this mailbox.
                if (_deliverQueues.ContainsKey(mailbox.Address))
                {
                    // Grab it.
                    var queue = _deliverQueues[mailbox.Address];
                    
                    // Dequeue it.
                    if (queue.TryDequeue(out var req))
                    {
                        // Dispatch it.
                        mailbox.Receive(req.From, req.Message, true);
                        
                        // Save it.
                        SaveEmail(req.From, req.Message);
                    }
                    else
                    {
                        // No message in the queue so we can safely
                        // destroy it.
                        _deliverQueues.Remove(mailbox.Address);
                    }
                }
            }

            _flush += gameTime.ElapsedGameTime.TotalSeconds;

            if (_flush >= FlushInterval)
            {
                FlushDeliveryQueue();
                _flush = 0;
            }
        }

        private void FlushDeliveryQueue()
        {
            var mailboxes = _scene.Registry.View<Mailbox>().Select(x => _scene.Registry.GetComponent<Mailbox>(x))
                .ToDictionary(x => x.Address);

            foreach (var key in _deliverQueues.Keys.ToArray())
            {
                if (!mailboxes.ContainsKey(key))
                {
                    while (_deliverQueues[key].TryDequeue(out var req))
                    {
                        if (mailboxes.ContainsKey(req.From))
                        {
                            var sender = mailboxes[req.From];

                            var failMessage = sender.CreateMessage(sender.Address, "Mail delivery issue");
                            failMessage.Email.AddParagraph(
                                $"Your message sent to {req.Message.ToAddress} could not be delivered. Check that the destination address is correct and try again.");
                        }
                    }
                    _deliverQueues.Remove(key);
                }
            }
        }
        
        private void SaveEmail(string from, EmailRequest message)
        {
            var saveManager = _scene.Game.GetComponent<SaveManager>();
            
            var savedEmail = new SavedEmail();
            savedEmail.Id = message.Email.Id;
            savedEmail.Subject = message.Email.Subject;
            savedEmail.From = from;
            savedEmail.To = message.ToAddress;

            foreach (var paragraph in message.Email.Paragraphs)
            {
                var p = new SavedEmailParagraph();
                p.Text = paragraph.Text;

                if (paragraph.Image != null)
                {
                    var fname = Guid.NewGuid().ToString() + ".png";
                    var path = Path.Combine(saveManager.GameFolder, "attachments", fname);
                    var assetPath = "/attachments/" + path;
                    
                    using (var stream = File.Open(path, FileMode.OpenOrCreate))
                    {
                        paragraph.Image.SavePng(stream);
                        stream.Close();
                    }

                    p.ImagePath = assetPath;
                }

                savedEmail.Paragraphs.Add(p);
            }

            if (message.ReplyTo == null)
            {
                var convo = new SavedEmailConversation();
                convo.Email = savedEmail;
                saveManager.CurrentGame.Emails.Add(convo);
            }
            else
            {
                var e = message.ReplyTo;
                while (e.ReplyTo != null)
                    e = e.ReplyTo;

                var convo = saveManager.CurrentGame.Emails.First(x => x.Email.Id == e.Id);
                convo.Replies.Add(savedEmail);
            }

            saveManager.Save();
        }

        public void Render(GameTime gameTime)
        {
        }

        private void AddToDeliverQueue(string from, EmailRequest req)
        {
            // Add a delivery queue for the destination mail address
            // if there is none already.
            if (!_deliverQueues.ContainsKey(req.ToAddress))
                _deliverQueues.Add(req.ToAddress, new Queue<DeliveryRequest>());
            
            // Queue the message for delivery to this address.
            _deliverQueues[req.ToAddress].Enqueue(new DeliveryRequest(from, req));

            // Reset the flush timer so that we do not accidentally reject delivery.
            _flush = 0;
        }

        private class DeliveryRequest
        {
            public EmailRequest Message { get; }
            public string From { get; }

            public DeliveryRequest(string from, EmailRequest req)
            {
                From = from;
                Message = req;
            }
        }

        private void RestoreMailFromSave()
        {
            var saveManager = _scene.Game.GetComponent<SaveManager>();
            var save = saveManager.CurrentGame;

            foreach (var mailboxEntity in _scene.Registry.View<Mailbox>())
            {
                var mailbox = _scene.Registry.GetComponent<Mailbox>(mailboxEntity);

                var conversations = save.Emails.Where(x => x.Email != null && x.Email.To == mailbox.Address);

                foreach (var conversation in conversations)
                {
                    var email = new Email(conversation.Email.Subject);
                    email.Id = conversation.Email.Id;

                    foreach (var paragraph in conversation.Email.Paragraphs)
                    {
                        var realParagraph = email.AddParagraph(paragraph.Text);

                        if (AssetManager.TryLoadImage(paragraph.ImagePath, out var texture))
                        {
                            realParagraph.Image = texture;
                        }
                    }

                    var request = new EmailRequest(conversation.Email.To, email);

                    mailbox.Receive(conversation.Email.From, request, false);

                    foreach (var cReply in conversation.Replies)
                    {
                        var reply = new Email(cReply.Subject);
                        reply.Id = cReply.Id;

                        foreach (var rPara in cReply.Paragraphs)
                        {
                            var para = reply.AddParagraph(rPara.Text);

                            if (AssetManager.TryLoadImage(rPara.ImagePath, out var texture))
                            {
                                para.Image = texture;
                            }
                        }

                        var replyRequest = new EmailRequest(cReply.To, reply);

                        mailbox.Receive(cReply.From, replyRequest, false);
                    }
                }
            }
        }
    }

    public class InboxEmail
    {
        public string  Subject { get; }
        public EmailParagraph[] Paragraphs { get; }
        public string From { get; }
        public string To { get; }
        public DateTime Received { get; }

        public InboxEmail(string from, Email message)
        {
            From = from;
            Paragraphs = message.Paragraphs.ToArray();
            Subject = message.Subject;
            Received = DateTime.Now;
        }
    }

    public class Mailbox
    {
        private Queue<UnreadEmail> _unread = new();
        private List<InboxEmail> _inbox = new();
        private ConcurrentQueue<EmailRequest> _outbox = new();
        private string _address;

        public IEnumerable<InboxEmail> Inbox => _inbox.OrderByDescending(x => x.Received);
        
        public string Address => _address;

        public Mailbox(string address)
        {
            _address = address;
        }

        public EmailRequest CreateMessage(string to, string subject)
        {
            var email = new Email(subject);

            var outboxRequest = new EmailRequest(to, email);

            this._outbox.Enqueue(outboxRequest);

            return outboxRequest;
        }

        public bool TryGetOutboxRequest(out EmailRequest req)
        {
            return _outbox.TryDequeue(out req);
        }

        public void Receive(string from, EmailRequest req, bool makeUnread)
        {
            // If the request has a reply-to attached to it, then
            // we will simply add the received email as a reply. No 
            // need to update the inbox.
            if (req.ReplyTo != null)
            {
                req.ReplyTo.AddReply(req.Email);
            }
            else
            {
                // This is where we add to the inbox.
                var inboxMessage = new InboxEmail(from, req.Email);
                _inbox.Add(inboxMessage);
            }
            
            // Add the email to the unread list.
            if (makeUnread)
            {
                var unread = new UnreadEmail(from, req.Email);
                _unread.Enqueue(unread);
            }
        }

        public bool TryGetUnreadMessage(out UnreadEmail unread)
        {
            return _unread.TryDequeue(out unread);
        }
        
        public int UnreadCount => _unread.Count;
        public bool HasUnread => _unread.Any();
    }

    public class UnreadEmail
    {
        public string From { get; }
        public Email Message { get; }

        public UnreadEmail(string from, Email message)
        {
            From = from;
            Message = message;
        }
    }

    public class EmailRequest
    {
        public Email Email { get; }
        public Email ReplyTo { get; }
        public string ToAddress { get; }

        public EmailRequest(string to, Email email)
        {
            this.ToAddress = to;
            this.Email = email;
        }
    }
    
    public class Email
    {
        private Email _replyTo;
        private List<Email> _replies = new();

        public Email ReplyTo => _replyTo;
        public Guid Id { get; internal set; } = Guid.NewGuid();
        
        public string Subject { get; }
        public List<EmailParagraph> Paragraphs { get; } = new();

        public IReadOnlyList<Email> Replies => _replies;

        public Email(string subject)
        {
            this.Subject = subject;
        }
        
        public EmailParagraph AddParagraph(string text)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(text));

            var p = new EmailParagraph(text);
            
            this.Paragraphs.Add(p);

            return p;
        }

        public void AddReply(Email email)
        {
            Debug.Assert(email != null);
            Debug.Assert(!this._replies.Contains(email));

            email._replyTo = this;
            _replies.Add(email);
        }
    }

    public class EmailParagraph
    {
        public string Text { get; }
        public Texture2D Image { get; set; }
        
        public Dictionary<string, Action> ActionLinks { get; } = new();

        public EmailParagraph(string text)
        {
            Text = text;
        }

        public void AddLink(string linkText, Action action)
        {
            Debug.Assert(!this.ActionLinks.ContainsKey(linkText));
            Debug.Assert(action != null);

            this.ActionLinks.Add(linkText, action);
        }
    }
}