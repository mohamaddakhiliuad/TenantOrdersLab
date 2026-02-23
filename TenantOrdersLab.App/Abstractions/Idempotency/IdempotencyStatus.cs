using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.App.Abstractions.Idempotency
{
    public enum IdempotencyStatus : byte
    {
        InProgress = 0,
        Completed = 1
    }
}
