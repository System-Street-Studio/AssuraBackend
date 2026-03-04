using Assura.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Assura.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IEnumerable<IValidator<TestRequest>>> _validatorsMock;
    private readonly ValidationBehavior<TestRequest, TestResponse> _behavior;

    public ValidationBehaviorTests()
    {
        _validatorsMock = new Mock<IEnumerable<IValidator<TestRequest>>>();
        _behavior = new ValidationBehavior<TestRequest, TestResponse>(_validatorsMock.Object.AsEnumerable());
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var request = new TestRequest();
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        _validatorsMock.Setup(x => x.GetEnumerator()).Returns(new List<IValidator<TestRequest>>().GetEnumerator());

        // Act
        await _behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidationFailures_ShouldThrowValidationException()
    {
        // Arrange
        var request = new TestRequest();
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        
        var failure = new FluentValidation.Results.ValidationFailure("Prop", "Error");
        var result = new FluentValidation.Results.ValidationResult(new[] { failure });
        
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var validators = new List<IValidator<TestRequest>> { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(request, nextMock.Object, CancellationToken.None));
    }

    public class TestRequest : IRequest<TestResponse> { }
    public class TestResponse { }
}
