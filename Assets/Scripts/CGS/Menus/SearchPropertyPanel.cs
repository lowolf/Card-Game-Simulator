﻿using CardGameDef;
using UnityEngine;
using UnityEngine.UI;

namespace CGS.Menus
{
    public class SearchPropertyPanel : MonoBehaviour
    {
        public Text nameLabelText;
        public InputField stringInputField;
        public Text stringPlaceHolderText;
        public InputField integerMinInputField;
        public InputField integerMaxInputField;
        public RectTransform enumContent;
        public Toggle enumToggle;
    }
}