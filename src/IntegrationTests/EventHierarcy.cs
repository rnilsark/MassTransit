namespace IntegrationTests
{
    public interface IBasiestEventInterface { string P1 { get; } }
    public interface IBaserEventInterface : IBasiestEventInterface { string P2 { get; } }
    public interface IBaseEventInterface : IBaserEventInterface { string P3 { get; } }
}
