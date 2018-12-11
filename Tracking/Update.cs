using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public class Update
    {
        public int TenantId { get; set; }
        public ApiType Api { get; set; }
        public string MatterNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public List<PropertyChange> PropertyChanges { get; set; }

        private Update(int tenantId, ApiType api, string matterNumber)
        {
            TenantId = tenantId;
            Api = api;
            MatterNumber = matterNumber;
            Timestamp = DateTime.UtcNow;
        }

        public Update(int tenantId, ApiType api, string matterNumber, ITrackable item)
            : this(tenantId, api, matterNumber)
        {
            PropertyChanges = item
                .TrackedProperties()
                .Select(x => new PropertyChange(x))
                .ToList();
        }

        public Update(
            int tenantId,
            ApiType api,
            string matterNumber,
            ITrackable originalItem,
            ITrackable modifiedItem)
            : this(tenantId, api, matterNumber)
        {
            PropertyChanges = originalItem
                .TrackedProperties()
                .Zip(modifiedItem.TrackedProperties(), (o, m) => new { o, m })
                .Where(x => x.o.HasChanged(x.m))
                .Select(x => new PropertyChange(x.o, x.m))
                .ToList();
        }
    }

    public enum ApiType
    {
        Employee,
        Project,
    }
}
