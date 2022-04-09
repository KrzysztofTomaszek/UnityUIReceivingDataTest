using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour
{
    DataServerMock serverMock = new DataServerMock();
    IList<DataItem> notesData = new List<DataItem>();
    
    [SerializeField] Sprite badgeRedSprite;
    [SerializeField] Sprite badgeBlueSprite;
    [SerializeField] Sprite badgeGreenSprite;
    [SerializeField] Sprite glowSprite;
    [SerializeField] Button btnNext;
    [SerializeField] Button btnPrevious;
    [SerializeField] GameObject LoadingGO;
    [SerializeField] GameObject[] notesGO = new GameObject[5];

    TextMeshProUGUI[] notesText = new TextMeshProUGUI[5];
    SpriteRenderer[] notesBadge = new SpriteRenderer[5];
    SpriteRenderer[] notesGlow = new SpriteRenderer[5];

    CancellationTokenSource source = new CancellationTokenSource();
    CancellationToken token = new CancellationToken();
       
    int noteIndex;
    int notesSize;
    int notesCount;

    private void Awake()
    {
        token = source.Token;
        notesSize = 5;
        noteIndex = 0;
        GetCount();
        GetGlow();
        GetBadge();
        GetText();
    }

    async void GetCount()
    {
        notesCount = await serverMock.DataAvailable(token);
        GetNotes(noteIndex, notesSize);
    }

    async void GetNotes(int startIndex, int count)
    {        
        ButtonUpdate();
        notesData = await serverMock.RequestData(startIndex, count, token);       
        SetNotes(notesData, count);        
    }
  
    void SetNotes(IList<DataItem> notesData, int size)
    {
        for (int i=0; i<size; i++)
        {
            SetNote(notesData[i],notesGO[i], notesText[i], notesBadge[i], notesGlow[i]);
            LoadingGO.SetActive(false);
        }
    }
          
    void SetNote(DataItem noteData,GameObject noteBox, TextMeshProUGUI textArea, SpriteRenderer badgeRenderer, SpriteRenderer glowRenderer)
    {
        noteBox.SetActive(true);
        SetDescription(textArea, noteData.Description);
        SetCategory(badgeRenderer, noteData.Category);
        if(noteData.Special) SetSpecial(glowRenderer);        
    }

    void CloseAllBoxes()
    {
        foreach (GameObject go in notesGO) go.SetActive(false);
        foreach (SpriteRenderer sr in notesGlow) sr.sprite = null;
        LoadingGO.SetActive(true);
    }

    public void NextNotes()
    {
        int nextCount = notesSize;
        if ((notesCount - (noteIndex + notesSize)) < notesSize) nextCount = notesCount - (noteIndex + notesSize);
        noteIndex += notesSize;
        CloseAllBoxes();
        GetNotes(noteIndex, nextCount);
    }

    public void PreviousNotes()
    {
        noteIndex -= notesSize;
        CloseAllBoxes();
        GetNotes(noteIndex, notesSize);
    }

    void ButtonUpdate()
    {
        if (noteIndex == 0) btnPrevious.interactable = false;
        else btnPrevious.interactable = true;
        if ((noteIndex + notesSize) <= notesCount) btnNext.interactable = true;
        else btnNext.interactable = false;
    }

    void SetSpecial(SpriteRenderer specialRenderer)
    {
        specialRenderer.sprite = glowSprite;
        specialRenderer.size = new Vector2(9f,2f);
    }
    
    void SetDescription(TextMeshProUGUI textArea, string text)
    {
        textArea.text = text;
    }

    void SetCategory(SpriteRenderer categoryRenderer, DataItem.CategoryType categoryType)
    {
        switch(categoryType)
        {
            case DataItem.CategoryType.RED:
                categoryRenderer.sprite = badgeRedSprite;
                break;
            case DataItem.CategoryType.GREEN:
                categoryRenderer.sprite = badgeGreenSprite;
                break;
            case DataItem.CategoryType.BLUE:
                categoryRenderer.sprite = badgeBlueSprite;
                break;
        }
    }

    void GetGlow()
    {
        for (int i = 0; i < notesGO.Length; i++)
        {
            notesGlow[i] = notesGO[i].transform.GetChild(notesGO[i].transform.childCount - 1).gameObject.GetComponent<SpriteRenderer>();
        }
    }

    void GetBadge()
    {
        for (int i = 0; i < notesGO.Length; i++)
        {
            notesBadge[i] = notesGO[i].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        }
    }

    void GetText()
    {
        for (int i = 0; i < notesGO.Length; i++)
        {
            notesText[i] = notesGO[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        }
    }
}