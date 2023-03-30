using CommonLibrary.Interfaces;
using Identity.Contracts;
using MassTransit;
using Trading.API.Entities;

namespace Trading.API.Consumers;

public class UserUpdatedConsumer : IConsumer<UserUpdated>
{
    private readonly IRepository<ApplicationUser> _repository;

    public UserUpdatedConsumer(IRepository<ApplicationUser> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        var message = context.Message;

        var user = await _repository.GetAsync(message.UserId);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = message.UserId,
                Gil = message.NewTotalGil
            };

            await _repository.CreateAsync(user);
        }
        else
        {
            user.Gil = message.NewTotalGil;

            await _repository.UpdateAsync(user);
        }
    }
}