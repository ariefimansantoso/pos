using System;

namespace Pos.Desktop.Controls
{
    public class SelectedItemEventArgs : EventArgs
    {
        public object Id { set; get; }
        public SelectedItemEventArgs(object id)
        {
            Id = id;
        }
        public SelectedItemEventArgs()
        {
            Id = 0;
        }
    }
}
