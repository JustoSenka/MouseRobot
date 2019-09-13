using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.CustomControls
{
    public class ToolStripToggleButton : ToolStripButton
    {
        private bool m_Active;
        public bool Active
        {
            get
            {
                return m_Active;
            }
            set
            {
                if (m_Active != value)
                {
                    m_Active = value;
                    ActiveStateChanged?.Invoke();
                }
            }
        }

        public Image ImageActive { get; set; }
        public Image ImageNotActive { get; set; }

        public event Action ActiveStateChanged;

        public ToolStripToggleButton() : base() { }
        public ToolStripToggleButton(Image image) : base(image) { }
        public ToolStripToggleButton(string text, Image image) : base(text, image) { }
        public ToolStripToggleButton(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
        public ToolStripToggleButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }

        // TODO: fix other constructors to work in the same way
        public ToolStripToggleButton(string text) : base(text)
        {
            ActiveStateChanged += OnActiveStateChanged;
            Click += ToggleButtonImage;
            Active = true;
        }

        private void OnActiveStateChanged()
        {
            if (Active)
                Image = ImageActive;
            else
                Image = ImageNotActive;
        }

        private void ToggleButtonImage(object sender, EventArgs e)
        {
            Active = !Active;
        }
    }
}
