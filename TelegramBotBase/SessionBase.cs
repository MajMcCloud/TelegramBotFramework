using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Sessions;
namespace TelegramBotBase
{
    public class SessionBase
    {
        public MessageClient Client { get; set; }

        public Dictionary<long, DeviceSession> SessionList { get; set; }


        public SessionBase()
        {
            this.SessionList = new Dictionary<long, DeviceSession>();
        }

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

        public DeviceSession GetSession(long deviceId)
        {
            DeviceSession ds = this.SessionList.FirstOrDefault(a => a.Key == deviceId).Value ?? null;
            return ds;
        }

        public async Task<DeviceSession> StartSession<T>(long deviceId)
            where T : FormBase
        {
            T start = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as T;

            start.Client = this.Client;

            DeviceSession ds = new Sessions.DeviceSession(deviceId, start);

            start.Device = ds;
            await start.Init();

            await start.Opened();

            this[deviceId] = ds;
            return ds;
        }


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
