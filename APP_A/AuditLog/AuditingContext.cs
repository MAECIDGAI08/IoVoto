using Microsoft.EntityFrameworkCore;

namespace AppA.AuditLog
{
    public class AuditingContext : DbContext
    {
        public DbSet<Audit> AuditRecords { get; set; }
    }
}
