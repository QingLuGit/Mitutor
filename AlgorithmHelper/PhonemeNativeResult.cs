/********************************************************
*                                                        *
*   Copyright (c) Microsoft. All rights reserved.        *
*                                                        *
*********************************************************/

namespace Microsoft.WindowsAzure.IntelligentServices.Pronunciation.AlgorithmHelper
{
    using System;
    using System.Linq;

    public class PhonemeNativeResult
    {
        private static readonly string[] SilenceFlags = { "sil", "sil[2]", "sil[4]", "silst" };

        private readonly string[] columns;

        public PhonemeNativeResult(string line)
        {
            this.columns = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool IsValid
        {
            get
            {
                if (this.columns.Length < 4)
                {
                    return false;
                }

                return SilenceFlags.All(flag => String.CompareOrdinal(this.Phoneme, flag) != 0);
            }
        }

        public float Score
        {
            get
            {
                return float.Parse(this.columns[3]);
            }
        }

        public int StartTime
        {
            get
            {
                return int.Parse(this.columns[0]) / 100000;
            }
        }

        public int EndTime
        {
            get
            {
                return int.Parse(this.columns[1]) / 100000;
            }
        }

        public string Phoneme
        {
            get
            {
                return this.columns[2];
            }
        }

        public bool IsStartOfWord
        {
            get
            {
                if (!this.IsValid)
                {
                    return false;
                }

                return this.columns.Length == 5;
            }
        }

        public string Word
        {
            get
            {
                if (this.IsStartOfWord)
                {
                    return this.columns[4];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}