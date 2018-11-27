using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Microsoft.VisualBasic.ApplicationServices;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace FileReminder
{ 

    class Program : MarshalByRefObject
    {
        static FileReminder f;

        [STAThread]
        static void Main(string[] args)
        {
            Program p = new Program();

            string mutexName = "FileReminder";

            bool createdNew;
            System.Threading.Mutex mutex =
                new System.Threading.Mutex(true, mutexName, out createdNew);

            if (createdNew)
            {
                ChannelServices.RegisterChannel(new IpcServerChannel(Application.ProductName), true);

                RemotingServices.Marshal(p, "open");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(f = new FileReminder());
            }
            else
            {
                mutex.Close();

                ChannelServices.RegisterChannel(new IpcClientChannel(), true);

                p = Activator.GetObject(typeof(Program), "ipc://" + Application.ProductName + "/open") as Program;

                if (p.StartupNextInstance(args))
                {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new FileReminder());
                }
            }

            mutex.Close();
        }

        public bool StartupNextInstance(string[] args)
        {
            if(f.saveConfig.startup_windowFlg)
            {
                return true;
            }
            else
            {
                f.Invoke((Action)delegate
                {
                    f.fremRead(args[0]);
                });
                return false;
            }
        }
    }
}
