using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA.Engine.Data;
using TA.Engine.Services;

namespace TA.Engine.Engines
{
    public class ScoreEngine
    {
        private ScoreSrv scSrv = new ScoreSrv();
        public string Answer(string userId, ref ScoreContext scContext)
        {
            string answer = null;
            switch (scContext.Intent)
            {
                case "Score":
                    if (!scContext.IsInTesting)
                    {
                        answer = this.scSrv.StartTest(ref scContext);
                    }
                    else
                    {
                        answer = this.scSrv.ReceiveAnswer(ref scContext);
                    }
                    break;
            }
            return answer;
        }
    }






}
