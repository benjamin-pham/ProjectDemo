using ProjectTemplate.Application.Abstractions.Authentication;
using ProjectTemplate.Application.Features.Auth.Register;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.UnitTests.Auth;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ReturnsUsernameAlreadyTaken()
    {
        var existingUser = User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

        _userRepository.GetByUsernameAsync("nguyenvana", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var command = new RegisterUserCommand(
            "Nguyen",
            "Van B",
            "nguyenvana",
            "Secret123",
            "vana@example.com",
            "0123456789",
            new DateOnly(1998, 1, 20));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UsernameAlreadyTaken);

        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
        _userRepository.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WithAvailableUsername_CreatesUserAndReturnsResponse()
    {
        User? addedUser = null;
        var birthday = new DateOnly(1998, 1, 20);

        _userRepository.GetByUsernameAsync("nguyenvana", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.Hash("Secret123").Returns("hashed-password");
        _userRepository.When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => addedUser = callInfo.Arg<User>());

        var command = new RegisterUserCommand(
            "Nguyen",
            "Van A",
            "nguyenvana",
            "Secret123",
            "vana@example.com",
            "0123456789",
            birthday);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("nguyenvana");
        result.Value.FirstName.Should().Be("Nguyen");
        result.Value.LastName.Should().Be("Van A");

        _passwordHasher.Received(1).Hash("Secret123");
        _userRepository.Received(1).Add(Arg.Any<User>());

        addedUser.Should().NotBeNull();
        addedUser!.Username.Should().Be("nguyenvana");
        addedUser.FirstName.Should().Be("Nguyen");
        addedUser.LastName.Should().Be("Van A");
        addedUser.PasswordHash.Should().Be("hashed-password");
        addedUser.Email.Should().Be("vana@example.com");
        addedUser.Phone.Should().Be("0123456789");
        addedUser.Birthday.Should().Be(birthday);

        result.Value.UserId.Should().Be(addedUser.Id);
    }
}
