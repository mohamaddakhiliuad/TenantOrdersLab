using Microsoft.AspNetCore.Mvc;
using TenantOrdersLab.App.Abstractions.Common;

namespace TenantOrdersLab.Api.Common;

public static class ResultHttpMapper
{
    public static IResult ToHttpResult(this Result result, HttpContext http)
    {
        if (result.IsSuccess) return Results.NoContent();
        return ApiProblemFactory.ToProblemResult(result.Error, http);
    }

    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext http, Func<T, IResult>? onSuccess = null)
    
    {
        if (result.IsSuccess)
        {
            if (result.Value is null) return Results.Ok();
            return onSuccess is null ? Results.Ok(result.Value) : onSuccess(result.Value);
        }

        return ApiProblemFactory.ToProblemResult(result.Error, http);
    }
}