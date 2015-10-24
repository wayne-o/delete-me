// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Infrastructure.Messaging;
using SendGrid;
using Stripe;

namespace Payments.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Infrastructure.Database;
    using Infrastructure.Messaging.Handling;
    using Payments.Contracts.Commands;
    using System.Net;
    using System.Net.Mail;

    public class ThirdPartyProcessorPaymentCommandHandler :
        ICommandHandler<InitiateThirdPartyProcessorPayment>,
        ICommandHandler<CompleteThirdPartyProcessorPayment>,
        ICommandHandler<CancelThirdPartyProcessorPayment>,
        ICommandHandler<NotifyUserOfSuccesfullPayment>,
        ICommandHandler<NotifyUserOfFailedfullPayment>
    {
        private readonly Func<IDataContext<ThirdPartyProcessorPayment>> _contextFactory;
        private const string StripeApiKey = "pk_test_oLaix1ER1XTQFXyyFTO7f4Jk";
        private readonly ICommandBus _commandBus;

        public ThirdPartyProcessorPaymentCommandHandler(Func<IDataContext<ThirdPartyProcessorPayment>> contextFactory, ICommandBus commandBus)
        {
            this._contextFactory = contextFactory;
            _commandBus = commandBus;
        }

        public void Handle(InitiateThirdPartyProcessorPayment command)
        {
            var repository = this._contextFactory();

            using (repository as IDisposable)
            {
                var items = command.Items.Select(t => new ThidPartyProcessorPaymentItem(t.Description, t.Amount)).ToList();
                var payment = new ThirdPartyProcessorPayment(command.PaymentId, command.PaymentSourceId, command.Description, command.TotalAmount, items);

                repository.Save(payment);
            }
        }

        private static StripeCharge ChargeCustomer(string tokenId, int amount)
        {
            var myCharge = new StripeChargeCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Description = "Sonatribe Tickets",
                Source = new StripeSourceOptions
                {
                    TokenId = tokenId
                }
            };

            var chargeService = new StripeChargeService();
            var stripeCharge = chargeService.Create(myCharge);

            return stripeCharge;
        }

        public void Handle(CompleteThirdPartyProcessorPayment command)
        {
            var repository = this._contextFactory();
            Console.WriteLine("Whoo Hoo...  We made our first sale!");
            using (repository as IDisposable)
            {
                var payment = repository.Find(command.PaymentId);

                if (payment != null)
                {
                    StripeCharge charge = null;

                    try
                    {
                        charge = ChargeCustomer(command.Token, (int)payment.TotalAmount);
                    }
                    catch (StripeException se)
                    {
                        Console.WriteLine("Payment failed. :(");
                        _commandBus.Send(new CancelThirdPartyProcessorPayment { PaymentId = command.PaymentId, UserEmail = command.UserEmail, Username = command.UserEmail, UserId = command.UserId });
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Payment failed. :(");
                        _commandBus.Send(new CancelThirdPartyProcessorPayment { PaymentId = command.PaymentId, UserEmail = command.UserEmail, Username = command.UserEmail, UserId = command.UserId });
                        throw;
                    }

                    if (charge != null && charge.Paid && charge.Status == "succeeded")
                    {
                        payment.Items.Add(new ThidPartyProcessorPaymentItem("payment", charge.Amount));
                        payment.Complete(charge);
                        repository.Save(payment);

                        _commandBus.Send(new NotifyUserOfSuccesfullPayment { PaymentId = command.PaymentId, UserEmail = command.UserEmail, Username = command.UserEmail, UserId = command.UserId });
                    }
                    else
                    {
                        Console.WriteLine("Payment failed. :(");
                        _commandBus.Send(new CancelThirdPartyProcessorPayment { PaymentId = command.PaymentId, UserEmail = command.UserEmail, Username = command.UserEmail, UserId = command.UserId });
                    }
                }
                else
                {
                    Trace.TraceError("Failed to locate the payment entity with id {0} for the completed third party payment.", command.PaymentId);
                }
            }
        }

        public void Handle(CancelThirdPartyProcessorPayment command)
        {
            var repository = this._contextFactory();

            using (repository as IDisposable)
            {
                var payment = repository.Find(command.PaymentId);

                if (payment != null)
                {
                    payment.Cancel();
                    repository.Save(payment);
                    _commandBus.Send(new NotifyUserOfFailedfullPayment { UserEmail = command.UserEmail, Username = command.UserEmail, UserId = command.UserId, PaymentId = command.PaymentId });
                }
                else
                {
                    Trace.TraceError("Failed to locate the payment entity with id {0} for the cancelled third party payment.", command.PaymentId);
                }
            }
        }

        public void Handle(NotifyUserOfSuccesfullPayment command)
        {
            var repository = this._contextFactory();

            using (repository as IDisposable)
            {
                var payment = repository.Find(command.PaymentId);

                if (payment != null)
                {
                    var message = new SendGridMessage { From = new MailAddress("tickets@sonatribe.com") };

                    var recipients = new List<String>
                    {
                        string.Format(@"{0} <{1}>", command.Username, command.UserEmail)
                    };

                    message.AddTo(recipients);

                    message.Subject = "Sonatribe tickets";

                    message.Html = "<p>Payment succesful!</p>";
                    message.Text = "Payment succesful!";


                    var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridKey"]);

                    Task.Run(() => transportWeb.DeliverAsync(message));
                }
                else
                {
                    Trace.TraceError("Failed to locate the payment entity with id {0} for the cancelled third party payment.", command.PaymentId);
                }
            }
        }

        public void Handle(NotifyUserOfFailedfullPayment command)
        {
            var repository = this._contextFactory();

            using (repository as IDisposable)
            {
                var payment = repository.Find(command.PaymentId);

                if (payment != null)
                {
                    var message = new SendGridMessage { From = new MailAddress("tickets@sonatribe.com") };

                    var recipients = new List<String>
                    {
                        string.Format(@"{0} <{1}>", command.Username, command.UserEmail)
                    };

                    message.AddTo(recipients);

                    message.Subject = "Sonatribe tickets";

                    message.Html = "<p>Payment wasn't succesful!</p> <p> OrPaymentder Id: " + command.PaymentId + "</p>";
                    message.Text = "Payment wasn't succesful!";


                    var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridKey"]);

                    Task.Run(() => transportWeb.DeliverAsync(message));
                }
                else
                {
                    Trace.TraceError("Failed to locate the payment entity with id {0} for the cancelled third party payment.", command.PaymentId);
                }
            }
        }
    }
}
