using Automatonymous;
using Inventory.Contracts;
using Trading.API.Activities;
using Trading.API.Contracts;

namespace Trading.API.StateMachines;

public class PurchaseStateMachine : MassTransitStateMachine<PurchaseState>
{
    public State? Accepted { get; }

    public State? ItemsGranted { get; }

    public State? Completed { get; }

    public State? Faulted { get; }

    public Event<PurchaseRequested>? PurchaseRequested { get; }

    public Event<GetPurchaseState>? GetPurchaseState { get; }

    public Event<InventoryItemsGranted>? InventoryItemsGranted { get; }

    public PurchaseStateMachine()
    {
        InstanceState(state => state.CurrentState);

        ConfigureEvents();

        ConfigureInitialState();

        ConfigureAny();

        ConfigureAccepted();
    }

    private void ConfigureEvents()
    {
        Event(() => PurchaseRequested);

        Event(() => GetPurchaseState);

        Event(() => InventoryItemsGranted);
    }

    private void ConfigureInitialState()
    {
        Initially(
            When(PurchaseRequested)
                .Then(context =>
                {
                    context.Instance.UserId = context.Data.UserId;
                    context.Instance.ItemId = context.Data.ItemId;
                    context.Instance.Quantity = context.Data.Quantity;
                    context.Instance.Received = DateTimeOffset.UtcNow;
                    context.Instance.LastUpdated = context.Instance.Received;
                })
                .Activity(x => x.OfType<CalculatePurchaseTotalActivity>())
                .Send(context => new GrantItems(
                        context.Instance.UserId,
                        context.Instance.ItemId,
                        context.Instance.Quantity,
                        context.Instance.CorrelationId))
                .TransitionTo(Accepted)
                .Catch<Exception>(ex => ex.
                        Then(context =>
                        {
                            context.Instance.ErrorMessage = context.Exception.Message;
                            context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                        })
                        .TransitionTo(Faulted))
        );
    }

    private void ConfigureAccepted()
    {
        During(Accepted,
            When(InventoryItemsGranted)
                .Then(context =>
                {
                    context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                })
                .TransitionTo(ItemsGranted));
    }

    private void ConfigureAny()
    {
        DuringAny(
            When(GetPurchaseState)
                .Respond(x => x.Instance)
        );
    }

}