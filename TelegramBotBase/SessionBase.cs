using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Sessions;
namespace TelegramBotBase
{
    /// <summary>
    /// Base class for managing all active sessions
    /// </summary>
    public class SessionBase
    {
        public MessageClient Client { get; set; }

        public Dictionary<long, DeviceSession> SessionList { get; set; }


        public SessionBase()
        {
            this.SessionList = new Dictionary<long, DeviceSession>();
        }

        /// <summary>
        /// Get device session from Device/ChatId
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DeviceSession this[long key]
        {
            get
            {
                return this.SessionList[key];
            }
            set
            {
                this.SessionList[key] = value;
            }
        }

        /// <summary>
        /// Get device session from Device/ChatId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public DeviceSession GetSession(long deviceId)
        {
            DeviceSession ds = this.SessionList.FirstOrDefault(a => a.Key == deviceId).Value ?? null;
            return ds;
        }

        /// <summary>
        /// Start a new session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<DeviceSession> StartSession<T>(long deviceId)
            where T : FormBase
        {
            T start = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as T;

            start.Client = this.Client;

            DeviceSession ds = new Sessions.DeviceSession(deviceId, start);

            start.Device = ds;
            await start.OnInit(new InitEventArgs());

            await start.OnOpened(new EventArgs());

            this[deviceId] = ds;
            return ds;
        }

        /// <summary>
        /// End session
        /// </summary>
        /// <param name="deviceId"></param>
        public void EndSession(long deviceId)
        {
            var d = this[deviceId];
            if (d != null)
            {
                this.SessionList.Remove(deviceId);
                
            }
        }

    }
}
