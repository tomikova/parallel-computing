using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Philosophers
{
    class Fork
    {
        bool dirty = true;

        bool _haveFork = false;

        public Fork(bool haveFork)
        {
            this._haveFork = haveFork;
        }

        public bool haveFork
        {
            get { return this._haveFork; }
            set { this._haveFork = value; }
        }

        public bool Dirty
        {
            get { return this.dirty; }
        }

        public void CLeanFork()
        {
            this.dirty = false;
        }

        public void DirtyFork()
        {
            this.dirty = true;
        }

    }
}
