namespace Application.Actions;

public class CustomAction
{
    public CustomAction(Action action)
    {
        this.Action = action ?? throw new ArgumentNullException(nameof(action));
    }
    
    public Action Action { get; set; }
}