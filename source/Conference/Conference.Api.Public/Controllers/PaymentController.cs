using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Conference.Api.Public.Models;
using Infrastructure.Messaging;
using Infrastructure.Tasks;
using Infrastructure.Utils;
using Payments.Contracts.Commands;
using Registration.Commands;
using Registration.ReadModel;

namespace Conference.Api.Public.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [OutputCache(Duration = 0, NoStore = true)]
    public class PaymentController : ControllerBase
    {
        private readonly ICommandBus _commandBus;
        public const string ThirdPartyProcessorPayment = "thirdParty";
        public const string InvoicePayment = "invoice";
        private static readonly TimeSpan DraftOrderWaitTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DraftOrderPollInterval = TimeSpan.FromMilliseconds(750);
        private static readonly TimeSpan PricedOrderWaitTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PricedOrderPollInterval = TimeSpan.FromMilliseconds(750);
        private ConferenceAlias _conferenceAlias;
        private string _conferenceCode;
        private readonly IConferenceDao _conferenceDao;

        private readonly IOrderDao _orderDao;

        public PaymentController(ICommandBus commandBus, IOrderDao orderDao, IConferenceDao conferenceDao)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _conferenceDao = conferenceDao;
        }

        public string ConferenceCode
        {
            get
            {
                return this._conferenceCode ??
                       (this._conferenceCode = (string)ControllerContext.RouteData.Values["conferenceCode"]);
            }
            internal set { this._conferenceCode = value; }
        }

        public ConferenceAlias ConferenceAlias
        {
            get
            {
                return this._conferenceAlias ??
                       (this._conferenceAlias = this._conferenceDao.GetConferenceAlias(this.ConferenceCode));
            }
            internal set { this._conferenceAlias = value; }
        }


        public void Get([FromUri] Guid paymentId, [FromUri]string token)
        {
            this._commandBus.Send(new CompleteThirdPartyProcessorPayment
            {
                PaymentId = paymentId,
                Token = token,
                UserEmail = _user.Email,
                Username = _user.Username,
                UserId = _user.Id
            });
        }

        public Task<PaymentViewModel> Post([FromUri]string conferenceCode, Guid orderId, string paymentType, int orderVersion)
        {
            this.ConferenceCode = conferenceCode;

            return this.WaitUntilSeatsAreConfirmed(orderId, orderVersion)
                .ContinueWith(t =>
                {
                    var order = t.Result;
                    if (order == null)
                    {
                        return new PaymentViewModel { ReservationUnknown = true };
                    }

                    if (order.State == DraftOrder.States.PartiallyReserved)
                    {
                        //TODO: have a clear message in the UI saying there was a problem and he actually didn't get all the seats.
                        // This happened as a result the seats availability being eventually but not fully consistent when
                        // starting the reservation. It is very uncommon to reach this step, but could happen under heavy
                        // load, and when competing for the last remaining seats of the conference.
                        return new PaymentViewModel { ConferenceCode = this.ConferenceCode, OrderId = orderId, OrderVersion = order.OrderVersion, ReturnToStartRegistration = true };
                    }

                    if (order.State == DraftOrder.States.Confirmed)
                    {
                        return new PaymentViewModel { ShowCompletedOrder = true };
                    }

                    if (order.ReservationExpirationDate.HasValue && order.ReservationExpirationDate < DateTime.UtcNow)
                    {
                        return new PaymentViewModel { ShowExpiredOrder = true, ConferenceCode = this.ConferenceAlias.Code, OrderId = orderId };
                    }

                    var pricedOrder = this._orderDao.FindPricedOrder(orderId);
                    if (pricedOrder.IsFreeOfCharge)
                    {
                        return CompleteRegistrationWithoutPayment(orderId);
                    }

                    switch (paymentType)
                    {
                        case ThirdPartyProcessorPayment:

                            return CompleteRegistrationWithThirdPartyProcessorPayment(pricedOrder, orderVersion);

                        case InvoicePayment:
                            break;

                        default:
                            break;
                    }

                    throw new InvalidOperationException();
                });

        }

        private PaymentViewModel CompleteRegistrationWithoutPayment(Guid orderId)
        {
            var confirmationCommand = new ConfirmOrder { OrderId = orderId };

            this._commandBus.Send(confirmationCommand);

            return new PaymentViewModel { ConferenceCode = this.ConferenceAlias.Code, OrderId = orderId };
        }

        private PaymentViewModel CompleteRegistrationWithThirdPartyProcessorPayment(PricedOrder order, int orderVersion)
        {
            var paymentCommand = CreatePaymentCommand(order);

            this._commandBus.Send(paymentCommand);

            return new PaymentViewModel
            {
                ConferenceCode = this.ConferenceAlias.Code,
                PaymentId = paymentCommand.PaymentId
            };
        }

        private InitiateThirdPartyProcessorPayment CreatePaymentCommand(PricedOrder order)
        {
            // TODO: should add the line items?

            var description = "Registration for " + this.ConferenceAlias.Name;
            var totalAmount = order.Total;

            var paymentCommand =
                new InitiateThirdPartyProcessorPayment
                {
                    PaymentId = GuidUtil.NewSequentialId(),
                    ConferenceId = this.ConferenceAlias.Id,
                    PaymentSourceId = order.OrderId,
                    Description = description,
                    TotalAmount = totalAmount
                };

            return paymentCommand;
        }

        private Task<DraftOrder> WaitUntilSeatsAreConfirmed(Guid orderId, int lastOrderVersion)
        {
            return
                TimerTaskFactory.StartNew<DraftOrder>(
                    () => this._orderDao.FindDraftOrder(orderId),
                    order => order != null && order.State != DraftOrder.States.PendingReservation && order.OrderVersion > lastOrderVersion,
                    DraftOrderPollInterval,
                    DraftOrderWaitTimeout);
        }

    }
}