using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using devRiot.GameLogic;
using devRiot.Scene;
using devRiot.Spec;
using devRiot.AdMob;
using GoogleMobileAds.Api;
using devRiot.Analytics;

namespace devRiot.UI
{
    //로비 카페씬을 표현하는 팝업.
    public class LobbyCafePopup : PopupBase
    {
        //UI
        public Button buttonExit;
        public Button buttonBuyGem;
        public Button buttonBuyAds;
        public TextMeshProUGUI textBuyGemCount;
        public TextMeshProUGUI textSelectItemName;
        public TextMeshProUGUI textSelectItemDesc;
        public TextMeshProUGUI textBonusDesc;
        public Image imageNpc;
        public Image imageNpcTextBG;
        public TextMeshProUGUI textNpcSpeech;
        public LobbyCafePurchaseItemMessagePopup purchasePopup;
        public List<LobbyCafeItemWidget> itemWidgetList;

        //현재 선택된 아이템
        private LobbyCafeItemWidget _selectedItem = null;

        public override void Init(SceneBase scene)
        {
            base.Init(scene);

            if(buttonExit != null)
            {
                buttonExit.onClick.AddListener(Close);
            }

            foreach(var item in itemWidgetList)
            {
                item.Init(this);
            }

            if(buttonBuyGem != null)
            {
                buttonBuyGem.onClick.AddListener(OnClickBuyGem);
            }

            if( buttonBuyAds != null)
            {
                buttonBuyAds.onClick.AddListener(OnClickPurchaseOkByAds);
            }

            if(purchasePopup != null)
            {
                purchasePopup.Init(_scene);
            }
        }

        public override void Close()
        {
            base.Close();

            //애드몹 배너 숨기기
            GoogleAdMobManager.instance.HideBannerViewAd2();

            //씬에 카페팝업 닫힘을 노티한다.
            DecoScene decoScene = (DecoScene)_scene;
            decoScene.OnCloseLobbyCafePopup();

            //튜토리얼씬
            _scene.SceneManager._tutorialScene.gameObject.SetActive(true);
        }

        public override void Open()
        {
            base.Open();

            //애드몹 배너
            GoogleAdMobManager.instance.ShowBannerViewAd2();

            if(imageNpc != null)
            {
                imageNpc.GetComponent<Jun_TweenRuntime>().Play(OnShowNpcTextbox);
                imageNpcTextBG.gameObject.SetActive(false);
            }

            if(textNpcSpeech != null)
            {
                textNpcSpeech.text = GameDataManager.Instance.GetString("ui_message_cafe_npc_speech1");
                textNpcSpeech.gameObject.SetActive(false);
            }

            //카페 아이템리스트를 불러온다.
            List<SpecShopCafeItemInfo> itemInfoList = GameDataManager.Instance.CafeItemList;
            for(int i=0; i< itemInfoList.Count; i++)
            {
                itemWidgetList[i].SetItem(itemInfoList[i]);
                itemWidgetList[i].Open();
            }

            //광고시청 시 보너스리워드 세팅(일단 모두 공통)
            SpecMatchShopItemInfo adsBonusInfo = GameShopItemHelper.GetShopItemInfo("ads_reward_bonus_gem");
            if (adsBonusInfo != null && textBonusDesc != null)
            {
                textBonusDesc.text = string.Format("+{0} Get!", adsBonusInfo.reward_value);
            }

            //첫번째 아이템 기본선택.
            OnSelectItem(itemWidgetList[0]);

            //튜토리얼씬 숨기기
            _scene.SceneManager._tutorialScene.gameObject.SetActive(false);
        }

        void OnShowNpcTextbox()
        {
            imageNpcTextBG.gameObject.SetActive(true);
            textNpcSpeech.gameObject.SetActive(true);
        }

        public void OnSelectItem(LobbyCafeItemWidget selectedItem)
        {
            Debug.Log("select item : " + selectedItem.name);

            //포커스 리셋
            foreach(var item in itemWidgetList)
            {
                item.SelectItem = false;
            }

            selectedItem.SelectItem = true;
            _selectedItem = selectedItem;

            //선택된 아이템 정보 셋팅
            textSelectItemName.text = GameDataManager.Instance.GetString(selectedItem.SpecInfo.itemName + "_name");
            textSelectItemDesc.text = GameDataManager.Instance.GetString(selectedItem.SpecInfo.itemName + "_desc");

            //구매버튼 정보셋팅
            if (selectedItem.SpecInfo != null)
            {
                textBuyGemCount.text = selectedItem.SpecInfo.itemPrice.ToString();
            }
        }

        //카페 아이템을 구매한다.
        void OnClickBuyGem()
        {
            Debug.Log("OnClickBuyGem : " + _selectedItem.name);

            //재화가 부족한지 체크.
            if(PlayerInfo.Instance.Gem < _selectedItem.SpecInfo.itemPrice)
            {
                return;
            }

            //재화 소모시키기
            PlayerInfo.Instance.UpdateGem(_selectedItem.SpecInfo.itemPrice);

            //아이템 효과인 플레이어 버프를 생성시킨다.
            SpecShopCafeItemInfo itemInfo = _selectedItem.SpecInfo;
            if (itemInfo.itemType == CafeItemType.Buff_GoldUp)
            {
                PlayerInfo.Instance.CreatePlayerBuff(BuffType.MatchGoldUpRatio, itemInfo.propValue, itemInfo.timeValue);
            }
            else if (_selectedItem.SpecInfo.itemType == CafeItemType.Buff_MoveUp)
            {
                PlayerInfo.Instance.CreatePlayerBuff(BuffType.MatchMoveUpCount, itemInfo.propValue, itemInfo.timeValue);
            }

            //구매 팝업 띄우기
            if(purchasePopup != null)
            {
                string itemName = GameDataManager.Instance.GetString(_selectedItem.SpecInfo.itemName + "_name");
                purchasePopup.Icon = _selectedItem.SpecInfo.iconName;
                purchasePopup.Desc = string.Format(GameDataManager.Instance.GetString("message_match_purchased_boost"), itemName);
                purchasePopup.Open();
            }

            //애널리틱스 이벤트 추가
            UserAnalyticsManager.instance.OnEvent("purchaedCafeItemByGem");
        }

        //광고시청 하기 버튼 클릭
        void OnClickPurchaseOkByAds()
        {
            Debug.Log("OnClickPurchaseOkByAds");

            //애널리틱스 이벤트 추가
            UserAnalyticsManager.instance.OnEvent("purchaedCafeItemByAds");

            //이미 광고시청을 눌렀는지 체크.
            if (GoogleAdMobManager.instance.IsProcessShowAds)
            {
                Debug.Log("GoogleAdMobManager.instance.IsProcessShowAds == true");
                return;
            }

            //인터넷 연결 체크
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("error OnClickPurchaseOkByAds");
                return;
            }

            //애드몹 광고요청
            GoogleAdMobManager.instance.ShowRewardAd();

            StartCoroutine(CoProcessPurchaseByAds());


        }

        //광고시청 결과체크를 위한 프로세스
        IEnumerator CoProcessPurchaseByAds()
        {
            //광고표시중엔 대기한다.
            while (GoogleAdMobManager.instance.IsProcessShowAds)
            {
                yield return null;
            }

            //보상값 확인
            Reward resultReward = GoogleAdMobManager.instance.ResultReward;
            if (resultReward != null)
            {
                //아이템 효과인 플레이어 버프를 생성시킨다.
                SpecShopCafeItemInfo itemInfo = _selectedItem.SpecInfo;
                if (itemInfo.itemType == CafeItemType.Buff_GoldUp)
                {
                    PlayerInfo.Instance.CreatePlayerBuff(BuffType.MatchGoldUpRatio, itemInfo.propValue, itemInfo.timeValue);
                }
                else if (_selectedItem.SpecInfo.itemType == CafeItemType.Buff_MoveUp)
                {
                    PlayerInfo.Instance.CreatePlayerBuff(BuffType.MatchMoveUpCount, itemInfo.propValue, itemInfo.timeValue);
                }

                //-23.10.02
                // TODO : 밸런스테스트가 안되있음. 광고리워드 보상은 일단 임시.
                //광고시청으로 인한 보너스 리워드
                SpecMatchShopItemInfo bonusInfo = GameDataManager.Instance.GetMatchShopInfo("ads_reward_bonus_gem");
                if (bonusInfo != null)
                {
                    PlayerInfo.Instance.AddGem(bonusInfo.reward_value);
                }

                //광고보상 리셋
                GoogleAdMobManager.instance.ResetResultReward();

                //구매 팝업 띄우기
                if (purchasePopup != null)
                {
                    string itemName = GameDataManager.Instance.GetString(_selectedItem.SpecInfo.itemName + "_name");
                    purchasePopup.Icon = _selectedItem.SpecInfo.iconName;
                    purchasePopup.Desc = string.Format(GameDataManager.Instance.GetString("message_match_purchased_boost"), itemName);
                    purchasePopup.Open();
                }
            }

            yield break;
        }
    }
}