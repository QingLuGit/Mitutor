using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class ScoreContext : BotContext
    {
        public ScoreContext(string userId)
        {
            this.userId = userId;
            this.type = ContextType.ScoreContext;
        }
        public string UserInput = null;
        public bool IsInTesting = false;
        public List<TestItem> taskItems = null;
        public int currentIndex = -1;
    }
}
