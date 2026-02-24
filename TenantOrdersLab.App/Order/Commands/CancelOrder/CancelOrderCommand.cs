using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.App.Order.Commands.CancelOrder;

/// <summary>
/// Input boundary for canceling an order (write side).
/// </summary>
public sealed record CancelOrderCommand(
    int OrderId,
    string Reason,
    string ExpectedRowVersion
);
