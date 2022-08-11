using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = new DateTime();
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime cretedDate)
        {
            Id = id;
            CreatedDate = cretedDate;
        }

        [JsonProperty]
        public Guid Id { get; private set; }
        [JsonProperty]
        public DateTime CreatedDate { get; private set; }

    }
}
