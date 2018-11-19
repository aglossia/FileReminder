using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileReminder
{
    //public partial class FileReminder
    //{
    //    public static FileReminder defaultForm = new FileReminder();

    //    public static FileReminder Default  
    //    {  
    //        get  
    //        {  
    //            if (defaultForm == null)  
    //                defaultForm = new FileReminder();  
    //            return defaultForm;  
    //        }  
    //    }  
    //}

    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(FileReminder.Default);
            Application.Run(new FileReminder());
        }
    }
}
