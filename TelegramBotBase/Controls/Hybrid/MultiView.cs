using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Controls.Hybrid;

/// <summary>
///     This Control is for having a basic form content switching control.
/// </summary>
public abstract class MultiView : ControlBase
{
    private int _mISelectedViewIndex;

    /// <summary>
    ///     Hold if the View has been rendered already.
    /// </summary>
    private bool _rendered;


    public MultiView()
    {
        Messages = new List<int>();
    }

    /// <summary>
    ///     Index of the current View.
    /// </summary>
    public int SelectedViewIndex
    {
        get => _mISelectedViewIndex;
        set
        {
            _mISelectedViewIndex = value;

            //Already rendered? Re-Render
            if (_rendered)
            {
                ForceRender().Wait();
            }
        }
    }

    private List<int> Messages { get; }


    private Task Device_MessageSent(object sender, MessageSentEventArgs e)
    {
        if (e.Origin == null || !e.Origin.IsSubclassOf(typeof(MultiView)))
        {
            return Task.CompletedTask;
        }

        Messages.Add(e.MessageId);
        return Task.CompletedTask;
    }

    public override void Init()
    {
        Device.MessageSent += Device_MessageSent;
    }

    public override Task Load(MessageResult result)
    {
        _rendered = false;
        return Task.CompletedTask;
    }


    public override async Task Render(MessageResult result)
    {
        //When already rendered, skip rendering
        if (_rendered)
        {
            return;
        }

        await CleanUpView();

        await RenderView(new RenderViewEventArgs(SelectedViewIndex));

        _rendered = true;
    }


    /// <summary>
    ///     Will get invoked on rendering the current controls view.
    /// </summary>
    /// <param name="e"></param>
    public virtual Task RenderView(RenderViewEventArgs e)
    {
        return Task.CompletedTask;
    }

    private async Task CleanUpView()
    {
        var tasks = new List<Task>();

        foreach (var msg in Messages)
        {
            tasks.Add(Device.DeleteMessage(msg));
        }

        await Task.WhenAll(tasks);

        Messages.Clear();
    }

    /// <summary>
    ///     Forces render of control contents.
    /// </summary>
    public async Task ForceRender()
    {
        await CleanUpView();

        await RenderView(new RenderViewEventArgs(SelectedViewIndex));

        _rendered = true;
    }

    public override async Task Cleanup()
    {
        Device.MessageSent -= Device_MessageSent;

        await CleanUpView();
    }
}