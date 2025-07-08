using JetBrains.Annotations;

namespace ReqnRollSample.Steps;

[UsedImplicitly]
public class StackContext
{
    private IStack? currentStack;

    public void CreateNewStack() => currentStack = IStack.Create();

    public void WithCurrentStack(Func<IStack, IStack> func)
    {
        if (currentStack == null)
        {
            Assert.Fail("stack is not initialized");
        }
        else
        {
            currentStack = func(currentStack);
        }
    }

    public void CheckCurrentStack(Action<IStack> action)
    {
        if (currentStack == null)
        {
            Assert.Fail("stack is not initialized");
        }
        else
        {
            action(currentStack);
        }
    }
}

public interface IStack
{
    int Size { get; }
    public IStack Push(int entry) => new Head(entry, this);
    public IStack Popped();
    public static IStack Create() => new EmptyStack();

    private record EmptyStack : IStack
    {
        public int Size => 0;
        public IStack Popped() => throw new NotImplementedException();
    }

    private record Head(int Value, IStack Tail) : IStack
    {
        public int Size => Tail.Size + 1;
        public IStack Popped() => Tail;
    }
}