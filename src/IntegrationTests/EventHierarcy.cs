// Event hierarchy logic
// AggregateRoot
//  Entity


namespace IntegrationTests
{
    public interface IProblem_1_BaseEventAllInheritsFrom // Problem: Will become single point. Topic will receive all events and discard them.
    {
        string Version { get; }
        string AggregateRootId { get; }
    }

    public abstract class BaseEvent : IProblem_1_BaseEventAllInheritsFrom
    {
        public string Version => "1";

        public string AggregateRootId => "A";
    }
}

namespace IntegrationTests.Problem
{
    public interface IAggregateRootLevel : IProblem_1_BaseEventAllInheritsFrom {  } // Probably unnecessary level (similar to problem 1, but on AR level)
    public interface IEntityLevel : IAggregateRootLevel { string EntityId { get; } } // Used to dispatch all entity level events to entity (similar to problem 1, but on Entity level)
    public interface IProblem_2_Part1 : IEntityLevel { string P1 { get; } }
    public interface IProblem_2_Part2 : IEntityLevel { string P2 { get; } }

    public class TheProblemEvent : BaseEvent, IEntityLevel, IProblem_2_Part1, IProblem_2_Part2
    {
        public string P1 => "A";
        public string P2 => "B";

        public string EntityId => "E";
    }
}

// In 9, Out 1 (deduplication)
namespace IntegrationTests.Cleaner
{
    public interface IEntityLevel : IProblem_1_BaseEventAllInheritsFrom { string EntityId { get; } } // Reduce depth as AR level is removed
    public interface IPart1  { string P1 { get; } }
    public interface IPart2  { string P2 { get; } }

    public class TheCleanerEvent : BaseEvent, IEntityLevel, IPart1, IPart2
    {
        public string P1 => "A";
        public string P2 => "B";

        public string EntityId => "E";
    }
}
