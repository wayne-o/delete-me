using Registration.ReadModel;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Tracing;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;
using Registration.Handlers;
using Infrastructure.Utils;

namespace Conference.Api.Public.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [OutputCache(Duration = 0, NoStore = true)]
    public class SeatController : ControllerBase
    {
        private readonly IConferenceDao _dao;
        private readonly System.Web.Http.Tracing.ITraceWriter _tracer;
        private ConferenceService service;

        public SeatController(IConferenceDao dao, ConferenceService conferenceService)
        {
            this._dao = dao;
            _tracer = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            service = conferenceService;
        }

        public IEnumerable<SeatType> Get(string slug)
        {
            var conference = this.service.FindConference(slug);
            return this.service.FindSeatTypes(conference.Id);
        }

        [System.Web.Http.HttpPost]
        public void Post([FromUri]string slug, [FromBody] SeatType seat)
        {
            _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "SeatController: POST");

            if (_user == null)
            {
                _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "User not found returning 404");
                this.NotFound();
            }

            var conference = this.service.FindConference(slug);

            try
            {
                seat.Id = GuidUtil.NewSequentialId();
                this.service.CreateSeat(conference.Id, seat);
            }
            catch (System.Exception exc)
            {
                _tracer.Error(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, exc);
                throw;
            }
        }

    }


    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [OutputCache(Duration = 0, NoStore = true)]
    public class ConferenceController : ControllerBase
    {
        private readonly IConferenceDao _dao;
        private readonly System.Web.Http.Tracing.ITraceWriter _tracer;
        private ConferenceService service;

        public ConferenceController(IConferenceDao dao, ConferenceService conferenceService)
        {
            this._dao = dao;
            _tracer = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            service = conferenceService;
        }

        // GET api/values
        public IList<ConferenceAlias> Get()
        {
            _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "ConferenceController: GET");
            if (_user == null)
            {
                _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "User not found returning 404");
                this.NotFound();
            }

            return this._dao.GetPublishedConferences();
        }

        [System.Web.Http.HttpPut]
        public ConferenceInfo Put(ConferenceInfo conference)
        {
            _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "ConferenceController: PUT");

            if (_user == null)
            {
                _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "User not found returning 404");
                this.NotFound();
            }

            if (conference.PublishNow)
            {
                try
                {
                    this.service.Publish(conference.Id);
                }
                catch (System.Exception exc)
                {
                    _tracer.Error(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, exc);
                    throw;
                }
            }

            return conference;
        }

        [System.Web.Http.HttpPost]
        public ConferenceInfo Post(ConferenceInfo conference)
        {
            _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "ConferenceController: POST");

            if (_user == null)
            {
                _tracer.Info(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, "User not found returning 404");
                this.NotFound();
            }

            try
            {
                conference.Id = GuidUtil.NewSequentialId();
                this.service.CreateConference(conference);
            }
            catch (System.Exception exc)
            {
                _tracer.Error(Request, this.ControllerContext.ControllerDescriptor.ControllerType.FullName, exc);
                throw;
            }

            return conference;
        }
    }
}
