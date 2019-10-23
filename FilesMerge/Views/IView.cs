using System;
using System.Collections.Generic;

namespace FilesMerge.Views
{
    interface IView
    {
        event Action<string> OpenFolder_Click;
        event Action ShowMarginFIle_Click;
        event Action<string> NodeSelectChange;
        event Action<bool> TextMargedChanged;
        event Action<string> Save;
        void Run();
        void ShowMarginText(string marginText);
        void ShowFileName(List<string> fileNames);
        string GetChangedText();
    }
}
