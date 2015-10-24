using System;

namespace Conference.Api.Public.Models
{
    public class PaymentViewModel
    {
        public string ConferenceCode { get; set; }
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public int OrderVersion { get; set; }
        public bool ReturnToStartRegistration { get; set; }
        public bool ShowCompletedOrder { get; set; }
        public bool ShowExpiredOrder { get; set; }
        public bool ReservationUnknown { get; set; }
    }
}