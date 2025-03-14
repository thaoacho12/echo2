using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace PresentationLayer.Exceptions
{
    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureBuildInExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var contextRequest = context.Features.Get<IHttpRequestFeature>();

                    if (contextFeature != null)
                    {
                        var exception = contextFeature.Error;

                        int statusCode;
                        string message;

                        switch (exception)
                        {
                            case ExceptionNotFound: //  không tìm thấy bản ghi
                                statusCode = (int)HttpStatusCode.NotFound;
                                message = exception.Message; 
                                break;

                            case UnauthorizedAccessException:
                                statusCode = (int)HttpStatusCode.Unauthorized;
                                message = "Unauthorized access.";
                                break;

                            case ArgumentException://  tham số không hợp lệ
                            case ExceptionBusinessLogic:
                            case ExceptionForeignKeyViolation: //  vi phạm quy tắc dữ liệu
                                statusCode = (int)HttpStatusCode.BadRequest;
                                message = exception.Message; // Ví dụ: "Tên đã tồn tại" hoặc "Vi phạm ràng buộc dữ liệu"
                                break;

                            case InvalidOperationException:
                                statusCode = (int)HttpStatusCode.BadRequest;
                                message = exception.Message;
                                break;

                            case SqlException:
                                statusCode = (int)HttpStatusCode.InternalServerError;
                                message = "A database error occurred. Please try again later.";
                                break;

                            case TimeoutException:
                                statusCode = (int)HttpStatusCode.RequestTimeout;
                                message = "The request timed out. Please try again later.";
                                break;

                            case ValidationException:
                                statusCode = (int)HttpStatusCode.BadRequest;
                                message = "Validation failed for the provided data.";
                                break;

                            default:
                                statusCode = (int)HttpStatusCode.InternalServerError;
                                message = $"An unexpected error of type '{exception.GetType().Name}' occurred. Details: {exception.Message}";
                                break;
                        }


                        context.Response.StatusCode = statusCode;

                        // Tạo đối tượng lỗi trả về
                        var errorResponse = new ErrorVm
                        {
                            StatusCode = statusCode,
                            Message = message,
                            Path = contextRequest?.Path
                        };

                        await context.Response.WriteAsync(errorResponse.ToString());
                    }
                });
            });
        }
    }

}
