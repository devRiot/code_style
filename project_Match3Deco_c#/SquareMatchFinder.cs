using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace devRiot.Match
{
    //사각형 2x2 매칭을 찾는걸 구현한다.
    //사각형 모양은 확장타입은 찾지 않는다.
    public class SquareMatchFinder : IMatchFinder
    {
        public List<MatchedShapeInfo> FindMatches(LevelManager levelManager)
        {
            List<MatchedShapeInfo> matchedGroups = new List<MatchedShapeInfo>();

            int maxCols = levelManager.maxCols;
            int maxRows = levelManager.maxRows;

            // 가로 방향부터 시작.
            for (int y = 0; y < maxRows; y++)
            {
                List<MatchBaseBlock> rowMatches = new List<MatchBaseBlock>();

                for (int x = 0; x < maxCols; x++)
                {
                    MatchBaseBlock sl = levelManager.gameField.GetSquare(x, y)?.Item;
                    MatchBaseBlock sr = levelManager.gameField.GetSquare(x + 1, y)?.Item;
                    MatchBaseBlock sb = levelManager.gameField.GetSquare(x, y + 1)?.Item;
                    MatchBaseBlock slr = levelManager.gameField.GetSquare(x + 1, y + 1)?.Item;

                    if (sl == null || sr == null || sb == null || slr == null)
                        continue;

                    if (sl.IsMatchable() == false || sr.IsMatchable() == false || sb.IsMatchable() == false || slr.IsMatchable() == false)
                    {
                        rowMatches.Clear();
                        continue;
                    }

                    if(sl.UnitIndex == sr.UnitIndex && 
                        sl.UnitIndex == sb.UnitIndex &&
                        sl.UnitIndex == slr.UnitIndex)
                    {
                        rowMatches.Clear();
                        rowMatches.Add(sl);
                        rowMatches.Add(sr);
                        rowMatches.Add(sb);
                        rowMatches.Add(slr);

                        MatchedShapeInfo matchedShapeInfo = new MatchedShapeInfo();
                        matchedShapeInfo.blocks = new List<MatchBaseBlock>(rowMatches);
                        matchedShapeInfo.hCount = 2;
                        matchedShapeInfo.vCount = 2;
                        matchedGroups.Add(matchedShapeInfo);
                    }
                }
            }

            return matchedGroups;
        }
    }
}