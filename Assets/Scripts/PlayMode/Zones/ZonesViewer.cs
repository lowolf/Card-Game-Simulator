﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZonesViewer : MonoBehaviour
{
    public const float Width = 350f;
    public const float Height = 300f;
    public const float ScrollbarWidth = 80f;

    public GameObject extraZonePrefab;
    public GameObject discardZonePrefab;
    public GameObject deckZonePrefab;
    public GameObject handZonePrefab;
    public GameObject resultsZonePrefab;
    public ScrollRect scrollView;
    public GameObject extendButton;
    public GameObject showButton;
    public GameObject hideButton;

    public StackedZone Discard { get; private set; }
    public StackedZone CurrentDeck { get; private set; }
    public ExtensibleCardZone Hand { get; private set; }
    public ExtensibleCardZone Results { get; private set; }

    protected List<ExtensibleCardZone> AllZones { get; } = new List<ExtensibleCardZone>();

    void Start()
    {
        if (CardGameManager.Current.GameHasDiscardZone)
            CreateDiscard();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!gameObject.activeInHierarchy)
            return;
        IsExtended = IsExtended;
        IsVisible = IsVisible;
        ResizeContent();
    }

    public void ResizeContent()
    {
        float height = Height;
        foreach (ExtensibleCardZone zone in AllZones)
            height += ((RectTransform)zone.transform).rect.height;
        scrollView.content.sizeDelta = new Vector2(scrollView.content.sizeDelta.x, height);
    }
    
    public void CreateDiscard()
    {
        Discard = Instantiate(discardZonePrefab, scrollView.content).GetComponent<StackedZone>();
        Discard.Viewer = this;
        AllZones.Add(Discard);
        ResizeContent();
    }

    public void CreateExtraZone(string name, List<Card> cards)
    {
        ExtensibleCardZone extraZone = Instantiate(extraZonePrefab, scrollView.content).GetComponent<ExtensibleCardZone>();
        extraZone.labelText.text = name;
        foreach (Card card in cards)
            extraZone.AddCard(card);
        extraZone.Viewer = this;
        AllZones.Add(extraZone);
        ResizeContent();
    }

    public void CreateDeck()
    {
        CurrentDeck = Instantiate(deckZonePrefab, scrollView.content).GetComponent<StackedZone>();
        CurrentDeck.Viewer = this;
        AllZones.Add(CurrentDeck);
        ResizeContent();
    }

    public void CreateHand()
    {
        if (Hand != null)
            return;

        Hand = Instantiate(handZonePrefab, scrollView.content).GetComponent<ExtensibleCardZone>();
        Hand.Viewer = this;
        AllZones.Add(Hand);
        ResizeContent();
        IsExtended = true;
        IsVisible = true;
    }

    public void CreateResults()
    {
        if (Results != null)
            return;

        Results = Instantiate(resultsZonePrefab, scrollView.content).GetComponent<ExtensibleCardZone>();
        Results.Viewer = this;
        AllZones.Add(Results);
        ResizeContent();
    }

    public bool IsExtended {
        get {
            return ((RectTransform)scrollView.transform.parent).anchoredPosition.x < 1 ;
        }
        set {
            RectTransform.Edge edge = RectTransform.Edge.Right;
            float size = Width;
            float inset = value ? 0 : -(size - ScrollbarWidth);
            ((RectTransform)scrollView.transform.parent ).SetInsetAndSizeFromParentEdge(edge, inset, size);
            foreach (ExtensibleCardZone zone in AllZones)
                zone.ResizeExtension();
            extendButton.SetActive(!IsExtended);
            showButton.SetActive(IsExtended && !IsVisible);
            hideButton.SetActive(IsExtended && IsVisible);
        }
    }

    public bool IsVisible {
        get { return scrollView.gameObject.activeSelf; }
        set {
            scrollView.gameObject.SetActive(value);
            RectTransform.Edge edge = RectTransform.Edge.Top;
            float size = GetComponent<RectTransform>().rect.height;
            ((RectTransform)scrollView.transform.parent).SetInsetAndSizeFromParentEdge(edge, 0, scrollView.gameObject.activeSelf ? size : ScrollbarWidth);
            extendButton.SetActive(!IsExtended);
            showButton.SetActive(IsExtended && !IsVisible);
            hideButton.SetActive(IsExtended && IsVisible);
        }
    }
}
