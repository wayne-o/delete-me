using System;
using Infrastructure.Messaging;

namespace Payments.Contracts.Commands
{
    public class NotifyUserOfSuccesfullPayment : ICommand
    {
        public NotifyUserOfSuccesfullPayment()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid PaymentId { get; set; }
        public string UserEmail { get; set; }
        public string Username { get; set; }
        public int UserId { get; set; }
    }
}