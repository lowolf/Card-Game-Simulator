﻿using System.Collections.Generic;
using CardGameDef;
using CardGameView;
using CGS.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace CGS.EditDeck
{
    public class SearchResults : MonoBehaviour
    {
        public const string EmptyFilterText = "*";

        public GameObject cardSearchMenuPrefab;
        public DeckEditor deckEditor;
        public RectTransform layoutArea;
        public InputField nameInputField;
        public Text filtersText;
        public Text countText;

        public int CardsPerPage => Mathf.FloorToInt(layoutArea.rect.width / 
        (CardGameManager.PixelsPerInch * CardGameManager.Current.CardSize.x + layoutArea.gameObject.GetOrAddComponent<HorizontalLayoutGroup>().spacing));
        public int TotalPageCount => CardsPerPage == 0 ? 0 : (AllResults.Count / CardsPerPage) + ((AllResults.Count % CardsPerPage) == 0 ? -1 : 0);

        public CardSearchMenu CardSearcher => _cardSearcher ??
                                              (_cardSearcher = Instantiate(cardSearchMenuPrefab).GetOrAddComponent<CardSearchMenu>());

        public int CurrentPageIndex { get; set; }

        private CardSearchMenu _cardSearcher;
        private List<Card> _allResults;

        void OnEnable()
        {
            CardSearcher.SearchCallback = ShowResults;
            CardGameManager.Instance.OnSceneActions.Add(CardSearcher.ClearSearch);
        }

        public string SetNameInputField(string nameFilter)
        {
            nameInputField.text = nameFilter;
            return nameInputField.text;
        }

        public void SetNameFilter(string nameFilter)
        {
            CardSearcher.SetNameFilter(nameFilter);
        }

        public void ClearSearchName()
        {
            CardSearcher.ClearSearchName();
        }

        public void Search()
        {
            CardSearcher.Search();
        }

        public void PageLeft()
        {
            if (!CardInfoViewer.Instance.zoomPanel.gameObject.activeSelf)
                CardInfoViewer.Instance.SelectedCardModel = null;
            CurrentPageIndex--;
            if (CurrentPageIndex < 0)
                CurrentPageIndex = TotalPageCount;
            UpdateSearchResultsPanel();
        }

        public void PageRight()
        {
            if (!CardInfoViewer.Instance.zoomPanel.gameObject.activeSelf)
                CardInfoViewer.Instance.SelectedCardModel = null;
            CurrentPageIndex++;
            if (CurrentPageIndex > TotalPageCount)
                CurrentPageIndex = 0;
            UpdateSearchResultsPanel();
        }

        public void UpdateSearchResultsPanel()
        {
            layoutArea.DestroyAllChildren();

            for (int i = 0; i < CardsPerPage && CurrentPageIndex >= 0 && CurrentPageIndex * CardsPerPage + i < AllResults.Count; i++)
            {
                string cardId = AllResults[CurrentPageIndex * CardsPerPage + i].Id;
                if (!CardGameManager.Current.Cards.ContainsKey(cardId))
                    continue;
                Card cardToShow = CardGameManager.Current.Cards[cardId];
                CardModel cardModelToShow = Instantiate(deckEditor.cardModelPrefab, layoutArea).GetOrAddComponent<CardModel>();
                cardModelToShow.Value = cardToShow;
                cardModelToShow.DoesCloneOnDrag = true;
                if (((RectTransform)deckEditor.transform).rect.width > ((RectTransform)deckEditor.transform).rect.height)
                    cardModelToShow.DoubleClickAction = deckEditor.AddCardModel;
                else
                    cardModelToShow.DoubleClickAction = CardInfoViewer.Instance.ShowCardZoomed;
            }

            countText.text = (CurrentPageIndex + 1) + "/" + (TotalPageCount + 1);
        }

        public void ShowSearchMenu()
        {
            CardSearcher.Show(SetNameInputField, ShowResults);
        }

        public void ShowResults(string filters, List<Card> results)
        {
            if (string.IsNullOrEmpty(filters))
                filters = EmptyFilterText;
            filtersText.text = filters;

            AllResults = results;
        }

        public List<Card> AllResults
        {
            get { return _allResults ?? (_allResults = new List<Card>()); }
            set
            {
                _allResults = value;
                CurrentPageIndex = 0;
                UpdateSearchResultsPanel();
            }
        }
    }
}
