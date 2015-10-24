using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Conference.Api.Public.Models;
using Infrastructure.Messaging;
using Infrastructure.Tasks;
using Registration.Commands;
using Registration.ReadModel;

namespace Conference.Api.Public.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [OutputCache(Duration = 0, NoStore = true)]
    public class PricedOrderController : ControllerBase
    {
        private ConferenceAlias _conferenceAlias;
        private string _conferenceCode;
        public const string ThirdPartyProcessorPayment = "thirdParty";
        public const string InvoicePayment = "invoice";
        private static readonly TimeSpan DraftOrderWaitTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DraftOrderPollInterval = TimeSpan.FromMilliseconds(750);
        private static readonly TimeSpan PricedOrderWaitTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PricedOrderPollInterval = TimeSpan.FromMilliseconds(750);

        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IConferenceDao _conferenceDao;

        public PricedOrderController(ICommandBus commandBus, IOrderDao orderDao, IConferenceDao conferenceDao)
        {
            this._commandBus = commandBus;
            this._orderDao = orderDao;
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

        [System.Web.Http.HttpGet]
        public Task<RegistrationViewModel> Get([FromUri]string conferenceCode = "", [FromUri]Guid? orderId = null, [FromUri]int orderVersion = 0)
        {
            this.ConferenceCode = conferenceCode;

            return this.WaitUntilOrderIsPriced(orderId.Value, orderVersion)
                .ContinueWith<RegistrationViewModel>(t =>
                {
                    var pricedOrder = t.Result;
                    if (pricedOrder == null)
                    {
                        return new RegistrationViewModel { PricedOrderUnknown = true };
                    }

                    if (!pricedOrder.ReservationExpirationDate.HasValue)
                    {
                        //return View("ShowCompletedOrder");
                        return new RegistrationViewModel { OrderCompleted = true };
                    }

                    if (pricedOrder.ReservationExpirationDate < DateTime.UtcNow)
                    {
                        return new RegistrationViewModel { ConferenceCode = this.ConferenceAlias.Code, OrderId = orderId.Value };
                    }

                    return new RegistrationViewModel
                    {
                        RegistrantDetails = new AssignRegistrantDetails { OrderId = orderId.Value },
                        Order = pricedOrder
                    };
                });
        }

        private Task<Registration.ReadModel.PricedOrder> WaitUntilOrderIsPriced(Guid orderId, int lastOrderVersion)
        {
            return
                TimerTaskFactory.StartNew<Registration.ReadModel.PricedOrder>(
                    () => this._orderDao.FindPricedOrder(orderId),
                    order => order != null && order.OrderVersion > lastOrderVersion,
                    PricedOrderPollInterval,
                    PricedOrderWaitTimeout);
        }
    }
}