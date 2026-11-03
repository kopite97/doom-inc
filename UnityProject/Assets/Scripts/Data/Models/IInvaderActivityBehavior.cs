namespace MobileGame.Foundation
{
    public readonly struct InvaderActivityContext
    {
        public readonly CreatureData Model;
        public readonly InvasionSettings Settings;
        public readonly long Round;
        public bool IsMajor => Settings.IsMajor(Round);

        public InvaderActivityContext(CreatureData model, InvasionSettings settings, long round)
        { Model = model; Settings = settings; Round = round; }
    }

    public readonly struct InvaderActivityOutcome
    {
        public readonly int Score;
        public readonly string ResourceId;
        public readonly int Amount;
        public InvaderActivityOutcome(int score, string resourceId = null, int amount = 0)
        { Score = score; ResourceId = resourceId; Amount = amount; }
    }

    public interface IInvaderActivityBehavior
    {
        InvaderActivityOutcome ResolveActivity(InvaderActivityContext context);
    }

    internal sealed class StandardInvaderBehavior : IInvaderActivityBehavior
    {
        public InvaderActivityOutcome ResolveActivity(InvaderActivityContext context) =>
            new InvaderActivityOutcome(context.IsMajor ? context.Settings.MajorScore : context.Settings.SmallScore);
    }

    internal sealed class RaiderInvaderBehavior : IInvaderActivityBehavior
    {
        public InvaderActivityOutcome ResolveActivity(InvaderActivityContext context)
        {
            var model = context.Model;
            var score = context.IsMajor ? model.MajorScore : model.SmallScore;
            return context.Round % model.SupplyEvery == 0
                ? new InvaderActivityOutcome(score, model.SupplyResource, model.SupplyAmount)
                : new InvaderActivityOutcome(score);
        }
    }
}
