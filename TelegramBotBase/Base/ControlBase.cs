using System.Threading.Tasks;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base;

/// <summary>
///     Base class for controls
/// </summary>
public class ControlBase
{
    public DeviceSession Device { get; set; }

    public int Id { get; set; }

    public string ControlId => "#c" + Id;

    /// <summary>
    ///     Defines if the control should be rendered and invoked with actions
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Get invoked when control will be added to a form and invoked.
    /// </summary>
    /// <returns></returns>
    public virtual void Init()
    {
    }

    public virtual Task Load(MessageResult result)
    {
        return Task.CompletedTask;
    }

    public virtual Task Action(MessageResult result, string value = null)
    {
        return Task.CompletedTask;
    }


    public virtual Task Render(MessageResult result)
    {
        return Task.CompletedTask;
    }

    public virtual Task Hidden(bool formClose)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Will be called on a cleanup.
    /// </summary>
    /// <returns></returns>
    public virtual Task Cleanup()
    {
        return Task.CompletedTask;
    }
}