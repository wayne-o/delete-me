using System;
using Infrastructure.Messaging;

namespace Payments.Contracts.Commands
{
    public class NotifyUserOfFailedfullPayment : ICommand
    {
        public NotifyUserOfFailedfullPayment()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid PaymentId { get; set; }
        public object UserEmail { get; set; }
        public object Username { get; set; }
        public object UserId { get; set; }
    }
}