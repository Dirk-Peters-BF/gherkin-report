using Reqnroll;

namespace ReqnRollSample.Steps;

[Binding]
public class StackSteps
{
    private readonly StackContext context;

    public StackSteps(StackContext context) => this.context = context;

    [Given("an empty stack")]
    public void GivenAnEmptyStack() => context.CreateNewStack();

    [Then("the current stack size is {int}")]
    public void ThenTheCurrentStackSizeIs(int expectedSize) =>
        context.CheckCurrentStack(stack => Assert.That(stack.Size, Is.EqualTo(expectedSize)));

    [When("{int} is pushed")]
    public void WhenIsPushed(int value) => context.WithCurrentStack(s => s.Push(value));

    [When("one item is popped")]
    public void WhenOneItemIsPopped() =>
        context.WithCurrentStack(s => s.Popped());
}