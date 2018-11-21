using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using FileReminder.utility;
using System.Diagnostics; //process
using System.Runtime.InteropServices;

namespace FileReminder
{
    public partial class FileReminder
    {
        public class reminderTab
        {
            public List<TabPage> tabs { get; set; }
            public List<ListBox> listboxs { get; set; }

            private FileReminder parent;

            public reminderTab(FileReminder parent)
            {
                tabs = new List<TabPage>();
                listboxs = new List<ListBox>();

                this.parent = parent;
            }

            public void tabInit(int tabNum)
            {
                parent.tabControl1.Controls.Add(tabs[tabNum]);

                this.tabs[tabNum].Controls.Add(listboxs[tabNum]);

                this.tabs[tabNum].AllowDrop = true;


                this.tabs[tabNum].Location = new System.Drawing.Point(4, 22);
                //this.tabs[tabNum].Name = "tabs[tabNum]";
                this.tabs[tabNum].Padding = new System.Windows.Forms.Padding(3);
                this.tabs[tabNum].Size = new System.Drawing.Size(402, 313);
                this.tabs[tabNum].TabIndex = 0;
                //this.tabs[tabNum].Text = "tabs[tabNum]";
                this.tabs[tabNum].UseVisualStyleBackColor = true;

                this.tabs[tabNum].ContextMenuStrip = parent.contextMenuStrip2;


                this.listboxs[tabNum].AllowDrop = true;
                this.listboxs[tabNum].Dock = System.Windows.Forms.DockStyle.Fill;
                this.listboxs[tabNum].Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
                this.listboxs[tabNum].FormattingEnabled = true;
                this.listboxs[tabNum].HorizontalScrollbar = true;
                this.listboxs[tabNum].ItemHeight = 24;
                this.listboxs[tabNum].Location = new System.Drawing.Point(3, 3);
                this.listboxs[tabNum].Name = "listboxs[tabNum]";
                this.listboxs[tabNum].SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
                this.listboxs[tabNum].Size = new System.Drawing.Size(396, 307);
                this.listboxs[tabNum].TabIndex = 0;





                //this.listboxs[tabNum].Dock = DockStyle.Fill;
                //this.listboxs[tabNum].AllowDrop = true;
                this.listboxs[tabNum].ContextMenuStrip = parent.contextMenuStrip1;


                this.listboxs[tabNum].MouseWheel += new System.Windows.Forms.MouseEventHandler(parent.listBox_MouseWheel); 
                this.listboxs[tabNum].DragEnter += new DragEventHandler(parent.listBox_DragEnter);
                this.listboxs[tabNum].DragDrop += new DragEventHandler(parent.listBox_DragDrop);
                this.listboxs[tabNum].MouseDoubleClick += new MouseEventHandler(parent.listBox_MouseDoubleClick);
                this.listboxs[tabNum].KeyUp += new KeyEventHandler(parent.listBox_KeyUp);
                this.listboxs[tabNum].KeyDown += new KeyEventHandler(parent.listBox_KeyDown);
                this.listboxs[tabNum].MouseMove += new MouseEventHandler(parent.listBox_MouseMove);
                this.listboxs[tabNum].MouseLeave += new EventHandler(parent.listBox_MouseLeave);
            }
        }
    }
}
