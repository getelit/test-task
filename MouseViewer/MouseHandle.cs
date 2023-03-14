using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;

namespace MouseViewer
{
    //public class MouseEventHandler
    //{
    //    [DllImport("user32.dll", SetLastError = true)]
    //    public static extern bool SetCursorPos(int X, int Y);
    //    private IKeyboardMouseEvents m_Events;
    //    private int lastXPos, lastYPos;

    //    public void Subscribe(IKeyboardMouseEvents events)
    //    {
    //        m_Events = events;
    //        m_Events.MouseMove += OnMouseMove;
    //        m_Events.MouseClick += OnMouseClick;
    //    }

    //    public void Unsubscribe()
    //    {
    //        if (m_Events == null) return;
    //        m_Events.MouseMove -= OnMouseMove;
    //        m_Events.MouseClick -= OnMouseClick;
    //        m_Events.Dispose();
    //        m_Events = null;
    //    }

    //    private void OnMouseMove(object sender, MouseEventArgs e)
    //    {
    //        int currentXPos = e.X, currentYPos = e.Y;

    //        if (Math.Abs(currentXPos - lastXPos) >= 10 ||
    //            Math.Abs(currentYPos - lastYPos) >= 10)
    //        {
    //            MainWindow.tbCoords.Text = string.Format("x={0:0000}; y={1:0000}", e.X, e.Y);

    //            lastXPos = currentXPos;
    //            lastYPos = currentYPos;
    //        }
    //    }

    //    private void OnMouseClick(object sender, MouseEventArgs e)
    //    {
    //        switch (e.Button)
    //        {
    //            case MouseButtons.Left:
    //                MainWindow.tbMouseEvent.Text = "LMB";
    //                break;
    //            case MouseButtons.Right:
    //                MainWindow.tbMouseEvent.Text = "RMB";
    //                break;
    //            case MouseButtons.Middle:
    //                MainWindow.tbMouseEvent.Text = "MMB";
    //                break;
    //        }
    //    }
    //}



    //internal class MouseHandle : MainWindow
    //{
    //    [DllImport("user32.dll", SetLastError = true)]
    //    public static extern bool SetCursorPos(int X, int Y);
    //    private IKeyboardMouseEvents m_Events;
    //    // private string coords;

    //    private void Subscribe(IKeyboardMouseEvents events)
    //    {
    //        m_Events = events;
    //        m_Events.MouseMove += M_Events_MouseMove;
    //        m_Events.MouseClick += M_Events_MouseClick;
    //    }

    //    private void Unsubscribe()
    //    {
    //        if (m_Events == null) return;
    //        m_Events.MouseMove -= M_Events_MouseMove;
    //        m_Events.MouseClick -= M_Events_MouseClick;
    //        m_Events.Dispose();
    //        m_Events = null;
    //    }

    //    int lastXPos, lastYPos;

    //    public void M_Events_MouseMove(object sender, MouseEventArgs e)
    //    {
    //        int currentXPos = e.X, currentYPos = e.Y;

    //        if (Math.Abs(currentXPos - lastXPos) >= 10 ||
    //            Math.Abs(currentYPos - lastYPos) >= 10)
    //        {
    //            tbCoords.Text = string.Format("x={0:0000}; y={1:0000}", e.X, e.Y);

    //            lastXPos = currentXPos;
    //            lastYPos = currentYPos;
    //        }
    //    }

    //    public void M_Events_MouseClick(object sender, MouseEventArgs e)
    //    {
    //        switch (e.Button)
    //        {
    //            case MouseButtons.Left:
    //                tbMouseEvent.Text = "LMB";
    //                break;
    //            case MouseButtons.Right:
    //                tbMouseEvent.Text = "RMB";
    //                break;
    //            case MouseButtons.Middle:
    //                tbMouseEvent.Text = "MMB";
    //                break;
    //        }
    //    }


    //    public void MainWin_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        Unsubscribe();
    //        Subscribe(Hook.GlobalEvents());
    //    }

    //    public void MainWin_ContextMenuClosing(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    //    {
    //        Unsubscribe();
    //    }
    //}
}

