using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.App.Abstractions.Idempotency
{
    public sealed record IdempotencyDecision(
    bool IsDuplicate,
    bool IsInProgress,
    bool HasConflict,
    int? ExistingOrderId);
}
