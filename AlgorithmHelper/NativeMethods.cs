namespace Microsoft.WindowsAzure.IntelligentServices.Pronunciation.AlgorithmHelper
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NativeMethods
    {
        [DllImport("DNN_HMM.dll", EntryPoint = "LoadModel", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int LoadModel(string modelFolder);

        [DllImport("FeatureExtraction.dll", EntryPoint = "Initialization", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitializeMfcExtraction(string command);

        [DllImport("F0Extraction.dll", EntryPoint = "F0ExtractionInitialization", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitializeF0Extraction(string command);

        [DllImport("FeatureExtraction.dll", EntryPoint = "FeatureExtractionFromMemory", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ExtractMfcFeature(byte[] intputData, int length, out IntPtr mfcPointer);

        [DllImport("F0Extraction.dll", EntryPoint = "F0ExtractionFromMemory", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ExtractF0Feature(byte[] inputData, int length, out IntPtr f0Pointer);

        [DllImport("DNN_HMM.dll", EntryPoint = "EvaluateStrictBoundary", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Evaluate(
            byte[] mfcData,
            string trackingID,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder resultBuffer,
            int resultBufferLength,
            [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] words,
            int wordsLength,
            bool product = false);
    }
}