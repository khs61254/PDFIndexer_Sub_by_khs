using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp3.Journal
{
    public readonly struct JournalMessage
    {
        public readonly JournalLevel Level;
        public readonly string Message;

        public JournalMessage(JournalLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }
}
