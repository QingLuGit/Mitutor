using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA.Engine.Data;
using Microsoft.WindowsAzure.IntelligentServices.Pronunciation.AlgorithmHelper;
using System.IO;
using NAudio;
using NAudio.Wave;
using TA.Engine.SCChecker;

namespace TA.Engine.Services
{
    class ScoreSrv
    {
        private const int TESTITEMCOUNT = 3;
        private static SCDBChecker checker = new SCDBChecker();

        public string StartTest(ref ScoreContext scContext)
        {
            string answer = null;
            Init(ref scContext);
            answer = "发送语音消息，读出给定的句子。尽量做到大声嘹亮，富有感情。加油！\r\n";
            answer += string.Format("本次测试共有{0}道题。\r\n", TESTITEMCOUNT);
            answer += GetText(ref scContext);
            return answer;
        }

        public void Init(ref ScoreContext scContext)
        {
            scContext.taskItems = checker.GenerateTestItems(TESTITEMCOUNT);
            scContext.currentIndex = 0;
            scContext.IsInTesting = true;
        }

        public string GenerateReport(ref ScoreContext scContext)
        {
            string answer = null;
            answer = "您的平均分是：";
            double sum = 0;
            foreach (var i in scContext.taskItems)
            {
                sum += i.score;
            }
            answer += (sum / scContext.currentIndex).ToString() + "\r\n";
            answer += "继续努力！";
            return answer;
        }

        public string GetText(ref ScoreContext scContext)
        {
            if (scContext.currentIndex == TESTITEMCOUNT)
            {
                scContext.IsInTesting = false;
                string answer = null;
                answer = "测试结束~\r\n";
                answer += GenerateReport(ref scContext);
                return answer;
            }
            return string.Format("{0}. {1}", (scContext.currentIndex + 1).ToString(), scContext.taskItems.ElementAt(scContext.currentIndex).question);
        }

        public string ReceiveAnswer(ref ScoreContext scContext)
        {
            string answer = null;
            answer = GetScore(ref scContext);
            if (string.IsNullOrEmpty(answer))
            {
                answer += "请发送语音消息，读出给定的句子。\r\n";
            }
            else
            {
                answer += "下一题：\r\n";
                answer += GetText(ref scContext);
            }
            return answer;
        }

        public string GetScore(ref ScoreContext scContext)
        {
            string answer = null;
            string rawScript = scContext.taskItems.ElementAt(scContext.currentIndex).question;
            string script = rawScript.Replace(",", "").Replace("?", "").Replace("!", "").Replace(".", "");
            string mp3Path = scContext.UserInput;
            string wavPath = mp3Path.Replace("mp3", "wav");
            try
            {
                using (Mp3FileReader reader = new Mp3FileReader(mp3Path))
                {
                    using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        WaveFileWriter.CreateWaveFile(wavPath, pcmStream);
                    }
                }
            }
            catch
            {
                return null;
            }
            var result = ScoringHost.Evaluate(
                SupportedLanguage.English,
                script,
                File.ReadAllBytes(wavPath));
            scContext.taskItems.ElementAt(scContext.currentIndex).score = result.Score;
            answer = string.Format("Your score: {0}\r\n", result.Score.ToString());
            scContext.currentIndex++;
            return answer;
        }
    }
}
