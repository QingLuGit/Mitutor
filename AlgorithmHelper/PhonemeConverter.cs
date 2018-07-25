namespace Microsoft.WindowsAzure.IntelligentServices.Pronunciation.AlgorithmHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class PhonemeConverter
    {
        #region Phoneme Mapping Table
        //The table is from http://en.wikipedia.org/wiki/Arpabet
        private static readonly Dictionary<string, string> phonesMap = new Dictionary<string, string>()
        {
            {"AO", "ɔ"},
            {"AA", "ɑ"},
            {"IY", "i"},
            {"UW", "u"},
            {"EH", "ɛ"},
            {"IH", "ɪ"},
            {"UH", "ʊ"},
            {"AH0", "ə"},
            {"AH1", "ʌ"},
            {"AH2", "ʌ"},
            {"AX", "ə"},
            {"AE", "æ"},
            {"EY", "eɪ"},
            {"AY", "aɪ"},
            {"OW", "oʊ"},
            {"AW", "aʊ"},
            {"OY", "ɔɪ"},
            {"ER", "ɝ"},
            {"AXR", "ɚ"},
            {"EHR", "ɛr"},
            {"UHR", "ʊr"},
            {"AOR", "ɔr"},
            {"AAR", "ɑr"},
            {"IHR", "ɪr"},
            {"IYR", "ɪr"},
            {"AWR", "aʊr"},
            {"P", "p"},
            {"B", "b"},
            {"T", "t"},
            {"D", "d"},
            {"K", "k"},
            {"G", "ɡ"},
            {"CH", "tʃ"},
            {"JH", "dʒ"},
            {"F", "f"},
            {"V", "v"},
            {"TH", "θ"},
            {"DH", "ð"},
            {"S", "s"},
            {"Z", "z"},
            {"SH", "ʃ"},
            {"ZH", "ʒ"},
            {"HH", "h"},
            {"M", "m"},
            {"EM", "m̩"},
            {"N", "n"},
            {"EN", "n̩"},
            {"NG", "ŋ"},
            {"ENG", "ŋ̍"},
            {"L", "ɫ"},
            {"EL", "ɫ̩"},
            {"R", "r"},
            {"DX", "ɾ"},
            {"NX", "ɾ̃"},
            {"Y", "j"},
            {"W", "w"},
            {"Q", "ʔ"}
        };

        #endregion

        private static Dictionary<string, string[]> dictWord2Phonemes = null;

        public static void Initialize(string cmuDictFilePath)
        {
            if (File.Exists(cmuDictFilePath))
            {
                dictWord2Phonemes = new Dictionary<string, string[]>();
                using (StreamReader sr = new StreamReader(cmuDictFilePath))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        dictWord2Phonemes.Add(parts[0], parts.Skip(1).ToArray());
                    }
                }
            }
        }

        public static string ConvertToIPA(string phoneme, string word, int index)
        {
            phoneme = phoneme.ToUpperInvariant();           

            //"AH" is special case, that "AH0" and "AH1" map to difference IPA phonemes
            var specialPhoneme = "AH";
            if (phoneme == specialPhoneme)
            {
                if (dictWord2Phonemes.ContainsKey(word))
                {
                    phoneme = dictWord2Phonemes[word][index];
                    if (!phoneme.StartsWith(specialPhoneme))
                    {
                        //remove number for other phonemes
                        phoneme = Regex.Replace(phoneme, @"[\d-]", string.Empty);
                    }
                }
                else
                {
                    phoneme = "AH0";
                }
            }

            if (phonesMap.ContainsKey(phoneme))
            {
                return phonesMap[phoneme];
            }
            else
            {
                throw new InvalidDataException( string.Format("input phoneme not exist in phoneme map! phoneme input: {0}", phoneme));
            }
        }
    }
}
