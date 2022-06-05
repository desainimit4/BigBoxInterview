using System;

namespace BigBoxInterview
{
    public class EventResponse
    {
        public string count{ get; set; }

        public EventResponse()
        {

        }

        public EventResponse(int count)
        {
            this.count = count.ToString();
        }
    }
}
