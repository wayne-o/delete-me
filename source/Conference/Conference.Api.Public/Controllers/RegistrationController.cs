using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Conference.Api.Public.Models;
using Infrastructure.Messaging;
using Infrastructure.Utils;
using Registration.Commands;
using Registration.ReadModel;

namespace Conference.Api.Public.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [OutputCache(Duration = 0, NoStore = true)]
    public class RegistrationController : ControllerBase
    {
        private ConferenceAlias _conferenceAlias;
        private string _conferenceCode;
        private readonly IConferenceDao _conferenceDao;
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;

        public RegistrationController(IConferenceDao conferenceDao, IOrderDao orderDao, ICommandBus commandBus)
        {
            this._conferenceDao = conferenceDao;
            _orderDao = orderDao;
            _commandBus = commandBus;
        }

        [System.Web.Http.HttpGet]
        public Task<OrderViewModel> Get([FromUri]string conferenceCode, [FromUri]Guid? orderId = null)
        {
            this.ConferenceCode = conferenceCode;

            var viewModelTask = Task.Factory.StartNew(() => this.CreateViewModel());

            if (!orderId.HasValue)
            {
                return viewModelTask.ContinueWith<OrderViewModel>(t =>
                {
                    var viewModel = t.Result;
                    viewModel.OrderId = GuidUtil.NewSequentialId();
                    return viewModel;
                });
            }

            //TODO: other iffs

            return null;
        }

        [System.Web.Http.HttpPost]
        public StartRegistrationResult Post(RegisterToConference command)
        {
            this.ConferenceCode = command.ConferenceCode;
            var existingOrder = command.OrderVersion != 0 ? this._orderDao.FindDraftOrder(command.OrderId) : null;
            var viewModel = this.CreateViewModel();

            if (existingOrder != null)
            {
                UpdateViewModel(viewModel, existingOrder);
            }

            viewModel.OrderId = command.OrderId;

            bool needsExtraValidation = false;
            foreach (var seat in command.Seats)
            {
                var modelItem = viewModel.Items.FirstOrDefault(x => x.SeatType.Id == seat.SeatType);
                if (modelItem != null)
                {
                    if (seat.Quantity > modelItem.MaxSelectionQuantity)
                    {
                        modelItem.PartiallyFulfilled = needsExtraValidation = true;
                        modelItem.OrderItem.ReservedSeats = modelItem.MaxSelectionQuantity;
                    }
                }
                else
                {
                    // seat type no longer exists for conference.
                    needsExtraValidation = true;
                }
            }

            //TODO: doesn't this need some work? 
            if (needsExtraValidation)
            {
                return new StartRegistrationResult
                {
                    Success = false
                };
            }

            command.ConferenceId = this.ConferenceAlias.Id;
            this._commandBus.Send(command);

            return new StartRegistrationResult
            {
                Success = true,
                ConferenceCode = this.ConferenceCode,
                OrderId = command.OrderId,
                Version = command.OrderVersion
            };
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

        private OrderViewModel CreateViewModel()
        {
            var seatTypes = this._conferenceDao.GetPublishedSeatTypes(this.ConferenceAlias.Id);
            var viewModel =
                new OrderViewModel
                {
                    ConferenceId = this.ConferenceAlias.Id,
                    ConferenceCode = this.ConferenceAlias.Code,
                    ConferenceName = this.ConferenceAlias.Name,
                    Items =
                        seatTypes.Select(
                            s =>
                                new OrderItemViewModel
                                {
                                    SeatType = s,
                                    OrderItem = new DraftOrderItem(s.Id, 0),
                                    AvailableQuantityForOrder = Math.Max(s.AvailableQuantity, 0),
                                    MaxSelectionQuantity = Math.Max(Math.Min(s.AvailableQuantity, 20), 0)
                                }).ToList(),
                };

            return viewModel;
        }

        private static void UpdateViewModel(OrderViewModel viewModel, DraftOrder order)
        {
            viewModel.OrderId = order.OrderId;
            viewModel.OrderVersion = order.OrderVersion;
            viewModel.ReservationExpirationDate = order.ReservationExpirationDate.ToEpochMilliseconds();

            // TODO check DTO matches view model

            foreach (var line in order.Lines)
            {
                var seat = viewModel.Items.First(s => s.SeatType.Id == line.SeatType);
                seat.OrderItem = line;
                seat.AvailableQuantityForOrder = seat.AvailableQuantityForOrder + line.ReservedSeats;
                seat.MaxSelectionQuantity = Math.Min(seat.AvailableQuantityForOrder, 20);
                seat.PartiallyFulfilled = line.RequestedSeats > line.ReservedSeats;
            }
        }
    }
}