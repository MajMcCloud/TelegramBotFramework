using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Controls.Hybrid
{

    /// <summary>
    /// This Control is for having a basic form content switching control.
    /// </summary>
    public abstract class MultiView : Base.ControlBase
    {
        /// <summary>
        /// Index of the current View.
        /// </summary>
        public int SelectedViewIndex
        {
            get
            {
                return m_iSelectedViewIndex;
            }
            set
            {
                m_iSelectedViewIndex = value;

                //Already rendered? Re-Render
                if (_Rendered)
                    ForceRender().Wait();
            }
        }

        private int m_iSelectedViewIndex = 0;

        /// <summary>
        /// Hold if the View has been rendered already.
        /// </summary>
        private bool _Rendered = false;

        private List<int> Messages { get; set; }


        public MultiView()
        {
            Messages = new List<int>();
        }


        private async Task Device_MessageSent(object sender, MessageSentEventArgs e)
        {
            if (e.Origin == null || !e.Origin.IsSubclassOf(typeof(MultiView)))
                return;

            this.Messages.Add(e.MessageId);
        }

        public override void Init()
        {
            Device.MessageSent += Device_MessageSent;
        }

        public override async Task Load(MessageResult result)
        {
            _Rendered = false;
        }


        public override async Task Render(MessageResult result)
        {
            //When already rendered, skip rendering
            if (_Rendered)
                return;

            await CleanUpView();

            await RenderView(new RenderViewEventArgs(this.SelectedViewIndex));

            _Rendered = true;
        }


        /// <summary>
        /// Will get invoked on rendering the current controls view.
        /// </summary>
        /// <param name="e"></param>
        public virtual async Task RenderView(RenderViewEventArgs e)
        {


        }

        async Task CleanUpView()
        {

            var tasks = new List<Task>();

            foreach (var msg in this.Messages)
            {
                tasks.Add(this.Device.DeleteMessage(msg));
            }

            await Task.WhenAll(tasks);

            this.Messages.Clear();

        }

        /// <summary>
        /// Forces render of control contents.
        /// </summary>
        public async Task ForceRender()
        {
            await CleanUpView();

            await RenderView(new RenderViewEventArgs(this.SelectedViewIndex));

            _Rendered = true;
        }

        public override async Task Cleanup()
        {
            Device.MessageSent -= Device_MessageSent;

            await CleanUpView();
        }

    }
}
