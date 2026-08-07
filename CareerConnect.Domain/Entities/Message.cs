using System;

namespace CareerConnect.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    
    public Guid ApplicationId { get; set; }
    public Application? Application { get; set; }

    public Guid SenderUserId { get; set; }
    public User? Sender { get; set; }

    public Guid ReceiverUserId { get; set; }
    public User? Receiver { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}
