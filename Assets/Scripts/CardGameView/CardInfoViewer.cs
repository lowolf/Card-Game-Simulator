﻿using System.Collections.Generic;
using CardGameDef;
using CGS;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CardGameView
{
    public class CardInfoViewer : MonoBehaviour, ICardDisplay, IPointerDownHandler, ISelectHandler, IDeselectHandler
    {
        public const string SetLabel = "Set";
        public const float VisibleYMin = 0.625f;
        public const float VisibleYMax = 1;
        public const float HiddenYmin = 1.025f;
        public const float HiddenYMax = 1.4f;
        public const float AnimationSpeed = 5.0f;

        public static CardInfoViewer Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                GameObject cardInfoViewer = GameObject.FindWithTag(Tags.CardInfoViewer);
                _instance = cardInfoViewer?.GetOrAddComponent<CardInfoViewer>();
                return _instance;
            }
        }
        private static CardInfoViewer _instance;

        public RectTransform infoPanel;
        public RectTransform zoomPanel;
        public Image cardImage;
        public Image zoomImage;
        public Text nameText;
        public Text idText;
        public Dropdown propertySelection;
        public Text labelText;
        public Text contentText;

        public List<Dropdown.OptionData> PropertyOptions { get; } = new List<Dropdown.OptionData>();
        public Dictionary<string, string> DisplayNameLookup { get; } = new Dictionary<string, string>();

        public string SelectedPropertyName
        {
            get
            {
                string selectedName = SetLabel;
                if (SelectedPropertyIndex >= 1 && SelectedPropertyIndex < PropertyOptions.Count)
                    DisplayNameLookup.TryGetValue(SelectedPropertyDisplay, out selectedName);
                return selectedName;
            }
        }
        public string SelectedPropertyDisplay
        {
            get
            {
                string selectedDisplay = SetLabel;
                if (SelectedPropertyIndex >= 1 && SelectedPropertyIndex < PropertyOptions.Count)
                    selectedDisplay = PropertyOptions[SelectedPropertyIndex].text;
                return selectedDisplay;
            }
        }
        public int SelectedPropertyIndex
        {
            get { return _selectedPropertyIndex; }
            set
            {
                _selectedPropertyIndex = value;
                if (_selectedPropertyIndex < 0)
                    _selectedPropertyIndex = PropertyOptions.Count - 1;
                if (_selectedPropertyIndex >= PropertyOptions.Count)
                    _selectedPropertyIndex = 0;
                propertySelection.value = _selectedPropertyIndex;
                labelText.text = SelectedPropertyDisplay;
                SetContentText();
            }
        }
        private int _selectedPropertyIndex;

        public CardModel SelectedCardModel
        {
            get { return _selectedCardModel; }
            set
            {
                if (_selectedCardModel != null)
                {
                    _selectedCardModel.IsHighlighted = false;
                    _selectedCardModel.Value.UnregisterDisplay(this);
                }

                _selectedCardModel = value;

                if (_selectedCardModel != null)
                {
                    Card selectedCard = _selectedCardModel.Value;
                    nameText.text = selectedCard.Name;
                    idText.text = selectedCard.Id;
                    SetContentText();
                    selectedCard.RegisterDisplay(this);
                }
                IsVisible = _selectedCardModel != null;
            }
        }
        private CardModel _selectedCardModel;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                if (!_isVisible && zoomPanel != null)
                    zoomPanel.gameObject.SetActive(false);
                if (SelectedCardModel != null)
                    SelectedCardModel.IsHighlighted = _isVisible;
            }
        }
        private bool _isVisible;
        public bool WasVisible => infoPanel.anchorMax.y < (HiddenYMax + VisibleYMax) / 2.0f;

        void Start()
        {
            ResetInfo();
        }

        void Update()
        {
            if (IsVisible && EventSystem.current.currentSelectedGameObject == null && !EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(gameObject);

            infoPanel.anchorMin = IsVisible ?
                new Vector2(infoPanel.anchorMin.x, Mathf.Lerp(infoPanel.anchorMin.y, VisibleYMin, AnimationSpeed * Time.deltaTime)) :
                new Vector2(infoPanel.anchorMin.x, Mathf.Lerp(infoPanel.anchorMin.y, HiddenYmin, AnimationSpeed * Time.deltaTime));
            infoPanel.anchorMax = IsVisible ?
                new Vector2(infoPanel.anchorMax.x, Mathf.Lerp(infoPanel.anchorMax.y, VisibleYMax, AnimationSpeed * Time.deltaTime)) :
                new Vector2(infoPanel.anchorMax.x, Mathf.Lerp(infoPanel.anchorMax.y, HiddenYMax, AnimationSpeed * Time.deltaTime));
        }

        void LateUpdate()
        {
            if (!IsVisible || SelectedCardModel == null || !Input.anyKeyDown || CardGameManager.TopMenuCanvas != null)
                return;

            if ((Input.GetKeyDown(Inputs.BluetoothReturn) || Input.GetButtonDown(Inputs.Submit)) && SelectedCardModel.DoubleClickAction != null)
                SelectedCardModel.DoubleClickAction(SelectedCardModel);
            else if (Input.GetButtonDown(Inputs.Page))
            {
                if (Input.GetAxis(Inputs.Page) > 0)
                    IncrementProperty();
                else
                    DecrementProperty();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(Inputs.Cancel))
                SelectedCardModel = null;
        }

        public void ResetInfo()
        {
            cardImage.gameObject.GetOrAddComponent<AspectRatioFitter>().aspectRatio = CardGameManager.Current.CardAspectRatio;
            zoomPanel.GetChild(0).gameObject.GetOrAddComponent<AspectRatioFitter>().aspectRatio = CardGameManager.Current.CardAspectRatio;

            int selectedPropertyIndex = 0;
            PropertyOptions.Clear();
            PropertyOptions.Add(new Dropdown.OptionData() { text = SetLabel });
            DisplayNameLookup.Clear();
            foreach (PropertyDef propDef in CardGameManager.Current.CardProperties)
            {
                string displayName = !string.IsNullOrEmpty(propDef.Display) ? propDef.Display : propDef.Name;
                PropertyOptions.Add(new Dropdown.OptionData() { text = displayName });
                DisplayNameLookup[displayName] = propDef.Name;
                if (propDef.Name.Equals(CardGameManager.Current.CardPrimaryProperty))
                    selectedPropertyIndex = PropertyOptions.Count - 1;
            }
            propertySelection.options = PropertyOptions;
            propertySelection.value = selectedPropertyIndex;
            propertySelection.onValueChanged.Invoke(selectedPropertyIndex);
        }

        public void DecrementProperty()
        {
            SelectedPropertyIndex--;
        }

        public void IncrementProperty()
        {
            SelectedPropertyIndex++;
        }

        public void SetContentText()
        {
            Set currentSet = null;
            if (SelectedCardModel != null)
                contentText.text = SelectedPropertyIndex != 0 ?
                    SelectedCardModel.Value.GetPropertyValueString(SelectedPropertyName)
                    : (CardGameManager.Current.Sets.TryGetValue(SelectedCardModel.Value.SetCode, out currentSet)
                        ? currentSet : Set.Default).ToString();
            else
                contentText.text = string.Empty;
        }

        public void SetImageSprite(Sprite imageSprite)
        {
            cardImage.sprite = imageSprite ?? CardGameManager.Current.CardBackImageSprite;
            zoomImage.sprite = imageSprite ?? CardGameManager.Current.CardBackImageSprite;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            IsVisible = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!zoomPanel.gameObject.activeSelf)
                IsVisible = false;
        }

        public void ShowCardZoomed(CardModel cardModel)
        {
            SelectedCardModel = cardModel;
            ShowCardZoomed();
        }

        public void ShowCardZoomed()
        {
            zoomPanel.gameObject.SetActive(true);
        }

        public void HideCardZoomed()
        {
            if (SwipeManager.IsSwiping())
                return;
            zoomPanel.gameObject.SetActive(false);
        }
    }
}
