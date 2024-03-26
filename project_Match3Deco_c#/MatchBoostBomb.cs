using devRiot.Scene;
using devRiot.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace devRiot.Match
{
    //부스트 폭탄이 표현되는 씬오브젝트
    //OnFinished에서 실제 부스트의 기능이 구현된다.
    public class MatchBoostBomb : MonoBehaviour
    {
        private LevelManager _levelManager;
        private Square _square;

        public void Init(LevelManager levelManager, Square square)
        {
            _levelManager = levelManager;
            _square = square;
        }

        public void ShowEffect()
        {
            SoundManager.instance.PlaySoundEffect("sound_bomb_001");

            ParticleManager.instance.SpawnParticleEffect("fx_match_boom", transform.position, 3f);
        }

        public void OnFinished()
        {
            //부스트선택 초기화
            _levelManager.BoostManager.ActiveBoost = BoostType.None;

            //타겟블럭 주변 블럭 찾아서 파괴.
            List<Square> listBlocks = _square.GetAllNeghbors();
            foreach(Square square in listBlocks)
            {
                if(square.Item != null)
                {
                    square.Item.DestroyItem();
                }
            }

            //타겟블럭 파괴.
            _square.Item.DestroyItem();

            //재매칭.
            _levelManager.StartCoroutine(_levelManager.FindMatchDelay());

            Destroy(gameObject);
        }
    }
}