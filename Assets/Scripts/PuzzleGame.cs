using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PuzzleMakerTwo;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class PuzzleGame : MonoBehaviour
{
    [SerializeField] private TextAsset _puzzleMakerJsonOutput;
    [SerializeField] private List<PuzzlePiece> _puzzlePieces;
    private PuzzleInfo _puzzleInfo;
    
    private void Start()
    {
        if (_puzzleInfo == null)
        {
            //_puzzleInfo = JsonUtility.FromJson(_puzzleMakerJsonOutput.text, typeof(PuzzleInfo)) as PuzzleInfo;
            _puzzleInfo = JsonConvert.DeserializeObject<PuzzleInfo>(_puzzleMakerJsonOutput.text);
        }

        foreach (var puzzlePiece in _puzzlePieces)
        {
            var name = puzzlePiece.gameObject.name;
            //var pieceNumber = (int)Char.GetNumericValue(name[name.Length-1]); 
            var pieceNumber = int.Parse(name.Remove(0, _puzzleInfo.PieceCountPrefix.Length - 1));
            
            var pieceData = _puzzleInfo.PuzzlePieceDatas.FirstOrDefault(t => t.PieceNumber == pieceNumber);
            puzzlePiece.Bind(pieceData);
            Instantiate(puzzlePiece.gameObject, new Vector3(pieceData.x, pieceData.y, 0), quaternion.identity);
        }
    }

    private void Update()
    {
        
            foreach (var puzzlePiece in _puzzlePieces)
            {
                puzzlePiece.gameUpdate();
            }
    }
}
