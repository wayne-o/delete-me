using System;
using System.Collections.Generic;

namespace Conference.Api.Public.Models
{
    public class StartRegistrationResult
    {
        public string ConferenceCode { get; set; }

        public Guid OrderId { get; set; }

        public int Version { get; set; }

        public bool Success { get; set; }

        public IList<string> Errors { get; set; }
    }
}