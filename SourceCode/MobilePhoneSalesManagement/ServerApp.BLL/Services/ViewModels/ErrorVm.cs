using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SkipValidationAttribute : Attribute
{
}
public class ErrorVm
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Path { get; set; }
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

//  vi phạm quy tắc dữ liệu
public class ExceptionBusinessLogic : Exception
{
    public ExceptionBusinessLogic(string message) : base(message) { }
}

//  không tìm thấy bản ghi
public class ExceptionNotFound : Exception
{
    public ExceptionNotFound(string message) : base(message) { }
}


//  vi phạm quy tắc dữ liệu
public class ExceptionForeignKeyViolation : Exception
{
    public ExceptionForeignKeyViolation(string message) : base(message) { }
}
