/********************************************************
*                                                        *
*   Copyright (c) Microsoft. All rights reserved.        *
*                                                        *
*********************************************************/

namespace Microsoft.WindowsAzure.IntelligentServices.Pronunciation.AlgorithmHelper
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    public class PhonemeEvaluationResult
    {
        public PhonemeEvaluationResult(PhonemeNativeResult phonemeNativeResult)
        {
            Phoneme = phonemeNativeResult.Phoneme;
            Score = phonemeNativeResult.Score;
            StartTime = phonemeNativeResult.StartTime;
            EndTime = phonemeNativeResult.EndTime;
        }

        [DataMember(Name = "phoneme")]
        public string Phoneme { get; set; }

        [DataMember(Name = "score")]
        public double Score { get; set; }

        [DataMember(Name = "startTime")]
        public int StartTime { get; set; }

        [DataMember(Name = "endTime")]
        public int EndTime { get; set; }
    }
}