using System;

namespace AppA.AuditLog
{
    public class Audit
    {
        public Guid AuditID { get; set; }
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public string AreaAccessed { get; set; }
        public DateTime Timestamp { get; set; }

        //Default Constructor
        public Audit() { }
    }
}
