using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Erenshor_Tracker.Core.Classes
{
    internal class Drawer : MelonMod
    {
        public static void DrawVersionText()
        {
            GUI.Label(new Rect(5, Screen.height - 20, 1000, 200), $"<b><color=cyan><size=15>Erenshor Tracker v0.1.0</size></color></b>");
        }
    }
}
