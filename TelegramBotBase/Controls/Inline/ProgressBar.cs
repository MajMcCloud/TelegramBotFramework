using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Controls.Inline
{
    /// <summary>
    /// A simple control for show and managing progress.
    /// </summary>
    public class ProgressBar : Base.ControlBase
    {
        public enum eProgressStyle
        {
            standard = 0,
            squares = 1,
            circles = 2,
            lines = 3,
            squaredLines = 4,
            custom = 10
        }

        public eProgressStyle ProgressStyle
        {
            get
            {
                return m_eStyle;
            }
            set
            {
                m_eStyle = value;
                LoadStyle();
            }
        }

        private eProgressStyle m_eStyle = eProgressStyle.standard;


        public int Value
        {
            get
            {
                return this.m_iValue;
            }
            set
            {
                if (value > this.Max)
                {
                    return;
                }

                if (this.m_iValue != value)
                {
                    this.RenderNecessary = true;
                }
                this.m_iValue = value;
            }
        }

        private int m_iValue = 0;

        public int Max
        {
            get
            {
                return this.m_iMax;
            }
            set
            {
                if (this.m_iMax != value)
                {
                    this.RenderNecessary = true;
                }
                this.m_iMax = value;
            }
        }

        private int m_iMax = 100;

        public int? MessageId { get; set; }

        private bool RenderNecessary { get; set; } = false;

        public int Steps
        {
            get
            {
                switch (this.ProgressStyle)
                {
                    case eProgressStyle.standard:

                        return 1;

                    case eProgressStyle.squares:

                        return 10;

                    case eProgressStyle.circles:

                        return 10;

                    case eProgressStyle.lines:

                        return 5;

                    case eProgressStyle.squaredLines:

                        return 5;

                    default:

                        return 1;
                }
            }
        }

        /// <summary>
        /// Filled block (reached percentage)
        /// </summary>
        public String BlockChar
        {
            get; set;
        }

        /// <summary>
        /// Unfilled block (not reached yet)
        /// </summary>
        public String EmptyBlockChar
        {
            get; set;
        }

        /// <summary>
        /// String at the beginning of the progress bar
        /// </summary>
        public String StartChar
        {
            get; set;
        }

        /// <summary>
        /// String at the end of the progress bar
        /// </summary>
        public String EndChar
        {
            get; set;
        }

        public ProgressBar()
        {
            this.ProgressStyle = eProgressStyle.standard;

            this.Value = 0;
            this.Max = 100;

            this.RenderNecessary = true;
        }

        public ProgressBar(int Value, int Max, eProgressStyle Style)
        {
            this.Value = Value;
            this.Max = Max;
            this.ProgressStyle = Style;

            this.RenderNecessary = true;
        }

        public override async Task Cleanup()
        {
            if (this.MessageId == null || this.MessageId == -1)
                return;


            await this.Device.DeleteMessage(this.MessageId.Value);
        }

        public void LoadStyle()
        {
            this.StartChar = "";
            this.EndChar = "";

            switch (this.ProgressStyle)
            {
                case eProgressStyle.circles:

                    this.BlockChar = "⚫️ ";
                    this.EmptyBlockChar = "⚪️ ";

                    break;
                case eProgressStyle.squares:

                    this.BlockChar = "⬛️ ";
                    this.EmptyBlockChar = "⬜️ ";

                    break;
                case eProgressStyle.lines:

                    this.BlockChar = "█";
                    this.EmptyBlockChar = "▁";

                    break;
                case eProgressStyle.squaredLines:

                    this.BlockChar = "▇";
                    this.EmptyBlockChar = "—";

                    this.StartChar = "[";
                    this.EndChar = "]";

                    break;
                case eProgressStyle.standard:
                case eProgressStyle.custom:

                    this.BlockChar = "";
                    this.EmptyBlockChar = "";

                    break;
            }

        }

        public async override Task Render(MessageResult result)
        {
            if (!this.RenderNecessary)
            {
                return;
            }

            if (this.Device == null)
            {
                return;
            }

            String message = "";
            int blocks = 0;
            int maxBlocks = 0;

            switch (this.ProgressStyle)
            {
                case eProgressStyle.standard:

                    message = this.Value.ToString("0") + "%";

                    break;

                case eProgressStyle.squares:
                case eProgressStyle.circles:
                case eProgressStyle.lines:
                case eProgressStyle.squaredLines:
                case eProgressStyle.custom:

                    blocks = (int)Math.Floor((decimal)this.Value / this.Steps);

                    maxBlocks = (this.Max / this.Steps);

                    message += this.StartChar;

                    for (int i = 0; i < blocks; i++)
                    {
                        message += this.BlockChar;
                    }

                    for (int i = 0; i < (maxBlocks - blocks); i++)
                    {
                        message += this.EmptyBlockChar;
                    }

                    message += this.EndChar;

                    message += " " + this.Value.ToString("0") + "%";

                    break;

                default:

                    return;
            }

            if (this.MessageId == null)
            {
                var m = await this.Device.Send(message);

                this.MessageId = m.MessageId;
            }
            else
            {
                await this.Device.Edit(this.MessageId.Value, message);
            }

            this.RenderNecessary = false;
        }

    }
}
