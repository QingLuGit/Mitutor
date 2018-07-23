namespace Microsoft.WindowsAzure.IntelligentServices.Pronunciation.AlgorithmHelper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Text;

    /// <summary>
    /// The ScoringHost. Leveraged from the workrole of CaptainService.
    /// </summary>
    public static class ScoringHost
    {
        private static ICollection<string> phonemeDictionary;

        private const int EvaluateResultMinLength = 1024 * 2;

        private const int PhonemeResultMaxLength = 50;

        static ScoringHost()
        {
            //string modelFolder = Directory.GetParent(Directory.GetCurrentDirectory()).FullName.Replace(@"AlgorithmTestConsole\bin", string.Empty) + @"Algorithm\Models\zh-cn";
            string modelFolder = @"D:\repository\Mitutor\Algorithm\Models\en-us";
            Initialize(modelFolder);
        }

        [HandleProcessCorruptedStateExceptions]
        private static void Initialize(string modelFolder)
        {
            Trace.TraceInformation("ScoringHost::Initialize(), Path=" + modelFolder);
            try
            {
                NativeMethods.LoadModel(modelFolder); // alignment by likelihood
                var t = string.Format("HTKfunctions -C {0}", Path.Combine(modelFolder, "hcopy.config"));

                NativeMethods.InitializeMfcExtraction(string.Format("HTKfunctions -C {0}", Path.Combine(modelFolder, "hcopy.config")));
                NativeMethods.InitializeF0Extraction(
                    string.Format("F0Extraction -C {0}", Path.Combine(modelFolder, "getf0.config")));
                phonemeDictionary = new HashSet<string>(ParsePhonemeDictionay(Path.Combine(modelFolder, "evadict")));
                PhonemeConverter.Initialize(Path.Combine(modelFolder, "cmudict"));
            }
            catch (Exception e)
            {
                Trace.TraceError(
                    "ScoringHost::Initialize(), unexpected exception, Message={0}, Stack={1}",
                    e.Message,
                    e.StackTrace);

                throw;
            }
        }

        public static SentenceEvaluationResult Evaluate(SupportedLanguage language, string script, byte[] wavData)
        {
            var wordList = script.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var words = wordList.Select(w => language == SupportedLanguage.English ? w.ToUpper() : w).ToArray();
            foreach (var word in words)
            {
                if (!phonemeDictionary.Contains(word))
                {
                    Trace.TraceError("ScoringHost::Evaluate(), unable to identify the word, Content={0}", word);
                    throw new ArgumentOutOfRangeException(word);
                }
            }

            var mfcData = FeatureGenerator.ExtractMfcFeature(wavData);
            if (mfcData == null || mfcData.Length == 0)
            {
                Trace.TraceError("ScoringHost::Evaluate(), mfc extraction error!");
                throw new ArgumentException("mfc extraction error!");
            }

            double[] f0Data = FeatureGenerator.ExtractF0Feature(wavData);

            if (f0Data == null || f0Data.Length == 0)
            {
                Trace.TraceWarning("ScoringHost::Evaluate(), f0 extraction error!");
                throw new InvalidDataException("f0 extraction error!");
            }

            if (SupportedLanguage.Chinese == language)
            {
                mfcData = FeatureGenerator.AppendF0Feature(f0Data, mfcData);
            }

            IList<string> wordsWithSilence = new List<string>(words);
            wordsWithSilence.Insert(0, "<s>");
            wordsWithSilence.Add("</s>");

            int maxLength = 0;
            if (language == SupportedLanguage.English)
            {
                maxLength = PhonemeResultMaxLength * 12 * words.Length;
            }
            else if (language == SupportedLanguage.Chinese)
            {
                maxLength = PhonemeResultMaxLength * 5 * words.Length;
            }

            if (maxLength < EvaluateResultMinLength)
            {
                maxLength = EvaluateResultMinLength;
            }

            var resultBuffer = new StringBuilder(maxLength);
            var resultLength = NativeMethods.Evaluate(
                    mfcData,
                    string.Empty,
                    resultBuffer,
                    maxLength,
                    wordsWithSilence.ToArray(),
                    wordsWithSilence.Count);

            if (resultLength < 0)
            {
                Trace.TraceError("ScoringHost::Evaluate(), invalid audio content, length of evaluation result: {0}.", resultLength);
                throw new ArgumentException("Invalid audio content.");
            }

            var evaluationResult = resultBuffer.ToString(0, resultLength);
            var phonemeEvaluationResults = evaluationResult.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                return CreateSentenceEvaluationResult(words.Length, phonemeEvaluationResults, f0Data, language);
            }
            catch (ArgumentException)
            {
                Trace.TraceError("ScoringHost::Evaluate(), bad phoneme evaluation result, sentence: {0}, result: {1}", script, evaluationResult);
                throw;
            }
        }

        public static double GetScorePercentage(double score)
        {
            if (score < 0.05)
            {
                return (score / 0.05 * 40);
            }
            else if (score < 0.12)
            {
                return 40 + ((score - 0.05) / 0.07 * 30);
            }
            else if (score < 0.2)
            {
                return 70 + ((score - 0.12) / 0.08 * 20);
            }
            else if (score < 0.36)
            {
                return 90 + ((score - 0.2) / 0.16 * 10);
            }
            else
            {
                return 100.0;
            }
        }

        private static IEnumerable<string> ParsePhonemeDictionay(string dictionaryPath)
        {
            var lines = File.ReadLines(dictionaryPath);
            var enumerator = lines.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();

            while (enumerator.MoveNext())
            {
                var line = enumerator.Current;
                var pos = line.IndexOfAny(new[] { ' ', '\t' });
                if (pos != -1)
                {
                    yield return line.Substring(0, pos);
                }
            }
        }

        private static SentenceEvaluationResult CreateSentenceEvaluationResult(int wordsCount, string[] phonemeNativeResults, double[] f0Data, SupportedLanguage language)
        {
            if (phonemeNativeResults == null || phonemeNativeResults.Length <= 0)
            {
                throw new ArgumentException("Phoneme evaluation result is null or empty.");
            }

            var validPhonemeNativeResults = phonemeNativeResults.Select(l => new PhonemeNativeResult(l)).Where(p => p.IsValid);
            var startWordsCount = validPhonemeNativeResults.Count(n => n.IsStartOfWord);
            if (startWordsCount != wordsCount)
            {
                throw new ArgumentException("Word count of phoneme evaluation result is not equal to original sentence.");
            }

            if (!validPhonemeNativeResults.First().IsStartOfWord)
            {
                throw new ArgumentException("The first phoneme is not start of a word.");
            }

            var enumerator = validPhonemeNativeResults.GetEnumerator();
            List<PhonemeEvaluationResult> wordPhonemeEvaluationResults = null;
            var sentenceEvaluationResult = new SentenceEvaluationResult
            {
                Words = new List<WordEvaluationResult>(),
            };

            var currentWord = string.Empty;

            while (true)
            {
                var eof = !enumerator.MoveNext();

                if ((eof || enumerator.Current.IsStartOfWord) &&
                    (wordPhonemeEvaluationResults != null && wordPhonemeEvaluationResults.Count != 0))
                {
                    var phonemesAvgScore = wordPhonemeEvaluationResults.Average(n => n.Score);
                    sentenceEvaluationResult.Words.Add(
                        new WordEvaluationResult
                        {
                            Score = Math.Round(ScoringHost.GetScorePercentage(phonemesAvgScore), 2),
                            StartTime = wordPhonemeEvaluationResults.First().StartTime,
                            EndTime = wordPhonemeEvaluationResults.Last().EndTime,
                            Word = currentWord,
                            Phonemes = wordPhonemeEvaluationResults,
                        });
                }

                if (eof)
                {
                    break;
                }

                if (enumerator.Current.IsStartOfWord)
                {
                    currentWord = enumerator.Current.Word;
                    wordPhonemeEvaluationResults = new List<PhonemeEvaluationResult>();
                }

                var phonemeEvaluationResult = new PhonemeEvaluationResult(enumerator.Current);

                if (language == SupportedLanguage.English)
                {
                    phonemeEvaluationResult.Phoneme = PhonemeConverter.ConvertToIPA(phonemeEvaluationResult.Phoneme, currentWord, wordPhonemeEvaluationResults.Count());
                }

                wordPhonemeEvaluationResults.Add(phonemeEvaluationResult);
            }

            var wordsAvgScore = sentenceEvaluationResult.Words.Average(w => w.Score);
            sentenceEvaluationResult.Score = Math.Round(wordsAvgScore, 2);
            sentenceEvaluationResult.ToneF0Data = f0Data;

            return sentenceEvaluationResult;
        }
    }
}