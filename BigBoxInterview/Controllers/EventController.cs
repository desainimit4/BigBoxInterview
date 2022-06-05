using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;

namespace BigBoxInterview.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;

        private IDistributedCache _memoryCache;

        public EventController(ILogger<EventController> logger, IDistributedCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("/events")]
        public IActionResult PostEvent(EventList events)
        {
            PostgresProvider postgresProvider = new PostgresProvider();

            foreach (var e in events.user_events)
            {
                int result = 0;
                byte[] bytes = _memoryCache.Get(e.action);
                result = (bytes != null) ? BitConverter.ToInt32(bytes) : 0;

                _memoryCache.Set(e.action, BitConverter.GetBytes(++result), new DistributedCacheEntryOptions());

                postgresProvider.InsertIntoTable(e);
            }

            return this.Ok(GetOutputString(new EventResponse(events.user_events.Count())));
        }

        [HttpGet]
        [Route("/events")]
        public ActionResult<string> GetActionCountFromCache([FromQuery(Name = "action")] string action)
        {           
            byte[] bytes = _memoryCache.Get(action);
            int result = (bytes != null) ? BitConverter.ToInt32(bytes) : 0;

            ActionResponse response = new ActionResponse()
            {
                action = action,
                count = result.ToString()
            };

            return this.Ok(GetOutputString(response));
        }

        [HttpGet]
        [Route("/eventsFromDB")]
        public ActionResult<string> GetActionCountFromDB([FromQuery(Name = "action")] string action)
        {
            PostgresProvider postgresProvider = new PostgresProvider();

            int result = postgresProvider.GetActionNameCount(action);

            //Rough way to handle cases where cache and DB are out of sync
            byte[] bytes = _memoryCache.Get(action);
            int cacheResult = (bytes != null) ? BitConverter.ToInt32(bytes) : 0;

            if (result != cacheResult)
            {
                _memoryCache.Set(action, BitConverter.GetBytes(result), new DistributedCacheEntryOptions());
            }

            ActionResponse response = new ActionResponse()
            {
                action = action,
                count = result.ToString()
            };

            return this.Ok(GetOutputString(response));
        }

        private string GetOutputString<T>(T response)
        {
            var acceptHeader = this.Request.Headers["Accept"];

            if (acceptHeader.Contains("xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, response);
                    return textWriter.ToString();
                }
            }

            //ToDo: add plain/text

            return JsonConvert.SerializeObject(response);
        }
    }
}
