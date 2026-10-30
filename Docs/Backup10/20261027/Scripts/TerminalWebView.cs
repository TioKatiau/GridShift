using System;
using UnityEngine;
// Uma implementação precisa fornecer um navegador real; o terminal não simula páginas.
public abstract class TerminalWebView:MonoBehaviour {
 public abstract void Open(string url,Action<string> status);
 public abstract void Hide();
}
