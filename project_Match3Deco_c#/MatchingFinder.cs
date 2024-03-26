using System.Collections;
using System.Collections.Generic;
using devRiot.Utill;

namespace devRiot.Match
{
    /// <summary>
    /// IMatchFinder로 구현된 매칭파인더들을 등록하여 3매치 및 특수조합을 찾는다.
    /// </summary>
    public class MatchingFinder : Singleton<MatchingFinder>
    {
        private LevelManager _levelManager;
        private List<IMatchFinder> matchFinders;

        public void Init(LevelManager levelManager)
        {
            _levelManager = levelManager;

            matchFinders = new List<IMatchFinder>
            {
                new LineMatchFinder(),
                new LMatchFinder(),
                new SquareMatchFinder()
            };
        }

        public bool FindMatches(MatchBaseBlock switchBlock, MatchBaseBlock switchBlock2)
        {
            bool isFind = false;
            foreach (IMatchFinder matchFinder in matchFinders)
            {
                List<MatchedShapeInfo> matchedGroups = matchFinder.FindMatches(_levelManager);

                // 매칭된 그룹 처리
                foreach (MatchedShapeInfo group in matchedGroups)
                {
                    if(group.Find(switchBlock))
                    {
                        foreach (MatchBaseBlock block in group.blocks)
                        {
                            isFind = true;
                            // 블럭 삭제 예약
                            _levelManager.AddReserveDestoryBlock(block);
                        }

                        //특수블럭 생성 예약
                        MakeSpecialBlockInfo makeSpecial = new(group, switchBlock2);
                        if (makeSpecial.blockType != BlockType.Unit)
                            _levelManager.createSpecialBlocks.Add(makeSpecial);
                    }

                    if (group.Find(switchBlock2))
                    {
                        foreach (MatchBaseBlock block in group.blocks)
                        {
                            isFind = true;
                            // 블럭 삭제 예약
                            _levelManager.AddReserveDestoryBlock(block);
                        }

                        //특수블럭 조건 체크
                        MakeSpecialBlockInfo makeSpecial = new(group, switchBlock);
                        if (makeSpecial.blockType != BlockType.Unit)
                            _levelManager.createSpecialBlocks.Add(makeSpecial);
                    }
                }
            }
            return isFind;
        }

        public bool FindMatches()
        {
            bool isFind = false;
            foreach (IMatchFinder matchFinder in matchFinders)
            {
                List<MatchedShapeInfo> matchedGroups = matchFinder.FindMatches(_levelManager);
                foreach (MatchedShapeInfo group in matchedGroups)
                {
                    foreach (MatchBaseBlock block in group.blocks)
                    {
                        isFind = true;
                        //블럭 삭제 예약
                        _levelManager.AddReserveDestoryBlock(block);
                    }

                    //특수블럭 생성 예약
                    MakeSpecialBlockInfo makeSpecial = new(group, null);
                    if (makeSpecial.blockType != BlockType.Unit)
                        _levelManager.AddReserveSpecialBlock(makeSpecial);
                }
            }
            return isFind;
        }
    }
}