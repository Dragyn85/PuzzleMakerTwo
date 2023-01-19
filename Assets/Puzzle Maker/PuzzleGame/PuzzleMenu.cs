using PuzzleMakerTwo.GameExample;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PuzzleMenu : MonoBehaviour
{
    [SerializeField] List<Puzzle> puzzlePrefabs;
    [SerializeField] PuzzleSelectButton buttonPrefab;
    [SerializeField] Transform puzzleHolder;
    [SerializeField] PuzzleArea puzzleArea;
    [SerializeField] CanvasGroup MenuCanvasGroup;
    [SerializeField] CanvasGroup GameCanvasGroup;
    [SerializeField] float RestartDelay =3f;

    private void Awake()
    {
        PuzzleSelectButton.AnyPuzzleStarted += HandlePuzzleStart;
        foreach (var puzzle in puzzlePrefabs)
        {
            var newbutton = Instantiate(buttonPrefab, puzzleHolder.transform);
            newbutton.SetPuzzle(puzzle, () => { StartPuzzle(puzzle); });
        }
    }

    private void HandlePuzzleStart(Puzzle puzzle)
    {
        SetCanvasGroupShow(MenuCanvasGroup,false);
        SetCanvasGroupShow(GameCanvasGroup, true);
        Puzzle.PuzzleCompleted += HandleCompletedPuzzle;
    }

    private void HandleCompletedPuzzle(Puzzle puzzle)
    {
        Puzzle.PuzzleCompleted -= HandleCompletedPuzzle;
        StartCoroutine(ReturnToMenuAfterDelay(puzzle));
    }
    IEnumerator ReturnToMenuAfterDelay(Puzzle puzzle)
    {
        yield return new WaitForSeconds(RestartDelay);
        Destroy(puzzle.gameObject);
        SetCanvasGroupShow(MenuCanvasGroup,true);

    }

    private void StartPuzzle(Puzzle puzzle)
    {
       var newPuzzle = Instantiate(puzzle, puzzleArea.transform);
        var targetSize = puzzleArea.GetPuzzleAreaSize();
        newPuzzle.SetUniscaledSize(targetSize);
        newPuzzle.transform.position = Vector3.zero;
    }

    private void SetCanvasGroupShow(CanvasGroup canvasGroup,bool show)
    {
        canvasGroup.alpha = show? 1: 0;
        canvasGroup.blocksRaycasts = show;
        canvasGroup.interactable = show;
    }
}
