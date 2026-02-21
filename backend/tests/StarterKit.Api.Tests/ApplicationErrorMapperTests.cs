using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Api.Errors;
using StarterKit.Application.Results;

namespace StarterKit.Api.Tests;

public sealed class ApplicationErrorMapperTests
{
    [Theory]
    [InlineData(ErrorCode.ValidationFailed, typeof(BadRequestObjectResult), "validation")]
    [InlineData(ErrorCode.EmailAlreadyRegistered, typeof(ConflictObjectResult), "duplicate")]
    [InlineData(ErrorCode.InvalidCredentials, typeof(UnauthorizedObjectResult), "invalid creds")]
    public void Map_ErrorWithMessage_ReturnsExpectedObjectResult(
        ErrorCode code,
        Type expectedResultType,
        string expectedMessage)
    {
        var controller = CreateController();

        var result = ApplicationErrorMapper.Map(controller, new AppError(code, expectedMessage));

        Assert.IsType(expectedResultType, result);
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(expectedMessage, objectResult.Value);
    }

    [Theory]
    [InlineData(ErrorCode.UserNotFound)]
    public void Map_ErrorWithoutPayload_ReturnsUnauthorized(ErrorCode code)
    {
        var controller = CreateController();

        var result = ApplicationErrorMapper.Map(controller, new AppError(code, "missing"));

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Map_NullError_ReturnsProblemObjectResult()
    {
        var controller = CreateController();

        var result = ApplicationErrorMapper.Map(controller, null);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    private static ControllerBase CreateController()
    {
        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private sealed class TestController : ControllerBase
    {
    }
}
