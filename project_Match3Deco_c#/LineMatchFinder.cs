using System.Collections;
using System.Collections.Generic;

namespace devRiot.Match
{
    public class LineMatchFinder : IMatchFinder
    {
        public List<MatchedShapeInfo> FindMatches(LevelManager levelManager)
        {
            List<MatchedShapeInfo> matchedGroups = new List<MatchedShapeInfo>();

            int maxCols = levelManager.maxCols;
            int maxRows = levelManager.maxRows;

            // 가로 방향 매칭 찾기
            for (int y = 0; y < maxRows; y++)
            {
                List<MatchBaseBlock> rowMatches = new List<MatchBaseBlock>();
                int lastIndex = -1;

                for (int x = 0; x < maxCols; x++)
                {
                    Square currentBlock = levelManager.gameField.GetSquare(x, y);
                    if (currentBlock.Item == null)
                        continue;
                    if (currentBlock.Item.IsMatchable() == false)
                    {
                        //매칭 불가능한 블럭이 나타나면 기존 매칭된 블럭 초기화 시켜준다.
                        if (rowMatches.Count < 3)
                            rowMatches.Clear();
                        continue;
                    }

                    if (currentBlock.Item?.UnitIndex == lastIndex)
                    {
                        if (!rowMatches.Contains(currentBlock.Item))
                            rowMatches.Add(currentBlock.Item);
                    }
                    else
                    {
                        if (rowMatches.Count >= 3)
                        {
                            MatchedShapeInfo matchedShapeInfo = new MatchedShapeInfo();
                            matchedShapeInfo.blocks = new List<MatchBaseBlock>(rowMatches);
                            matchedShapeInfo.hCount = rowMatches.Count;
                            matchedGroups.Add(matchedShapeInfo);
                        }

                        lastIndex = currentBlock.Item.UnitIndex;
                        rowMatches.Clear();
                        rowMatches.Add(currentBlock.Item);
                    }
                }

                if (rowMatches.Count >= 3)
                {
                    MatchedShapeInfo matchedShapeInfo = new MatchedShapeInfo();
                    matchedShapeInfo.blocks = new List<MatchBaseBlock>(rowMatches);
                    matchedShapeInfo.hCount = rowMatches.Count;
                    matchedGroups.Add(matchedShapeInfo);
                }
            }

            //세로 방향 매칭 찾기.
            for (int x = 0; x < maxCols; x++)
            {
                List<MatchBaseBlock> rowMatches = new List<MatchBaseBlock>();
                int lastIndex = -1;

                for (int y = 0; y < maxRows; y++)
                {
                    Square currentBlock = levelManager.gameField.GetSquare(x, y);
                    if (currentBlock.Item == null)
                        continue;
                    if (currentBlock.Item.IsMatchable() == false)
                    {
                        if(rowMatches.Count < 3)
                            rowMatches.Clear();
                        continue;
                    }

                    if (currentBlock.Item?.UnitIndex == lastIndex)
                    {
                        if (!rowMatches.Contains(currentBlock.Item))
                            rowMatches.Add(currentBlock.Item);
                    }
                    else
                    {
                        if (rowMatches.Count >= 3)
                        {
                            MatchedShapeInfo matchedShapeInfo = new MatchedShapeInfo();
                            matchedShapeInfo.blocks = new List<MatchBaseBlock>(rowMatches);
                            matchedShapeInfo.vCount = rowMatches.Count;
                            matchedGroups.Add(matchedShapeInfo);
                        }

                        lastIndex = currentBlock.Item.UnitIndex;
                        rowMatches.Clear();
                        rowMatches.Add(currentBlock.Item);
                    }
                }

                if (rowMatches.Count >= 3)
                {
                    MatchedShapeInfo matchedShapeInfo = new MatchedShapeInfo();
                    matchedShapeInfo.blocks = new List<MatchBaseBlock>(rowMatches);
                    matchedShapeInfo.vCount = rowMatches.Count;
                    matchedGroups.Add(matchedShapeInfo);
                }
            }

            return matchedGroups;
        }
    }
}